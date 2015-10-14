using UnityEngine;
using System.Collections.Generic;

public interface IGameInfo {
	GameCard getHand( int handIndex );
	IUnitInfo getUnit( int unitIndex );
	int getAttributeTotal( string attr );
	int getEnergyTotal();
}

public interface IUItoGame : IGameInfo {
	bool playCard( int cardIndex, int slotIndex );
	bool actionsInQueue();
	void runNextAction();
}

public interface IGametoUI {
	void onCreateUnit( int slotIndex, IUnitInfo u );
	void onModifyEnergy( int slotIndex, int amount );
	void onDestroyUnit( int slotIndex );
	void triggerFlash( IEnumerable<int> slots );
	void triggerFlash( int slot );
	void onNewHandCard( GameCard c, int slotIndex );
	void onAddAttribute( int slotIndex );
}

public interface IGameplay : IGameInfo {
	int modifyEnergy( int slotIndex, int amount );
	int addAttribute( int slotIndex, string attr, int amount=1 );
	void destroyUnit( int slotIndex );
	void buildUnit( int slotIndex, IUnitInfo unit ); //Replaces existing building if there is one
	void createHandCard( GameCard card, int slotIndex );
	void enqueueAction( Action a ); //Adds action to end queue	
	void pushAction( Action a ); //Adds action to start of queue
	void registerTrigger( Trigger t, int slotIndex );
}

public class BoardManager : IGameplay, IUItoGame {
	public const string 
		supplyAttr = "Supply",
		buildupAttr = "Buildup",
		constructionAttr = "Construction",
		energyMultiplierAttr = "Energy Multiplier",
		recycleAttr = "Recycle";
	
	public const int MAX_UNITS = 9;
	private IGametoUI ui;

	private List<GameCard> hand;
	private GameUnit[] units = new GameUnit[MAX_UNITS];
	private LinkedList<Action> actionQueue = new LinkedList<Action>();
		
	private BoardManager( GameCard[] startHand, IGametoUI ui ) {
		hand = new List<GameCard> ();
		foreach (GameCard c in startHand) { 
			if( c == null ) Debug.Log( "null card!" );
			hand.Add (c);
		}
		this.ui = ui; 
	}


	public static IUItoGame newGame( GameCard[] startHand, IGametoUI ui ) {
		return new BoardManager(startHand, ui); 
	}

	public GameCard getHand( int handIndex ) {
		return hand[handIndex];
	}
	
	public IUnitInfo getUnit(int unitIndex) {
		return units[unitIndex];
	}
	
	public bool playCard(int cardIndex, int slotIndex) {
		if( actionsInQueue() ) return false; //Cannot play cards until action queue is empty
		
		GameCard card = hand[cardIndex];
		if( !card.isPlayable(this, slotIndex) ) return false;
		hand.RemoveAt(cardIndex);
		card.play(this, slotIndex);

		if( card is EventCard ) {
			//Run any PlayEventTriggers
			foreach( Trigger t in allTriggers ) if( t is PlayEventTrigger ) {
				PlayEventTrigger captured = t as PlayEventTrigger;
				if( captured.test != null && !captured.test(this, t.attachedSlot, slotIndex, (EventCard)card) ) continue;
				pushAction( delegate( IGameplay g ) {
					byte flags = captured.run(this, captured.attachedSlot, slotIndex, (EventCard)card);
					if( (flags & Trigger.FLASH) != 0 ) ui.triggerFlash( captured.attachedSlot );
					if( (flags & Trigger.REMOVE) != 0 ) allTriggers.Remove( captured );
				});
			}
		}

		return true;
	}


	public void runNextAction() {
		if (actionQueue.Count == 0) return;
		Action a = actionQueue.First.Value;
		actionQueue.RemoveFirst ();
		if( a == null ) return;
		a(this);
	}
	
	public bool actionsInQueue() {
		return actionQueue.Count != 0;
	}


	//list of triggers
	private LinkedList<Trigger> allTriggers = new LinkedList<Trigger>();
	
	public void registerTrigger( Trigger t, int slot ) {
		Trigger clone = t.Clone () as Trigger;
		clone.attachedSlot = slot;
		allTriggers.AddFirst( clone ); 
	} 

	public int modifyEnergy(int slotIndex, int amount) {
		if( units[slotIndex] == null ) return 0;

        //Multiply by the Energy Multiplier
        if (amount > 0)
        {
            int? eMult = units[slotIndex].getAttrValue(energyMultiplierAttr);
            amount *= (eMult ?? 1);
        }
		
		//Save the old energy amount for comparison
		int oldAmount = units [slotIndex].energy;
		
		//Can't go below zero; recalculate amount to represent the actual change
		units[slotIndex].energy += amount;
		if( units[slotIndex].energy < 0 ) units[slotIndex].energy = 0;
		amount = units[slotIndex].energy - oldAmount;
		ui.onModifyEnergy( slotIndex, amount );
		
		//Run any ModifyEnergyTriggers
		foreach( Trigger t in allTriggers ) if( t is ModifyEnergyTrigger ) {
			ModifyEnergyTrigger captured = t as ModifyEnergyTrigger;
			if( captured.test != null && !captured.test(this, t.attachedSlot, slotIndex, oldAmount) ) continue;
			pushAction( delegate(IGameplay g) {
				byte flags = captured.run(this, captured.attachedSlot, slotIndex, oldAmount);
				if( (flags & Trigger.FLASH) != 0 ) ui.triggerFlash( captured.attachedSlot );
				if( (flags & Trigger.REMOVE) != 0 ) allTriggers.Remove( captured );
			});
		}
		
		return amount;
	}
	
