package sequence;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.LinkedList;

interface GameInfo {
	GameCard getHand( int handIndex );
	UnitInfo getUnit( int unitIndex );
	int getAttributeTotal( String attr );
	int getEnergyTotal();
}

interface UItoGame extends GameInfo {
	boolean playCard( int cardIndex, int slotIndex );
	boolean actionsInQueue();
	void runNextAction();
}

interface Gameplay extends GameInfo {
	int modifyEnergy( int slotIndex, int amount );
	int addAttribute( int slotIndex, String attr, int amount );
	void destroyUnit( int slotIndex );
	void buildUnit( int slotIndex, UnitInfo unit ); //Replaces existing building if there is one
	void createHandCard( GameCard card, int slotIndex );
	void enqueueAction( Action a ); //Adds action to end queue	
	void pushAction( Action a ); //Adds action to start of queue
	void registerTrigger( Trigger t );
}

public class GameState implements UItoGame, Gameplay {
	public static final String 
	supplyAttr = "Supply",
	conduitAttr = "Conduit",
	constructionAttr = "Construction",
	energyMultiplierAttr = "Energy Multiplier";
	
	public static final int MAX_UNITS = 9;
	
	private ArrayList<GameCard> hand = new ArrayList<GameCard>();
	private GameUnit[] units = new GameUnit[MAX_UNITS];
	private LinkedList<Action> actionQueue = new LinkedList<Action>();
	private GametoUI ui;
	
	private GameState( GameCard[] startHand, GametoUI ui ) {
		for( GameCard c : startHand ) hand.add(c);
		this.ui = ui; 
	}
	
	public static UItoGame newGame( GameCard[] startHand, GametoUI ui ) {
		return new GameState(startHand, ui); 
	}
	
	public GameCard getHand( int handIndex ) {
		return hand.get( handIndex );
	}
	
	public GameUnit getUnit(int unitIndex) {
		return units[unitIndex];
	}
	
	public boolean playCard(int cardIndex, int slotIndex) {
		if( actionsInQueue() ) return false; //Cannot play cards until action queue is empty
		
		GameCard card = hand.get(cardIndex);
		if( !card.isPlayable(this, slotIndex) ) return false;
		hand.remove(cardIndex);
		card.play(this, slotIndex);
		
		if( card instanceof EventCard ) {
			//Run any PlayEventTriggers
			for( Trigger t : allTriggers ) if( t instanceof PlayEventTrigger ) {
				if( ((PlayEventTrigger)t).run(this, slotIndex, (EventCard)card) )
					pushAction( new TriggerAction( t.getAttachedSlot()) );
			}
		}
		
		return true;
	}
		
	public void runNextAction() {
		if( actionQueue.isEmpty() ) return;
		Action a = actionQueue.remove();
		if( a == null ) return;
		a.run(this);
		ui.onAction(a);
	}
	
	public boolean actionsInQueue() {
		return !actionQueue.isEmpty();
	}
	
	//list of triggers
	private LinkedList<Trigger> allTriggers = new LinkedList<Trigger>();
	
	public void registerTrigger( Trigger t ) { allTriggers.push( t ); } 
	
	public int modifyEnergy(int slotIndex, int amount) {
		if( units[slotIndex] == null ) return 0;
		
		//Multiply by the Energy Multiplier
		Integer eMult = units[slotIndex].getAttrValue(energyMultiplierAttr);
		if( eMult != null && amount > 0 ) amount *= eMult;
		
		//Save the old energy amount for comparison
		int oldAmount = units[slotIndex].energy;
				
		//Can't go below zero; recalculate amount to represent the actual change
		units[slotIndex].energy += amount;
		if( units[slotIndex].energy < 0 ) units[slotIndex].energy = 0;
		amount = units[slotIndex].energy - oldAmount;
		ui.onModifyEnergy( slotIndex, amount );
		
		//Run any ModifyEnergyTriggers
		for( Trigger t : allTriggers ) if( t instanceof ModifyEnergyTrigger ) {
			if( ((ModifyEnergyTrigger)t).run(this, slotIndex, oldAmount) )
				pushAction( new TriggerAction( t.getAttachedSlot()) );
		}
			
		return amount;
	}
	
