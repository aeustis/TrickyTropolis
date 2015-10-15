using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour, IGametoUI {
	public GameObject handCardObject, boardCardObject, floatingTextObject;
	public MeterScript meterScript;
	public CameraScript theCamera;

	public Transform zoomedCardPos, handSpawnPos;
	public int MAX_COLUMN_SIZE = 10;

	public Transform handOrigin, handNextColumn, handBottom, columnDefaultOffset;
	public Color slotActiveColor, slotInactiveColor, slotConfirmColor, slotDenyColor, slotFlashColor;

	internal CardUI selectedCard = null;
	internal Vector3 dragOffset;

	internal int numHandColumns;
	List<CardUI> handCards;

	IUItoGame theGame;
	public GameObject[] buildSlots;
	BoardCardScript[] boardUnits = new BoardCardScript[ BoardManager.MAX_UNITS];
	
	int selectedSlot = -1;
	bool[] slotsPlayable;

    // Use this for initialization
    int level = -1;
    int levelGroup = -1;
    public Level currentLevel;
	void Start () {
		slotsPlayable = new bool[buildSlots.Length];
		//PlayerPrefs.DeleteAll (); //Delete me!

        if (chaosMode) {
			startNextLevel();
		} 
	}

	int constructionTotal=0;
	CardUI newHandCard( string code, Vector3 startPos ) {
		GameObject obj = Instantiate(handCardObject, startPos, Quaternion.identity) as GameObject;
		obj.GetComponent<SpriteRenderer>().sprite = 
			GameObject.Find(code).GetComponent<SpriteBox>().handSprite;
		CardUI newCard = obj.GetComponent<CardUI>();
		GameCard cardData = GameCard.findByCode (code);
        newCard.gameCard = cardData;
		if (cardData is BuildingCard) {
			BuildingCard b = cardData as BuildingCard;
			obj.GetComponentInChildren<Text> ().text = setEnergyText(b,constructionTotal);
		} else {
			obj.GetComponentInChildren<Text>().text = "";
		}
		
		newCard.zoomPos = zoomedCardPos.position;
		newCard.manager = this;
		handCards.Add(newCard);
		return newCard;
	}

	public string setEnergyText( BuildingCard card, int construction ) {
		int cost = card.energyCost - construction;
		if (cost < 0) cost = 0;
		string text;
		if (cost < card.energyCost) 
			text = "<color=#00C800FF>" + cost + "</color>";
		else
			text = "" + cost;
		text += "/" + card.startingEnergy;
		return text;
	}

	HandGenerator handGen = new HandGenerator ();
	bool chaosMode = true;
	int levelBest;
	public MessageBoxScript msgManager;
	public ButtonFlash nextButton, resetButton, backButton;

	public void startGame () {
		StopAllCoroutines ();
		nextButton.endFlash ();
		resetButton.endFlash ();
		//Get rid of all currently existing cards
		if( handCards != null )
			foreach (CardUI card in handCards)
				Destroy (card.gameObject);
		selectedCard = null;
		selectedSlot = -1;

		//Get rid of effects, including board cards
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("effect")) {
			Destroy (obj);
		}

		animsRunning = 0;

		//Set up energy meter
		meterScript.setCurrent (0);
		levelBest = PlayerPrefs.GetInt( currentLevel.hand ); //Gets 0 if the key doesn't exist
		int goal = ( levelBest > currentLevel.energyGoal && currentLevel.bonusGoal != -1 ) 
			? currentLevel.bonusGoal : currentLevel.energyGoal ;
		meterScript.setMax( goal );
		meterScript.maxEnergyText.text = "" + goal;
		meterScript.setFlagHeight (currentLevel.energyGoal);

		meterScript.trophyImage.SetActive (currentLevel.bonusGoal != -1 && levelBest >= currentLevel.bonusGoal);
		meterScript.flagImage.SetActive (levelBest >= currentLevel.energyGoal);

		meterScript.setGhost (levelBest);
		Color c = getMeterFillColor (levelBest);
		c = new Color (c.r, c.g, c.b, 100f/255);
		meterScript.meterFillGhost.color = c;
		meterScript.meterFill.color = meterScript.normalFillColor;

		//Set slots to default state
		for (int i = 0; i < BoardManager.MAX_UNITS; ++i) 
			setSlotColor (i, slotActiveColor);

		//Determine the starting hand
		string[] handCodes;
		handCodes = currentLevel.hand.Split ();

		//Create the hand cards
		constructionTotal = 0;
		handCards = new List<CardUI> ();
		foreach (string code in handCodes) {
			newHandCard (code, handSpawnPos.position);
		}
		StartCoroutine (setHandPositions ());

		//Display level messages
		if (currentLevel.messages != null)
			msgManager.messageBox (currentLevel.messageTitle, currentLevel.messages);
		else
			msgManager.close ();

		//Create the Board Manager
		GameCard[] cards = new GameCard[handCodes.Length];
		for (int i=0; i<handCodes.Length; ++i) {
			cards[i] = GameCard.findByCode(handCodes[i]);
		}
		theGame = BoardManager.newGame(cards, this);
	}

	Color getMeterFillColor( int amt ) {
		if (currentLevel.bonusGoal != -1 && amt >= currentLevel.bonusGoal)
			return meterScript.bonusFillColor;
		if (amt >= currentLevel.energyGoal)
			return meterScript.completeFillColor;
		return meterScript.normalFillColor;
	}

	void saveLevelScore() {
		if (chaosMode) return;

		int score = theGame.getEnergyTotal ();
		meterScript.meterFill.color = getMeterFillColor (score);

		if ( score > levelBest) {
			PlayerPrefs.SetInt( currentLevel.hand, score );
			PlayerPrefs.Save();

			//Did we reveal the bonus energy goal?
			if( score > meterScript.getMax() && currentLevel.bonusGoal != -1) {
				meterScript.setMax( currentLevel.bonusGoal );
				meterScript.maxEnergyText.text = "" + currentLevel.bonusGoal;
				meterScript.setFlagHeight(currentLevel.energyGoal);
				//Do some kind of celebration?
			}

			//Have we earned rewards for the first time this level?
			bool trophyEarned = 
				currentLevel.bonusGoal != -1 && score >= currentLevel.bonusGoal && levelBest < currentLevel.bonusGoal;
			bool flagEarned = score >= currentLevel.energyGoal && levelBest < currentLevel.energyGoal;

			if( trophyEarned ) {
				--levelSelect.trophiesMissed;
			}
			if( flagEarned ) {
				--levelSelect.flagsMissed;
			}

			//Have we completed the group?
			if( levelSelect.nextLockedGroup != -1 ) {
				LevelGroup lockedGroup = LevelGroup.allGroups[ levelSelect.nextLockedGroup ];
				if( levelSelect.flagsMissed == 0 ) {
					if( levelSelect.trophiesMissed <= lockedGroup.trophiesSkippable ) {
						msgManager.messageBox( "Rank Up!!", 
						"You did it!  You have unlocked the next group of levels and earned the rank of " +
						lockedGroup.name + "!");
					} 
					else {
						msgManager.messageBox( "Group Complete", 
						"You finished all the levels in this group!  However, you will need to find more " +
						"hidden trophies to proceed to the next rank.  Return to the Level Select screen " +
						"and start hunting!");
					}
					backButton.beginFlash();
					return;
				} 
			}
			if( trophyEarned ) {
				//We earned a trophy 
				msgManager.messageBox( "Trophy Earned!",
					"You found a bonus trophy!  Amazing! Collect these to unlock more puzzles " +
					"and game modes.");
				meterScript.flagImage.SetActive(true);
				meterScript.trophyImage.SetActive(true);
				nextButton.beginFlash();
			} else if( flagEarned ) {
				//We earned a flag
				msgManager.messageBox( "Level Complete!", 
					"Good job!  Press the \"next level\" button to continue.");
				meterScript.flagImage.SetActive(true);
				nextButton.beginFlash();
			}
		}
	}

	public LevelSelectScript levelSelect;
    public void onBackButton()
    {
		backButton.endFlash ();
		levelSelect.init();
		theCamera.panTo ( new Vector3(-25,0,-10), CAMERA_PAN_SPEED, CARD_FLY_DAMP_DIST);
    }

    public void startNextLevel()
    {
		if (chaosMode) {
			currentLevel = new Level( handGen.generate(10), 50 );
			meterScript.setMax(50);
		} else {
			level++;
			LevelGroup currentGroup = LevelGroup.allGroups [levelGroup];
			if (level >= currentGroup.levels.Length)
				level = 0;
			currentLevel = currentGroup.levels [level];
		}
        startGame();
    }

	public void onNextButton() {
		startNextLevel ();
	}

	public float CAMERA_PAN_SPEED;

	public void selectLevel(int difficulty, int lvl) {
		chaosMode = false; //Fix me
		theCamera.panTo (new Vector3 (1, 0, -10), CAMERA_PAN_SPEED, CARD_FLY_DAMP_DIST);
		level = lvl - 1;
		levelGroup = difficulty;
		startNextLevel();
	}

	public float HAND_DEAL_DELAY, CARD_FLY_SPEED, CARD_FLY_DAMP_DIST;
	IEnumerator setHandPositions() {
		numHandColumns = 0;
		int i = 0;
		List<CardUI> tempList = new List<CardUI> (); //handCards may be modified during enumeration
		foreach (CardUI card in handCards) {
			if( ++i % 10  == 1 ) ++numHandColumns;
			card.handColumn = numHandColumns - 1;
			tempList.Add(card);
		}
		for (i = 0; i < numHandColumns; ++i) adjustColumn (i, false);

		//Deal the cards
		foreach (CardUI card in tempList) {
			card.flyTo( card.snapPos, CARD_FLY_SPEED, CARD_FLY_DAMP_DIST );
			yield return new WaitForSeconds(HAND_DEAL_DELAY);
		}
	}
	
	void adjustColumn( int col, bool flyImmediate ) {
		Vector3 columnOrigin = handOrigin.position + (handNextColumn.position - handOrigin.position) * (numHandColumns - col - 1);
		//Create a temporary list containing the cards in the civen column
		List<CardUI> thisColumn = new List<CardUI> ();
		foreach (CardUI card in handCards)
			if (card.handColumn == col)
				thisColumn.Add (card);
		int colSize = thisColumn.Count;
		Vector3 cardOffset = (colSize <= 3) ? columnDefaultOffset.position - handOrigin.position : 
			(handBottom.position - handOrigin.position) / (colSize - 1);
		columnOrigin += new Vector3 (0, 0, colSize);
		cardOffset += new Vector3 (0, 0, -1);
		int i = 0;
		foreach (CardUI card in thisColumn) {
			card.snapPos = columnOrigin + cardOffset * i;
			if( flyImmediate ) 
				 card.flyTo( card.snapPos, CARD_FLY_SPEED, CARD_FLY_DAMP_DIST );
			++i;
		}
	}

	// Update is called once per frame
	public void Update () {
		if (theGame == null) return;

		//Run actions until we have to wait for animations to finish
		while (theGame.actionsInQueue() && animsRunning == 0) {
			theGame.runNextAction ();

			updatePostAction();
		}


		//Drag our selected card around, under the right conditions
		if (selectedCard != null && !selectedCard.zoomed && selectedCard.dragEnabled) {
			Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			selectedCard.transform.position = mousePos + dragOffset;
		
			//If the action queue is empty, check to see if we're dragging over a build slot
			if( theGame.actionsInQueue() ) return;
			if (selectedSlot != -1 && slotTest (selectedSlot))
				return;
			deselectSlot ();

			for (int i = 0; i < buildSlots.Length; ++i) {
				if (slotTest (i)) {
					selectSlot (i);
					return;
				}
			}
		}
	}

	//Helper function
	Color[] slotColors = new Color[BoardManager.MAX_UNITS];
	void setSlotColor( int slot, Color color ) {
		slotColors[slot] = color;
		buildSlots [slot].GetComponent<SpriteRenderer> ().color = color;
	}

	void deselectSlot() {
		if (selectedSlot == -1)	return;
        if (selectedCard.gameCard == EventCard.quadBoost)
        {
            for( int i=0; i < 9; ++i )
                setSlotColor(i, slotActiveColor);
        }
        else
        {
            setSlotColor(selectedSlot, slotActiveColor);
        }
        selectedSlot = -1;
	}

	void selectSlot(int slot) {
		//Get rid of any message box
		msgManager.close ();

        selectedSlot = slot;
        if (selectedCard.gameCard == EventCard.quadBoost)
        {
            int startSlot = selectedSlot;
            if (startSlot % 3 == 2) startSlot--;
            if (startSlot / 3 == 2) startSlot -= 3;
            for (int i = 0; i < 2; ++i)
                for (int j = 0; j < 2; ++j)
                    setSlotColor(startSlot + i + j * 3, slotConfirmColor);
        } else
        {
            setSlotColor(slot, slotsPlayable[slot] ? slotConfirmColor : slotDenyColor);
        }
	}

	//Helper function with precondition: selectedCard != null
	bool slotTest( int slot ) {
		return buildSlots [slot].GetComponent<BoxCollider2D>().OverlapPoint (selectedCard.transform.position);
	}

	public void deselectCard() {
		if (selectedCard == null)
			return;
		selectedCard.setZoom (false);
		selectedCard = null;
		deselectSlot ();
	}

	public void onDragStart() {
		GameCard card = theGame.getHand( handCards.IndexOf (selectedCard) );
		bool ready = !theGame.actionsInQueue ();
		for (int i=0; i < buildSlots.Length; ++i) {
			slotsPlayable[i] = ready && card.isPlayable(theGame, i);
		}
	}

	public void onDragRelease() {
		//Attempt to play the card 
		if ( selectedSlot != -1 && slotsPlayable [selectedSlot]
		    && theGame.playCard( handCards.IndexOf(selectedCard), selectedSlot ) ) {
			//Remove the card from our hand
			handCards.Remove(selectedCard);
			adjustColumn(selectedCard.handColumn, true);

			//Deselect and destroy the card 
			//TODO: Add a card play animation (turns white or something?)
			Destroy( selectedCard.gameObject );
			deselectSlot();
			selectedCard = null;
			
			//Set slots to inactive state (can't play additional cards yet)
			//(unless the action queue is already empty)
			if( theGame.actionsInQueue() )
				for( int i = 0; i < buildSlots.Length; ++i ) 
					setSlotColor( i, slotInactiveColor );

			//Update stuff like meter, construction
			updatePostAction();


		} else {
            selectedCard.flyTo( selectedCard.snapPos, CARD_FLY_SPEED, CARD_FLY_DAMP_DIST );
            deselectSlot();
            selectedCard = null;
		}
	}

	void updatePostAction() {
		//Update meter
		meterScript.setCurrent( theGame.getEnergyTotal() );
		
		int c = theGame.getAttributeTotal (BoardManager.constructionAttr);
		if (c != constructionTotal) {
			constructionTotal = c;
			foreach( CardUI card in handCards ) {
				if( card.gameCard is BuildingCard )
					card.GetComponentInChildren<Text>().text = 
						setEnergyText( card.gameCard as BuildingCard, c );
			}
		}

		if( !theGame.actionsInQueue() ) {
			//We just emptied the action queue, so re-enable drag and drop and save the level score
			saveLevelScore();
			for (int i=0; i < buildSlots.Length; ++i)
				setSlotColor (i, slotActiveColor);
			
			if (selectedCard != null) {
				onDragStart ();
				selectedSlot = -1;
			}
		} 
	}

	void updateUnitEnergy( int slotIndex ) {
		boardUnits [slotIndex].updateEnergy ();
	}

	//Callbacks from BoardManager
	public float WIPE_TIME,
	TEXT_FLOAT_TIME,
	TEXT_FLOAT_DELTA,
	TRIGGER_FLASH_TIME,
	FADEOUT_TIME;

	public int NUM_TRIGGER_FLASHES;

	public void onCreateUnit( int slotIndex, IUnitInfo u ) {
		string unitCode = theGame.getUnit (slotIndex).getOriginCard ().code;
		GameObject newUnit = Instantiate (boardCardObject, buildSlots [slotIndex].transform.position, 
			Quaternion.identity) as GameObject;
		newUnit.GetComponent<SpriteRenderer> ().sprite = 
			GameObject.Find (unitCode).GetComponent<SpriteBox> ().boardSprite;
		
		boardUnits [slotIndex] = newUnit.GetComponent<BoardCardScript>();
		boardUnits [slotIndex].unit = u;
		updateUnitEnergy (slotIndex);
		boardUnits [slotIndex].updateAttributes ();
		StartCoroutine (wipeUpwardsAnim (boardUnits[slotIndex], WIPE_TIME));
	}

	public void onModifyEnergy( int slotIndex, int amount ) {
		updateUnitEnergy (slotIndex);
        string text = (amount >= 0 ? "+" : "") + amount;
        Color color = amount >= 0 ? Color.yellow : Color.red;
		StartCoroutine (floatingTextAnim (boardUnits[slotIndex].transform.position, text, color, 
            TEXT_FLOAT_TIME, TEXT_FLOAT_DELTA ));
	}

	public void onDestroyUnit( int slotIndex ) {
		StartCoroutine( destroyUnitAnim( boardUnits[slotIndex].gameObject, FADEOUT_TIME ));
	}

	public void onAddAttribute( int slotIndex ) {
		boardUnits [slotIndex].updateAttributes ();
	}

	public void triggerFlash( IEnumerable<int> slots ) {
		foreach (int slot in slots) 
			StartCoroutine (triggerFlashAnim (slot, NUM_TRIGGER_FLASHES, TRIGGER_FLASH_TIME));
	}

	public void triggerFlash( int slot ) {
		triggerFlash (new int[] {slot});
	}

	public void onNewHandCard( GameCard c, int slotIndex ) {
		CardUI card = newHandCard ( c.code, buildSlots[slotIndex].transform.position );
		card.handColumn = numHandColumns-1;
		adjustColumn (numHandColumns-1, true);
	}

	//Animations
	int animsRunning = 0;
	IEnumerator wipeUpwardsAnim( BoardCardScript obj, float duration ) {
		++animsRunning;
		//Turn off the energy text for now
		obj.attrText.enabled = obj.energyText.enabled = false;

		float startTime = Time.time;
		SpriteRenderer sr = obj.GetComponent<SpriteRenderer> ();
		Sprite originalSprite = sr.sprite;
		Texture2D tx = sr.sprite.texture;
		Rect r = sr.sprite.textureRect;
		float height = r.height;
		sr.enabled = false;
		
		for( ;; ) {
			int h = (int)(height*(Time.time - startTime)/duration);
			if( h == 0 ) {
				yield return null;
				continue;
			}
			if( h > height ) break;
			sr.enabled = true;
			r.height = h;
			Sprite clippedSprite = Sprite.Create (tx, r, 
				new Vector2(0.5f, 0.5f*height/h), 100f);
			sr.sprite = clippedSprite;
			yield return null;
		}

		sr.sprite = originalSprite;
		obj.attrText.enabled = obj.energyText.enabled = true;
		--animsRunning;
	}

	IEnumerator floatingTextAnim( Vector3 pos, string text, Color color, float duration, float deltaY ) {
		++animsRunning;
		GameObject floatingText = Instantiate (floatingTextObject) as GameObject;
        Text txt = floatingText.GetComponentInChildren<Text> ();
        txt.text = text;
        txt.color = color;
        floatingText.transform.position = pos;
		float startTime = Time.time;
		Vector3 originalPos = pos;

		for (;;) {
			float y = deltaY * (Time.time - startTime);
			if( y > deltaY ) break;

			floatingText.transform.position = originalPos + new Vector3(0,y,0);
			yield return null;
		}

		Destroy (floatingText);
		--animsRunning;
	}

	IEnumerator triggerFlashAnim( int slotNum, int numFlashes, float flashDuration ) {
		++animsRunning;
		for( int i = 0; i < numFlashes; ++i ) {
			buildSlots[slotNum].GetComponent<SpriteRenderer>().color = slotFlashColor;
			yield return new WaitForSeconds( flashDuration );
			buildSlots[slotNum].GetComponent<SpriteRenderer>().color = slotColors[slotNum];
			yield return new WaitForSeconds( flashDuration );
		}
		--animsRunning;
	}

	IEnumerator destroyUnitAnim( GameObject obj, float duration ) {
		++animsRunning;
		SpriteRenderer sr = obj.GetComponent<SpriteRenderer> ();
		float startTime = Time.time;
		for (;;) {
			float t = (Time.time - startTime)/duration;
			if( t > 1 ) break;
			sr.color = new Color( 1f, 1f, 1f, 1-t );
			yield return null;
		}
		Destroy (obj);
		--animsRunning;
	}
}
