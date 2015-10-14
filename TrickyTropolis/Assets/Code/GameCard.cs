using UnityEngine;
using System.Collections.Generic;

public delegate bool PlayableTester( IGameInfo game, int slotIndex );
public delegate void TargetedAction( IGameplay game, int slotIndex );

public class GameCard {
	public readonly string name;
	public string getName() { return name; }

	//GameCards can only be constructed statically from within a derived class of GameCard
	static Dictionary<string, GameCard> allCards = new Dictionary<string, GameCard>();
	protected GameCard( string n, string c, TargetedAction a = null, PlayableTester t = null ) { 
		name = n; code = c; runner = a; tester = t;
		allCards.Add (c, this);
	}

	public static GameCard[] getAllCards() {
		GameCard[] cards = new GameCard[allCards.Count];
		allCards.Values.CopyTo( cards, 0 );
		return cards;
	}

	public readonly PlayableTester tester;
	public virtual bool isPlayable( IGameInfo game, int slotIndex ) {
		if (tester == null) return true;
		return tester (game, slotIndex);
	} 

	public readonly TargetedAction runner;
	virtual public void play( IGameplay g, int slotIndex ) {
		if (runner != null)
			runner (g, slotIndex);
	}
	
	public readonly string code;

	/*
	public string text;
	public GameCard setText( string s ) {
		text = s;
		return this;
	}*/

	public static GameCard findByCode( string code ) {
		GameCard card;
		if (!allCards.TryGetValue (code, out card))
			Debug.Log ("couldn't find card with code " + code);
		return card;
	}

	static GameCard() {
		BuildingCard.init();
		EventCard.init();
	}
}

public class BuildingCard : GameCard, IUnitInfo {
	internal static void init() {}

	public readonly int energyCost;
	public readonly int startingEnergy;

	private Dictionary<string, int> attrs = new Dictionary<string, int>();
	public List<Trigger> triggers = new List<Trigger>();

	public int getEnergy() { return startingEnergy;	}
	public BuildingCard getOriginCard() { return this; }	
	public int? getAttrValue(string attr) { 
		int val;
		if (attrs.TryGetValue (attr, out val))
			return val;
		return null;
	}
	public IEnumerable<string> getAttributes() { return attrs.Keys; }

	public readonly TargetedAction upgrader;
	protected BuildingCard( string n, string code, int cost, int start, 
			TargetedAction run = null, TargetedAction up = null, PlayableTester test = null ) : 
		base(n,code,run,test) { 
		energyCost = cost; startingEnergy = start; upgrader = up;
	}

	public override void play( IGameplay g, int slotIndex ) { 
		g.buildUnit( slotIndex, this );
		if (runner != null)
			g.enqueueAction (delegate(IGameplay g2) {
				runner (g, slotIndex);
			});
	}
	
	//"UPGRADE" effect: return an action to be played just after another building 
	//gets built over this one
	
	public override bool isPlayable( IGameInfo game, int slotIndex ) {
		int cost = energyCost - game.getAttributeTotal(BoardManager.constructionAttr);
		IUnitInfo u = game.getUnit(slotIndex);
		return cost <= ( u == null ? 0 : u.getEnergy() )
			&& base.isPlayable (game, slotIndex);
	}
	
	protected BuildingCard addAttribute( string attr, int amount ) {
		attrs.Add (attr, amount);
		return this;
	}

	protected BuildingCard addTrigger( Trigger t ) {
		triggers.Add (t);
		return this;
	}

	internal IEnumerable<Trigger> getTriggers() {
		return triggers;
	}

	//Building cards
	public static readonly BuildingCard 
	warehouse = new BuildingCard ("Warehouse", "w", 0, 2),

	databank = new BuildingCard ("Databank", "d", 0, 1, null, 
		delegate( IGameplay g, int slotIndex ) {
			g.modifyEnergy (slotIndex, 1);
		}).addAttribute( "UPGRADE: +1 energy", 1 ),
	
	industrialZone = new BuildingCard( "Industrial Zone", "iz", 0, 0, delegate( IGameplay g, int slotIndex ) {
		int nextAvail;
		for( nextAvail = slotIndex + 1; nextAvail != slotIndex; ++nextAvail ) {
			if( nextAvail == BoardManager.MAX_UNITS ) nextAvail = 0;
			if( g.getUnit(nextAvail) == null ) {
				g.buildUnit( nextAvail, industrialZone );
				return;
			}
		}
	}),

