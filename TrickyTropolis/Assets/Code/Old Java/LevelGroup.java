package sequence;

import java.util.HashMap;
import java.util.LinkedList;
import java.util.Map;
import java.util.Scanner;

class Level {
	public GameCard[] startHand;
	public int energyGoal;
	public String title;
	public String description;
	Level( GameCard[] h, int e, String t, String d ) {
		startHand = h; energyGoal = e; title = t; description = d;
	}
}

public class LevelGroup {
	private LinkedList<Level> list = new LinkedList<Level>();
	public Level[] list() { return list.toArray(new Level[list.size()]); }
	public LevelGroup add( String handCode, int e, String t, String d ) {
		LinkedList<GameCard> cards = new LinkedList<GameCard>();
		Scanner s = new Scanner(handCode);
		while( s.hasNext() ) cards.add( code.get(s.next()) );
		s.close();
		list.add( new Level (cards.toArray(new GameCard[0]),e,t,d) );
		return this;
	}
	
	private static final Map<String, GameCard> code = new HashMap<String, GameCard>();
	public static int NUM_GROUPS = 3;
	public static LevelGroup[] levelGroups = new LevelGroup[NUM_GROUPS];
	public static void loadLevels() {
		for( GameCard c : BuildingCard.list() ) code.put( c.code, c );
		for( GameCard c : EventCard.list() ) code.put( c.code, c );
		
		/////////////////////////////////////////
		//Tutorial levels
		/////////////////////////////////////////
		
		LevelGroup tutorialLevels = new LevelGroup()
		.add( "w w w w", 8,
			"Welcome to Sequence!", 
			"The object of this game is to reach the total Energy goal for each level.\n"
			+ "Do this by playing buildings from your hand (left), each of which holds a \n"
			+ "certain amount of Energy. The counter above the playing field shows how \n"
			+ "close you are to your goal.\n\n"
			+ "For this level, simply play all four Warehouses to win.  You must place \n"
			+ "them in different slots or you will have to restart (spacebar)." )
			
		.add( "w w cs cs", 8, 
			"Energy Requirement",
			"Pay attention to the numbers in the upper right corner of your cards!\n"
			+ "The first number is Energy Cost; the second number is Starting Energy.\n\n"
			+ "Because Charging Station costs 2, it cannot be played on an empty square.\n"  
			+ "Instead, it must replace an existing building having at least 2\n"
			+ "Energy. That building and its Energy will be lost, so choose carefully!" ) 
			
		.add( "w w w w s", 9, 
			"Conduit",
			"Buildings with Conduit gain +1 Energy whenever another building\n"
			+ "is played.  The sooner you get Conduits out, the more Energy\n"
			+ "they will gain.")
			
		.add( "d d cs", 5, 
			"UPGRADE!",
			"The UPGRADE attribute gives a bonus to whichever building\n"
			+ "is constructed over it.  When you play a building over a \n"
			+ "Databank, the new building gets 1 more Energy than normal." )
			
		.add( "d w cs cs", 8, 
			"Energy Precision",
			"In genereral, you get the most value out of your buildings when you meet \n"
			 + "their Energy requirements exactly. Avoid waste!")
			 
		.add( "w w d wt", 6, 
			"Supply",
			"When you have a building with Supply in play, it will provide \n"
			+ "+1 Energy to every new building you make. Like Conduit, this \n"
			+ "ability gives more Energy the earlier you can get it.")
			
		.add("w d d cs pu", 10, 
			"Event Cards",
			"Event cards, such as Power-Up, give Energy or other bonuses to your \n"
			+ "existing buildings. Figuring out the best time to play them can be tricky!" )
			
		.add("d i i s", 5, 
			"Industrial Zone",
			"The Industrial Zone creates two buildings with no starting Energy.\n"
			+ "This is useful for triggering effects like Conduit and Supply." )
			
		.add( "w s cs cs cy", 6,
			"Construction",
			"The Construction ability reduces the Energy Cost of the buildings\n"
			+ "in your hand by 1. If a building's cost is reduced to zero,\n"
			+ "you can play in on an empty slot!\n\n"
			+ "Hint: You can replace the Construction Yard itself and still\n"
			+ "get the cost reduction!" )

		.add("d d cs rf ps", 8,
			"Energy Multiplier",
			"The Research Facility gains double Energy from all sources!\n"
			+ "This makes it a great target for Energy Boost, Supply, and so on.");
		
		/////////////////////////////////////////
		//Intermediate levels
		/////////////////////////////////////////
		
		LevelGroup intermediateLevels = new LevelGroup()
		.add( "d d i s s pu", 12,
			"Intermediate Levels", 
			"Now that you know the basics, let's start to make things a \n"
			+"little harder. You'll start with more cards and fewer hints." )
			
		.add( "i i i s s pu pu", 15,
			"Zoning Code", 
			"Get ready for some serious Industrial Zoning!" )
			
		.add( "w w w s s s s s", 7,
				"Too many Substations",
				"No hints this time..." )
			
		.add( "i w s cs pu pu",  11,
			"Precision vs Sequencing",
			"Sometimes you will have to choose between meeting an Energy \n"
			+"requirement exactly, versus playing cards in the more logical order.\n"
			+"Always be aware of your choices!" )
				
		.add( "d w s cy wt ps", 8,
			"Ability Choices", 
			"Conduit, Supply, and Construction all want to be played \n"
			+"early. So which comes first?  That depends, of course!" )
		
		.add( "d i w sc sc wt rf", 11,
			"Storage Cells",
			"Storage cells can be used to concentrate Energy in one place."
			+"They have other uses too!")
			
		.add( "d d i w sc wt", 10,
			"Storage Cells 2",
			"Can you get that last point of energy?")
			
		.add( "i d w s wt cy pu", 11,
				"Red Herring",
				"Occasionally you'll be given cards which don't actually need.\n"
				+"Don't be fooled!  Playing all your cards is not a requirement\n"
				+"for victory!")
		
		.add( "d w s cs rf ps c", 13, 
				"Science!",
				"Time to fund some serious research.")
				
		.add( "d w w s wt wt wt wt", 9,
				"A Quixotic Level",
				"Try to make the most of a weird hand." );
		
		/////////////////////////////////////////
		//Advanced levels
		/////////////////////////////////////////
		LevelGroup advancedLevels = new LevelGroup()
		.add( "i sc w w cs wt c sk sk", 26,
			"Advanced Levels", 
			"Welcome to the Advanced levels!  There will be some bigger cards and \n"+ 
			"even bigger hands!")
							
		.add( "d d i i i w cs cp ps", 16,
			"Nonrenewable Resource",
			"The Coal plant gives 2 Supply, but loses 1 Energy with each play. If\n" +
			"it runs out of Energy, it self-destructs!")
			
		.add( "d w w i cs wt wt wt ps cp", 25, 
			"Supply Center",
			"Can you manage your resources effectively?" )
		
		.add( "d w s s sc cs cy f f", 23, 
			"Assembly Line",
			"Don't be afraid to use trial and error, but it has to be guided by a sense\n"+
			"of what the possible good plays are." )
			
		.add( "d d w w rc wt wt f f", 26,
			"Reactor Core",
			"The Reactor Core gives an extra Supply to any building constructed over it.\n"+
			"Bonus: 27 Energy is possible!" )

		.add( "d d i w rc cs cs wt rf", 24,
			"Reactor Core II",
			"Can you spot the clever opening?" )
		
		.add( "w w cs ms ms ms f sk sk", 24,
			"Machine Shop", 
			"The Machine Shop is a flexible building which can upgrade itself or another\n"+
			"building with Conduit." )
			
		.add( "d d w tl cs cy ms sk ps", 23, 
			"Tech Lab",
			"The Tech Lab rapidly gains Energy when you play Event cards." )
			
		.add( "d w w s rc cs cy cp f rf sk", 26,
			"Heavy-handed",
			"You have many high-cost buildings.  Can you make use of them all?\n"+
			"28 Energy is possible!" )
			
		.add( "d d w i s cs wt ms cp rf", 32,
			"Science II",
			"Last level (for now)!  Don't give up!"  );
				
		levelGroups[0] = tutorialLevels; 
		levelGroups[1] = intermediateLevels;
		levelGroups[2] = advancedLevels;
	}	
}

	
	
	
	
	