	public void buildUnit(int slotIndex, IUnitInfo unit) {
		if (unit == null) return;

		TargetedAction upgradeAction = null;

		//Begin by destroying the old unit
		if( units[slotIndex] != null ) {
			upgradeAction = units[slotIndex].originCard.upgrader;
			if( upgradeAction != null ) ui.triggerFlash( new int[]{slotIndex} );
			destroyUnit( slotIndex );
			//If destroyed unit has an UPGRADE, it will trigger the UI flash as it's being destroyed
		}

		//Create the actual unit
		enqueueAction (delegate(IGameplay g) {
			GameUnit u = new GameUnit ();
			u.energy = unit.getEnergy ();
			u.originCard = unit.getOriginCard ();
			u.name = unit.getName ();
			
			//Add attributes attached to this card
			foreach (string attr in unit.getAttributes()) 
				u.addAttribute (attr, (int)unit.getAttrValue (attr));
			
			//Register triggers attached to this card
			foreach( Trigger t in unit.getOriginCard().getTriggers() ) 
				registerTrigger( t, slotIndex ); 
			
			units [slotIndex] = u;
			ui.onCreateUnit (slotIndex, u);
		});

		//Do the UPGRADE trigger
		if( upgradeAction != null ) 
			enqueueAction ( delegate(IGameplay g) {
				upgradeAction(g, slotIndex);
			});

		//Supply
		if (getAttributeTotal (BoardManager.supplyAttr) > 0)
		enqueueAction( delegate(IGameplay g) {
			List<int> supplySlots = new List<int>(BoardManager.MAX_UNITS);
			int supplyTotal = 0;
			for( int i = 0; i < BoardManager.MAX_UNITS; ++i ) {
				if( i == slotIndex || units[i] == null ) continue;
				int? supplyValue = units[i].getAttrValue(supplyAttr);
				if( supplyValue != null && supplyValue != 0 ) {
					supplySlots.Add(i);
					supplyTotal += (int)supplyValue;
				}
			}
			
			if( supplySlots.Count > 0 ) g.modifyEnergy(slotIndex, supplyTotal );
			ui.triggerFlash( supplySlots.ToArray() );
		});

		//buildup
		if( getAttributeTotal(BoardManager.buildupAttr) > 0 )
		enqueueAction( delegate(IGameplay g) {
			for( int i = 0; i < BoardManager.MAX_UNITS; ++i ) {
				if( i == slotIndex || units[i] == null ) continue;
				int? buildupValue = units[i].getAttrValue(buildupAttr);
				if( buildupValue != null && buildupValue != 0 )
					g.modifyEnergy( i, (int)buildupValue );
			}
		});
		
		//CreateUnit triggers (other than supply/buildup)
		foreach (Trigger t in allTriggers) if (t is CreateUnitTrigger) {
			CreateUnitTrigger captured = t as CreateUnitTrigger;
			if( captured.test != null && !captured.test(this, t.attachedSlot, slotIndex) ) continue;
			enqueueAction ( delegate(IGameplay g) {
				byte flags = captured.run( g, captured.attachedSlot, slotIndex);
				if( (flags & Trigger.FLASH) != 0 ) ui.triggerFlash( captured.attachedSlot );
				if( (flags & Trigger.REMOVE) != 0 ) allTriggers.Remove( captured );
			});
		}
	}

	public void enqueueAction(Action a) {
		actionQueue.AddLast (a);
	}
	
	public void pushAction( Action a ) {
		actionQueue.AddFirst(a);
	}
	

	public void destroyUnit(int slotIndex) {
		if( units[slotIndex] == null ) return;

		//Run any DestroyUnitTriggers
		foreach( Trigger t in allTriggers ) if( t is DestroyUnitTrigger ) {
			DestroyUnitTrigger captured = t as DestroyUnitTrigger;
			IUnitInfo unit = units[slotIndex];
			if( captured.test != null && !captured.test(this, t.attachedSlot, slotIndex, unit ) ) continue;
			pushAction ( delegate(IGameplay g) {
				byte flags = captured.run( this, captured.attachedSlot, slotIndex, unit );
				if( (flags & Trigger.FLASH) != 0 ) ui.triggerFlash( captured.attachedSlot );
				if( (flags & Trigger.REMOVE) != 0 ) allTriggers.Remove( captured );
			});
		}

		//Unregister any triggers attached to this unit
		LinkedListNode<Trigger> node = allTriggers.First;
		while (node != null) {
			if( node.Value.attachedSlot == slotIndex )
				allTriggers.Remove( node );
			node = node.Next;
		}

		//Destroy the unit
		units[slotIndex] = null;
		ui.onDestroyUnit( slotIndex );
	}
	
	public int addAttribute(int slotIndex, string attr, int amount=1) {
		if (units [slotIndex] == null)
			return 0;
		units[slotIndex].addAttribute(attr, amount);
		ui.onAddAttribute (slotIndex);
		return amount;
	}
	
	public void createHandCard( GameCard card, int slotIndex ) {
		hand.Add(card);
		ui.onNewHandCard(card, slotIndex);
	}
	
	public int getAttributeTotal(string attr) {
		int total = 0;
		for( int i = 0; i < BoardManager.MAX_UNITS; ++i ) {
			if( units[i] == null ) continue;
			int? value = units[i].getAttrValue(attr);
			total += value ?? 0;
		}
		return total;
	}
	
	public int getEnergyTotal() {
		int total = 0;
		for( int i = 0; i < BoardManager.MAX_UNITS; ++i ) {
			if( units[i] == null ) continue;
			total += units[i].getEnergy();
		}
		return total;
	}
}