	distributionNetwork = new BuildingCard( "Distribution Network", "dn", 1, 2, 
		delegate( IGameplay g, int slotIndex) {
			List<int> buffSlots = new List<int>();
			int x = slotIndex % 3, y = slotIndex / 3;
			if( y > 0 ) buffSlots.Add( slotIndex - 3 );
			if( y < 2 ) buffSlots.Add( slotIndex + 3 );
			if( x > 0 ) buffSlots.Add( slotIndex - 1 );
			if( x < 2 ) buffSlots.Add( slotIndex + 1 );
			foreach( int slot in buffSlots ) 
				g.modifyEnergy( slot, 1 );
		}),

	storageCell = new BuildingCard("Storage Cell", "sc", 0, 1, 
		delegate(IGameplay g, int slotIndex) {
			int transferred = 0;
			for( int i = 0; i < BoardManager.MAX_UNITS; ++i ) {
				if( i == slotIndex ) continue;
                if( g.getUnit(i) != null && g.getUnit(i).getEnergy() > 0 )
				    transferred -= g.modifyEnergy( i, -1 );
			}
			g.pushAction( delegate(IGameplay g2) {
				g.modifyEnergy(slotIndex, transferred);
			});
		}),
	
	
	substation = new BuildingCard("Substation", "s", 1, 0)
		.addAttribute( BoardManager.buildupAttr, 1 ),
		
	constructionYard = new BuildingCard("Construction Yard", "cy", 3, 2 )
		.addAttribute( BoardManager.constructionAttr, 1),
			
	chargingStation = new BuildingCard ("Charging Station", "cs", 2, 3, 
		delegate( IGameplay g, int slotIndex ) {
			g.createHandCard (EventCard.energyBoost, slotIndex);
		}),
	
	reactorCore = new BuildingCard("Portable Generator", "pg", 2, 2, null, 
	    delegate(IGameplay g, int slotIndex ) {
			g.addAttribute( slotIndex, BoardManager.supplyAttr, 1 );
		})
		.addAttribute("UPGRADE: Supply", 1),
	
	scaffolding = new BuildingCard("Scaffolding", "sf", 2, 2, null, 
		delegate(IGameplay g, int slotIndex ) {
			g.addAttribute( slotIndex, BoardManager.buildupAttr, 1 );
		})
		.addAttribute("UPGRADE: Buildup", 1),
	
	windTurbine = new BuildingCard( "Wind Turbine", "wt", 3, 3 )
		.addAttribute( BoardManager.supplyAttr, 1 ),
	
	researchFacility = new BuildingCard( "Research Lab", "rl", 4, 4 )
			.addAttribute( BoardManager.energyMultiplierAttr, 2 ),
			
	skyscraper = new BuildingCard( "Skyscraper", "sk", 7, 12 ),
	
	factory = new BuildingCard( "Factory", "f", 5, 4,
		delegate(IGameplay g, int slotIndex ) {
			g.createHandCard( warehouse, slotIndex );	
		}),
			
	coalPlant = new BuildingCard( "Coal Plant", "cp", 5, 5 )
		.addTrigger( new ModifyEnergyTrigger( 
			delegate( IGameplay g, int attachedSlot, int slot, int oldAmount ) {
				g.destroyUnit( attachedSlot );
				return Trigger.FLASH;
			},
			delegate( IGameInfo g, int attachedSlot, int slot, int oldAmount ) {
				return attachedSlot == slot && g.getUnit(slot).getEnergy() == 0;
			}
 		))
		.addTrigger( new CreateUnitTrigger( delegate( IGameplay g, int attachedSlot, int slot ) {
			g.modifyEnergy( attachedSlot, -1 );
			return 0;
		}))
		.addAttribute( BoardManager.supplyAttr, 2 )
		.addAttribute( "Countdown", 1 ),
			
	shippingCenter = new BuildingCard( "Shipping Center", "sh", 6, 3,
		delegate( IGameplay g, int slot ) {
			for( int i =0; i < BoardManager.MAX_UNITS; ++i ) {
				if( i != slot && g.getUnit(i) != null )
					g.modifyEnergy(i,2);
			}
		}),

	techLab = new BuildingCard( "Tech Lab", "tl", 1, 0 )
		.addTrigger( new PlayEventTrigger( 
			delegate( IGameplay g, int attachedSlot, int slot, EventCard card ) {
				g.modifyEnergy( attachedSlot, 2 );
				return Trigger.FLASH;
			}))
		.addAttribute( "Event Buildup 2", 1 ),

