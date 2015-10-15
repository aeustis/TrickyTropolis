using UnityEngine;
using System.Collections;

public class Level
{
    public readonly string hand;
    public readonly int energyGoal;
	public readonly int bonusGoal;
	public string messageTitle;
	public string[] messages;
    public Level( string h, int e, int b = -1 )
    {
        hand = h;
        energyGoal = e;
		bonusGoal = b;
    }
	public Level addMessage( string title, params string[] m ) {
		messageTitle = title;
		messages = m;
		return this;
	}
}

public class LevelGroup {
    public readonly Level[] levels;
    public readonly string name;
	public readonly int trophiesSkippable;

    public LevelGroup(string n, Level[] lvls, int skippable=0 ) { 
		name = n; 
		levels = lvls;
		trophiesSkippable = skippable;
	}

    public static LevelGroup[] allGroups = {
	/*
	new LevelGroup ( "Test", new Level[] {
			new Level( "w w w", 2, 6 )
				.addMessage( "Title Bar", "First message", "second message", "third message" ),
			new Level("w iz iz cs cy s s s sh sh", 2, 46 ),
			new Level("w pg ps wt iz iz iz iz cs sh", 2, 50 )
	}),*/

	new LevelGroup ("Beginner", new Level[] {
        new Level ("w w w w", 8)
.addMessage("Welcome to TrickyTropolis!", 
"The object of this game is to reach the Energy goal for each level.  " +
"You gain Energy by creating buildings, each of which holds a certain amount of Energy.",
"To complete this level, simply play all four Warehouses by dragging them onto " + 
"empty slots.",
"Whenever you need to try a level again, press and hold the red button for 1 second to restart from the " +
"beginning.  There is no limit to the number of attempts you can make!"),
        new Level ("w w cs cs", 8)
.addMessage( "Energy Cost",
"Pay attention to the numbers in the upper right corner of your cards!  The first " +
"number is Energy Cost, and the second is Starting Energy.",
"You can take a closer look at your cards by tapping on them.  Tap the Charging Station now.",
"Because the Charging Station has a cost of 2, it cannot be played onto an empty slot.  " +
"Instead, it must be played over an existing building having 2 or more Energy.  That building " +
"and its energy will be lost, so choose wisely!"),				          
        new Level ("w w w w s", 9)
.addMessage( "Buildup",
"Some buildings have Buildup.  They will gain +1 Energy every time another new building is created.  " +
"These buildings can gain a lot of Energy if you play them early!"),
        new Level ("w w cs wt", 7)
.addMessage( "Supply",
"Some buildings have Supply.  They will provide +1 extra Energy to every new building you create.  " +
"Supply is another ability that you want to get as early as possible." ),			           
        new Level ("d w w wt", 6)
.addMessage( "Upgrade",
"A building with the Upgrade attribute will give a bonus to whichever building is constructed " +
"<i>over</i> it.  The Databank gives +1 extra energy.",
"You will need this extra Energy to construct a Wind Turbine."),
		new Level ("d w cs cs", 8)
.addMessage( "Energy Precision",
"In general, you can get the most Energy from your buildings when you meet their Energy Cost exactly.  " +
"For this level, make sure to waste nothing!"),					           
        new Level ("w iz iz s", 4)
.addMessage( "Industrial Zone", 
"The Industrial Zone creates two buildings with no starting Energy.  Although it does little by itself, " +
"it is very useful for triggering other abilities such as Buildup and Supply.",
"TrickyTropolis is all about making your cards work together!"),
        new Level ("d w wt ps", 7)
.addMessage( "Event Cards",
"Event cards are distinguished by their purple color.  Instead of creating a new building, they provide " +
"benefits to your existing buildings."),
		new Level ("d d cs rl", 6)
.addMessage( "Research Lab",
"The Research Lab is a special building with an Energy multiplier.  Whenever a building would " +
"normally gain energy, the Research Lab gains twice as much!",
"This makes it a perfect target for cards such as Energy Boost.")
    }, 0 ),

		new LevelGroup ("Student", new Level[] {
		new Level ("iz iz qb", 4)
			.addMessage( "Student", 
		"Welcome to the Student levels!  Now that you know the basics, " +
		"there will be fewer hints.  If you're not sure what a card does, experiment!  You can always " +
		"restart the level." ),
		new Level ("d d iz s s qb", 12),	
		new Level ("d w s s cy cy", 5)
			.addMessage( "Construction Yard", 
				"The Construction Yard lowers the cost of buildings in your hand by 1.  Not only does " +
				"it make your buildings cheaper to play, but also any building that normally costs 1 can be played " +
				"onto an empty slot!"),			
		new Level ("w s s iz cy qb", 7),
		new Level ("d w s cs rl ps", 12),
		new Level ("w w w dn dn", 10)
			.addMessage( "Distribution Netork",
		"The Distribution Network boosts the buildings adjacent to it when it is played.  Try to boost " +
		"as many buildings as you can!",
		"Buildings across the diagonal don't count as adjacent." ),

        new Level ("d iz dn", 4, 5)
			.addMessage( "Bonus Trophy", 
		"There is a bonus trophy in this level!  You can earn it if you fill your energy meter " +
		"<i>past</i> the top.  Trophies will help you unlock additional stuff in the game.",
		"We've told you about this one, but normally these bonus trophies will be hidden!  You " +
		"won't know which levels they're in until you find them!",
		"The Level Select screen will tell you how many trophies are still left to find within each " +
		"group." ),
		new Level ("w w cs sf rl ps", 13 )
			.addMessage( "Scaffolding",
			"The Scaffolding grants Buildup to the building played over it.  It works the " +
			"same way as the Databank.  You can give Buildup to potentially any building, if you have the " +
			"right cards!"),
		new Level("d w cs cs f", 9),
        new Level ("w w w sc cs rl", 10)
			.addMessage( "Storage Cell",
		"The Storage Cell starts with 1 energy, and also steals 1 more from each other building you have.",
		"This can give you a free building with a lot of starting energy, but be very careful about " +
		"which buildings to steal from!" )
	//new Level(
	//new Level(
    }, 0 ),

    new LevelGroup ("Challenger", new Level[] {
		new Level ("d w w s s s s s", 8)
			.addMessage( "Challenger", 
		"You made it!  Time to ramp up the difficulty even more!  You'll have more cards, and " +
		"there are two hidden trophies somewhere in these levels!" ),
		new Level ("w w w s cy wt ps", 11, 12), //Good candidate for trophy
		new Level ("iz w s cs qb qb", 11),
		new Level ("d d iz w sc wt", 10),
		new Level ("iz iz w s qb b", 11, 12 ), //Trophy?
		//new Level ( "d w s iz cs hd ps", 14 ),
		new Level ("d sc sc w dn rl", 10),	
		new Level("d d w s s sf cy f", 15),
		new Level ("d w w s wt wt wt wt", 9),
		new Level("w w d cs wt sf r", 15)
    }, 1 ),

	new LevelGroup ("Expert", new Level[] {
		new Level( "iz sc w w cs wt sk sk b", 28 ),
		new Level ("w w w dn dn cs cy f", 18, 19), //Trophy?
		new Level ("d d sc s cs cy rl", 9, 10), //Red Herring
		new Level ("d d w sf dn cs wt", 14, 15), //Hard!
		new Level( "d iz iz iz dn dn dn", 14, 15 ),
		new Level( "d d w sf cs wt rl ps", 20 ),  //Trophy?
		new Level( "w w w cs cs cs rl tt b ps", 27 ),
		new Level( "d w w iz cs wt wt wt cp ps", 25 ),
		new Level( "d d iz iz iz w cs cp ps", 15, 16 ),
		new Level( "d w s s sc cs cy f f", 23 ),
		new Level( "d d w w pg wt wt f f", 26, 27 )
	}, 1 )
	};
	/* some Chaos Mode levels
	d d sc dn dn cs dy wt cp b
	sc sc sc w dn dn cs sf sh b
	d d w dn cs cy sh b b ps
	*/
	// tutorial, basic, intermediate, tricky, complex, advanced, expert, stupefying, mind-bending, genius
}