	private void createUnit(final int slotIndex, UnitInfo unit) {
		if( unit == null ) return;
		
		GameUnit u = new GameUnit();
		u.energy = unit.getEnergy();
		u.originCard = unit.originCard();
		u.name = unit.getName();
		
		for( String attr : unit.attrSet() ) 
			u.addAttribute(attr, unit.getAttrValue(attr));
		
		units[slotIndex] = u;
		ui.onCreateUnit(slotIndex, unit);
		
		
		//Register triggers attached to this card
		for( Trigger t : u.originCard.getTriggers() ) {
			Trigger tCopy = t.makeCopyAtIndex(slotIndex);
			registerTrigger(tCopy);
		}
		
		//The following actions will occur in reverse order (LinkedList.push):
		
		//CreateUnit triggers (other than supply/conduit)
		for( Trigger t : allTriggers ) if( t instanceof CreateUnitTrigger ) 
			if( ((CreateUnitTrigger)t).run( this, slotIndex ) )
				pushAction( new TriggerAction( t.getAttachedSlot()) );
		
		//Conduit
		pushAction( new Action() {
			public void run(Gameplay g) {
				for( int i = 0; i < GameState.MAX_UNITS; ++i ) {
					if( i == slotIndex || units[i] == null ) continue;
					Integer conduitValue = units[i].getAttrValue(conduitAttr);
					if( conduitValue != null && conduitValue != 0 )
						g.modifyEnergy( i, conduitValue );
				}
			}
		});
		
		//Supply
		pushAction( new Action() {
			public void run( Gameplay g ) {
				LinkedList<Integer> supplySlots = new LinkedList<Integer>();
				int supplyTotal = 0;
				for( int i = 0; i < GameState.MAX_UNITS; ++i ) {
					if( i == slotIndex || units[i] == null ) continue;
					Integer supplyValue = units[i].getAttrValue(supplyAttr);
					if( supplyValue != null && supplyValue != 0 ) {
						supplySlots.add(i);
						supplyTotal += supplyValue;
					}
				}
			
				if( !supplySlots.isEmpty() ) {
					pushAction( new ModifyEnergyAction(slotIndex, supplyTotal ) );
					pushAction( new TriggerAction(supplySlots.toArray(new Integer[0])) );
				}
			}
		});
	}
	
	public void buildUnit( final int slotIndex, final UnitInfo unit ) {
		if( units[slotIndex] == null ) { 
			pushAction( new Action() {
				public void run( Gameplay g ) {
					createUnit( slotIndex, unit );
				}
			});
		} else {
			//Action from UPGRADE-ing over the previous unit
			final Action upgradeAction = units[slotIndex].originCard.onUpgrade(this, slotIndex);
			
			pushAction( new Action() {
				public void run( Gameplay g ) {
					createUnit( slotIndex, unit );
					pushAction(upgradeAction);				
				}
			});
			pushAction( new DestroyUnitAction( slotIndex ) );
			if( upgradeAction != null ) pushAction( new TriggerAction( slotIndex ) );
		}
	}
	
	public void enqueueAction(Action a) {
		actionQueue.add(a);
	}
	
	public void pushAction( Action a ) {
		actionQueue.push(a);
	}
	
	public void destroyUnit(final int slotIndex) {
		if( units[slotIndex] == null ) return;
		
		//Unregister any triggers attached to this unit
		Iterator<Trigger> iter = allTriggers.iterator();
		while( iter.hasNext() ) {
			Trigger t = iter.next();
			if( t.getAttachedSlot() == slotIndex ) iter.remove();
		}
		
		//Run any DestroyUnitTriggers
		for( Trigger t : allTriggers ) if( t instanceof DestroyUnitTrigger ){
			if( ((DestroyUnitTrigger)t).run( this, slotIndex ) )
				pushAction( new TriggerAction( t.getAttachedSlot() ) ); 
		}

		pushAction( new Action() {
			public void run( Gameplay g ) {
				units[slotIndex] = null;
				ui.onDestroyUnit( slotIndex );
			}
		});
	}

	public int addAttribute(int slotIndex, String attr, int amount) {
		if( units[slotIndex] == null ) return 0;
		units[slotIndex].addAttribute(attr, amount);
		return amount;
	}
	
	public void createHandCard( GameCard card, int slotIndex ) {
		hand.add(card);
		ui.onNewHandCard(card, slotIndex);
	}

	public int getAttributeTotal(String attr) {
		int total = 0;
		for( int i = 0; i < GameState.MAX_UNITS; ++i ) {
			if( units[i] == null ) continue;
			Integer value = units[i].getAttrValue(attr);
			if( value != null ) total += value;
		}
		return total;
	}
	
	public int getEnergyTotal() {
		int total = 0;
		for( int i = 0; i < GameState.MAX_UNITS; ++i ) {
			if( units[i] == null ) continue;
			total += units[i].getEnergy();
		}
		return total;
	}
}