	turbochargeStation = new BuildingCard( "Turbocharge Station", "ts", 4, 5,
		delegate(IGameplay g, int slotIndex) {
			g.createHandCard( EventCard.quadBoost, slotIndex );
		}),

	solarPaneling = new BuildingCard( "Solar Paneling", "sp", 4, 4 )
		.addTrigger( new CreateUnitTrigger(
			delegate(IGameplay g, int attachedSlot, int slot) {
				int belowSlot = attachedSlot + 3;
				if( belowSlot < 9 && g.getUnit( belowSlot ) != null ) {
					g.modifyEnergy( belowSlot, 1 );
					return Trigger.FLASH;
				}
				return 0;
		}))
		.addAttribute( "Feed below", 1 ),
		
	hydroDam = new BuildingCard( "Hydro Dam", "hd", 6, 4 )
		.addTrigger( new CreateUnitTrigger(
			delegate(IGameplay g, int attachedSlot, int slot) {
				int rowStart = 3 * (attachedSlot / 3 );
				for( int i = 0; i < 3; ++i )
					if( rowStart + i != attachedSlot && g.getUnit( rowStart + i ) != null ) {
						g.modifyEnergy( rowStart + i, 1 );
					}
				return Trigger.FLASH;
			}))
		.addAttribute( "Row Buildup", 1 ),

	thinkTank = new BuildingCard( "Think Tank", "tt", 5, 4 )
		.addTrigger( new ModifyEnergyTrigger(
			delegate( IGameplay g, int attachedSlot, int slot, int oldAmount )  {
				g.enqueueAction( delegate(IGameplay g2) {
					g2.createHandCard( EventCard.buildup, slot );
					g2.buildUnit( slot, BuildingCard.skyscraper );
				});
				return Trigger.FLASH | Trigger.REMOVE;
			},
			delegate( IGameInfo g, int attachedSlot, int slot, int oldAmount ) {
				return( attachedSlot == slot && g.getUnit(slot).getEnergy() >= 10 );
			}))
		.addAttribute( BoardManager.buildupAttr, 1 )
		.addAttribute( "Initiative 10", 1 ),
	
	roboticsFacility = new BuildingCard( "Robotics Facility", "rb", 6, 5 )
		.addAttribute( BoardManager.constructionAttr, 2 );
		
}

public class EventCard : GameCard {
	internal static void init() {}

	protected EventCard( string n, string c, TargetedAction a=null, PlayableTester t=null ) : base(n,c,a,t) {}

	public override bool isPlayable( IGameInfo g, int slotIndex ) {
		//by default, event cards can be played on any building
		return (tester == null) ? g.getUnit(slotIndex) != null : tester(g, slotIndex);
	}
	
	//Event cards
	public static readonly EventCard 
	energyBoost = new EventCard( "Energy Boost", "eb", 
		delegate( IGameplay g, int slotIndex ) {
			g.modifyEnergy(slotIndex, 1);
		}),
	
	powerSurge = new EventCard( "Power Surge", "ps",
		delegate( IGameplay g, int slotIndex ) {
			g.modifyEnergy(slotIndex, 2);
		}),
    
    quadBoost = new EventCard( "Quad Boost", "qb",
        delegate( IGameplay g, int slotIndex ) {
            if (slotIndex % 3 == 2) slotIndex--;
            if (slotIndex / 3 == 2) slotIndex -= 3;
            for( int i = 0; i < 2; ++i )
                for( int j = 0; j < 2; ++j )
                {
                    int slot = slotIndex + i + j * 3;
                    if (g.getUnit(slot) != null) g.modifyEnergy(slot, 1);
                }
        }),

	buildup = new EventCard( "Buildup", "b", 
		delegate( IGameplay g, int slotIndex ) {
			g.addAttribute( slotIndex, BoardManager.buildupAttr, 1);
		}),

	recycle = new EventCard( "Recycle", "r", 
		delegate( IGameplay g, int slotIndex ) {
			g.registerTrigger( recycleTrigger, slotIndex );
			g.addAttribute( slotIndex, BoardManager.recycleAttr, 1);
		});

	public static Trigger recycleTrigger = new DestroyUnitTrigger(
		delegate(IGameplay g, int attachedSlot, int slot, IUnitInfo unit ) {
			g.createHandCard( unit.getOriginCard(), attachedSlot );
			return Trigger.FLASH;
		},
		delegate(IGameInfo g, int attachedSlot, int slot, IUnitInfo unit ) {
			return( attachedSlot == slot );
		});

}