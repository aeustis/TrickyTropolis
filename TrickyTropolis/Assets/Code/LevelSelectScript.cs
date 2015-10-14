using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelSelectScript : MonoBehaviour {
	public DifficultyButtonScript[] buttons;
	public UIManager manager;
	LevelButtonScript[] levelButtons;

	public int[] groupFlagsEarned;

	// Use this for initialization
	void Start() {
		groupFlagsEarned = new int[ buttons.Length ];
		init ();
	}

	internal int flagsMissed, trophiesMissed, nextLockedGroup;
	public void init() {
		LevelGroup[] groups = LevelGroup.allGroups;
		if (levelButtons != null) {
			foreach (LevelButtonScript b in levelButtons) 
				Destroy (b.gameObject);
			levelButtons = null;
		}

		flagsMissed = trophiesMissed = 0;
		nextLockedGroup = -1;
		for (int i=0; i < buttons.Length; ++i) {
			DifficultyButtonScript b = buttons[i];
			b.buttonText.text = groups[i].name;

			//Determine if this group is unlocked
			if( isGroupUnlocked(i) ) {
				//Unlock the group
				b.gameObject.SetActive(true);
				b.padlock.SetActive(false);
				b.GetComponent<Button>().interactable = true;

				//Determine the maximum number of flags and trophies for this group,
				//and also the number of flags and trophies earned
				int flags = groups[i].levels.Length;
				int trophies = 0, trophiesEarned = 0;
				groupFlagsEarned[i] = 0;
				foreach( Level lvl in groups[i].levels ) {
					int bestScore = PlayerPrefs.GetInt( lvl.hand );
					if( lvl.bonusGoal != -1 ) {
						++trophies;
						if( bestScore >= lvl.bonusGoal ) ++trophiesEarned;
						else ++trophiesMissed;
					}
					if( bestScore >= lvl.energyGoal ) ++groupFlagsEarned[i];
					else ++flagsMissed;
				}
				b.flagText.text = groupFlagsEarned[i] + "/" + flags;
				b.trophyText.text = trophiesEarned + "/" + trophies;
			} else {
				nextLockedGroup = i;
				//Lock the group
				b.gameObject.SetActive(true);
				b.GetComponent<Button>().interactable = false;
				b.flagText.text = flagsMissed + " left";
				b.trophyText.text = (trophiesMissed - groups[i].trophiesSkippable) + " left";

				//Make any remaining buttons invisible, then be done
				while( ++i < buttons.Length ) {
					buttons[i].gameObject.SetActive(false);
				}
				break;
			}
		}
	}

	internal bool isGroupUnlocked( int groupNum ) {
		return flagsMissed == 0 && trophiesMissed <= LevelGroup.allGroups[groupNum].trophiesSkippable;
	}

	public GameObject firstLevelButton;
	public RectTransform nextColPos, nextRowPos;
	int ROW_LENGTH = 5;
	int groupIndex;
	
	public void difficultyButtonPressed( DifficultyButtonScript button ) {
		//Figure out which button was pressed
		groupIndex = 0;
		while (button != buttons[groupIndex]) 
			if (++groupIndex == buttons.Length) return;

		//If the user hasn't beaten any levels in this group, go directly
		//to Level 1
		if (groupFlagsEarned [groupIndex] == 0) {
			manager.selectLevel( groupIndex, 0 );
			return;
		}

		//Make the difficulty buttons invisible
		foreach (DifficultyButtonScript b in buttons)
			b.gameObject.SetActive (false);

		//Populate the window with level buttons
		levelButtons = new LevelButtonScript[LevelGroup.allGroups[groupIndex].levels.Length];
		Vector3 colDelta = nextColPos.localPosition - firstLevelButton.transform.localPosition;
		Vector3 rowDelta = nextRowPos.localPosition - firstLevelButton.transform.localPosition;
		Color groupColor = buttons [groupIndex].GetComponent<Image> ().color;
		for( int i=0; i < levelButtons.Length; ++i ) {
			LevelButtonScript b = levelButtons[i] = 
				Instantiate( firstLevelButton ).GetComponent<LevelButtonScript>();
			b.GetComponent<RectTransform>().SetParent( this.transform, false );
			b.transform.localPosition = 
				firstLevelButton.transform.localPosition + (i%ROW_LENGTH)*colDelta + (i/ROW_LENGTH)*rowDelta;
			b.gameObject.GetComponent<Image>().color = groupColor;
			b.buttonText.text = "" + (i+1);

			//Figure out whether the trophy/flag icons should be visible
			Level lvl = LevelGroup.allGroups[groupIndex].levels[i];
			int bestScore = PlayerPrefs.GetInt( lvl.hand );
			b.flag.SetActive( bestScore >= lvl.energyGoal );
			b.trophy.SetActive( lvl.bonusGoal != -1 && bestScore >= lvl.bonusGoal );
			b.gameObject.SetActive(true);
		}
	}

	public void levelButtonPressed( LevelButtonScript button ) {
		//Figure out which button was pressed
		int i = 0;
		while (button != levelButtons[i]) 
			if (++i == levelButtons.Length) return;

		manager.selectLevel (groupIndex, i);
	}
}
