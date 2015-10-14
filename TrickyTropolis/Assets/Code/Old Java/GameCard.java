package sequence;

import java.util.HashMap;
import java.util.LinkedList;
import java.util.Map;
import java.util.Set;

abstract class GameCard {
	public final String name;
	public String getName() { return name; }
		
	abstract public boolean isPlayable( GameInfo game, int slotIndex ); 
	public GameCard( String n, String c ) { name = n; code = c; }
	abstract public void play( Gameplay g, int slotIndex );
	
	private LinkedList<String> handText = new LinkedList<String>();
	private LinkedList<String> boardText = new LinkedList<String>();
	public final String code;
	public String[] textLines( boolean inHand ) { 
		if(inHand) return handText.toArray(new String[0]);
		else return boardText.toArray(new String[0]);
	}
	public GameCard addTextLine( String s, boolean board ) {
		handText.add(s);
		if( board ) boardText.add(s);
		return this;
	}
	public GameCard addTextLine( String s ) { return addTextLine(s, false); }
}

class BuildingCard extends GameCard implements UnitInfo {
	public final int energyCost;
	public final int startingEnergy;
	
	public int getEnergy() { return startingEnergy;	}
	public BuildingCard originCard() { return this; }	
	public Integer getAttrValue(String attr) { return attributes.get(attr); }
	public Set<String> attrSet() { return attributes.keySet(); }
	
	private Map<String, Integer> attributes = new HashMap<String, Integer>();
	
	public BuildingCard( String n, String code, int c, int s ) { 
		super(n, code);
		energyCost = c; startingEnergy = s;
		allBuildings.add(this);
	}
	
	//Override for additional "battlecry" effect
	public void play( Gameplay g, int slotIndex ) { 
		g.buildUnit( slotIndex, this );
	}
	
	//"UPGRADE" effect: return an action to be played just after another building 
	//gets built over this one
	
	public Action onUpgrade( Gameplay g, int slotIndex ) { return null; } 
	
	public boolean isPlayable( GameInfo game, int slotIndex ) {
		int cost = energyCost - game.getAttributeTotal(GameState.constructionAttr);
		UnitInfo u = game.getUnit(slotIndex);
		if( u == null ) return cost <= 0;
		return cost <= u.getEnergy();
	}
	
	public BuildingCard addAttribute( String attr, int amount ) {
		attributes.put(attr, amount);
		return this;
	}
	
	private LinkedList<Trigger> triggers = new LinkedList<Trigger>(); 
	public BuildingCard addTrigger( Trigger t ) {
		triggers.add(t);
		return this;
	}
	public Trigger[] getTriggers() { return triggers.toArray(new Trigger[0]); } 
		
	private static final LinkedList<BuildingCard> allBuildings = new LinkedList<BuildingCard>();
	public static BuildingCard[] list() { return allBuildings.toArray(new BuildingCard[allBuildings.size()]); }
	
	
	//Building cards
	public static final GameCard 
	warehouse = new BuildingCard("Warehouse", "w", 0, 2 ),
	
	databank = new BuildingCard("Databank", "d", 0, 1 ) {
		public Action onUpgrade( Gameplay g, int slotIndex ) {
			return new ModifyEnergyAction( slotIndex, 1 );
		}
	}.addTextLine( "UPGRADE: +1 Energy.", true ),
	
	industrialZone = new BuildingCard( "Industrial Zone", "i", 0, 0 ) {
		public void play( Gameplay g, final int slotIndex ) {
			g.buildUnit(slotIndex, this);
			g.enqueueAction( new Action() {
				public void run( Gameplay g ) {
					int nextAvail;
					for( nextAvail = slotIndex + 1; nextAvail != slotIndex; ++nextAvail ) {
						if( nextAvail == GameState.MAX_UNITS ) nextAvail = 0;
						if( g.getUnit(nextAvail) == null ) {
							g.buildUnit( nextAvail, (BuildingCard)industrialZone );
							return;
						}
					}
				}
			});
		}
	}.addTextLine( "Create two copies.", false ),
	
	storageCell = new BuildingCard("Storage Cell", "sc", 0, 1 ) {
		public void play( Gameplay g, final int slotIndex )
		{
			super.play(g, slotIndex);
			g.enqueueAction( new Action() { 
				public void run( Gameplay g ) {
					int transferred = 0;
					for( int i = 0; i < GameState.MAX_UNITS; ++i ) {
						if( i == slotIndex ) continue;
						transferred -= g.modifyEnergy( i, -1 );
					}
					g.pushAction( new ModifyEnergyAction( slotIndex, transferred) );
				}
			});
		}
	}.addTextLine( "Each building transfers 1 Energy to this one." ),
	
	substation = new BuildingCard("Substation", "s", 1, 0)
		.addAttribute( GameState.conduitAttr, 1 ),
		
	constructionYard = new BuildingCard("Construction Yard", "cy", 3, 1 )
		.addAttribute( GameState.constructionAttr, 1),
		
	chargingStation = new BuildingCard("Charging Station", "cs", 2, 3) {
		public void play( Gameplay g, final int slotIndex ) {
			super.play(g, slotIndex);
			g.enqueueAction( new Action() {
				public void run( Gameplay g2 ) {
					g2.createHandCard(EventCard.energyBoost, slotIndex);
				}
			});
		}
	}.addTextLine( "Draw a +1 Energy Boost.", false ),
	
	reactorCore = new BuildingCard("Reactor Core", "rc", 2, 2) {
		public Action onUpgrade( Gameplay g, int slotIndex ) {
			return new AddAttributeAction( slotIndex, GameState.supplyAttr, 1 );
		}
	}.addTextLine( "UPGRADE: Supply", true ),
	
	windTurbine = new BuildingCard( "Wind Turbine", "wt", 3, 3 )
		.addAttribute( GameState.supplyAttr, 1 ),
		
	researchFacility = new BuildingCard( "Research Facility", "rf", 6, 6 )
		.addAttribute( GameState.energyMultiplierAttr, 2 ),
		
	solarArray = new BuildingCard( "Solar Array", "sa", 4, 4 ) {
		public void play( Gameplay g, int slot ) {
			super.play(g, slot);
			g.enqueueAction( new PowerUpAction() );
		}
	}.addTextLine("+1 to all your buildings.", false ),
	
	techLab = new BuildingCard( "Tech Lab", "tl", 1, 0 ) 
	.addTrigger( new PlayEventTrigger(-1) { //This trigger begins with no attached slot
		public boolean run( Gameplay g, int slotIndex, EventCard ec ) {
			g.pushAction( new ModifyEnergyAction(getAttachedSlot(), 2) );
			return true;
		}
	})
	.addTextLine( "Whenever an Event card is played, +2.", true ),
	
	coalPlant = new BuildingCard( "Coal Plant", "cp", 5, 5 )
	.addTrigger( new ModifyEnergyTrigger(-1) {
		public boolean run( Gameplay g, int slotIndex, int oldAmount ) {
			if( slotIndex != getAttachedSlot() || g.getUnit(slotIndex).getEnergy() != 0 ) 
				return false;
			g.pushAction( new DestroyUnitAction(getAttachedSlot()) );
			return true;
		}
	})
	.addAttribute( GameState.supplyAttr, 2 )
	.addAttribute( GameState.conduitAttr, -1 )
	.addTextLine( "If this building reaches 0, destroy it.", true ),
	
	factory = new BuildingCard("Factory", "f", 5, 4) {
		public void play( Gameplay g, final int slotIndex ) {
			super.play(g, slotIndex);
			g.enqueueAction( new Action() {
				public void run( Gameplay g2 ) {
					g2.createHandCard(BuildingCard.warehouse, slotIndex);
				}
			});
		}
	}.addTextLine( "Draw a Warehouse.", false ),
	
	machineShop = new BuildingCard("Machine Shop", "ms", 4, 5 ) {		
		public void play( Gameplay g, final int slotIndex ) {
			super.play(g, slotIndex);
			g.enqueueAction( new Action() {
				public void run( Gameplay g2 ) {
					g2.createHandCard(EventCard.conduit, slotIndex);
				}
			});
		}
	}.addTextLine( "Draw a Conduit card.", false ),
	
	skyscraper = new BuildingCard("Skyscraper", "sk", 7, 11 );
}


abstract class EventCard extends GameCard {
	public EventCard( String n, String code ) {
		super(n, code);
		allEvents.add(this);
	}
	public boolean isPlayable( GameInfo g, int slotIndex ) {
		//by default, event cards can be played on any building
		return g.getUnit(slotIndex) != null;
	}
	private static final LinkedList<EventCard> allEvents = new LinkedList<EventCard>();
	public static final EventCard[] list() { return allEvents.toArray( new EventCard[allEvents.size()]); } 
	
	//Event cards
	public static final GameCard
	energyBoost = new EventCard( "Energy Boost", "eb" ) {
		public void play( Gameplay g, int slotIndex ) {
			g.modifyEnergy(slotIndex, 1);
		}
	}.addTextLine( "+1 Energy" ),
	
	powerSurge = new EventCard( "Power Surge", "ps" ) {
		public void play( Gameplay g, int slotIndex ) {
			g.modifyEnergy(slotIndex, 2);
		}
	}.addTextLine( "+2 Energy" ),
	
	overload = new EventCard( "Overload", "o" ) {
		public void play( Gameplay g, int slotIndex ) {
			g.modifyEnergy(slotIndex, 3);
		}
	}.addTextLine( "+3 Energy" ),
	
	powerUp = new EventCard( "Power-Up", "pu" ) {
		public void play( Gameplay g, int slotIndex ) {
			g.enqueueAction( new PowerUpAction() );
		}
		public boolean isPlayable( GameInfo g, int slotIndex ) { return true; }
	}.addTextLine( "+1 to all your buildings." ),
	
	conduit = new EventCard( "Conduit", "c" ) {
		public void play( Gameplay g, int slotIndex ) {
			g.addAttribute(slotIndex, GameState.conduitAttr, 1 );
		}
	}.addTextLine( "Give a building Conduit." );
}