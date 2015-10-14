package sequence;

import java.util.Collections;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.Random;

class CardComparer implements Comparable<CardComparer> {
	public GameCard card;
	
	public CardComparer( GameCard c ) { card = c; }
	
	public int compareTo(CardComparer c) {
		int thisCost, otherCost;
		thisCost = ( card instanceof BuildingCard ? ((BuildingCard)card).energyCost : 9999 );
		otherCost = ( c.card instanceof BuildingCard ? ((BuildingCard)c.card).energyCost : 9999 );
		if( thisCost < otherCost ) return -1;
		if( thisCost > otherCost ) return 1;
		return card.name.compareTo(c.card.name);
	}
}

public class HandGenerator {
	
	private HashMap< GameCard, Float > cardFreqs = new HashMap<GameCard, Float>();
	private Random rng = new Random();
	private float freqTotal;
	
	HandGenerator() {
		//Add all buildings and cards with frequency 1.0
		for( GameCard b : BuildingCard.list() ) cardFreqs.put(b, 1.0f);
		for( GameCard e : EventCard.list() ) cardFreqs.put(e, 1.0f);
		
		//Exceptions: 0-cost buildings are more common
		for( GameCard c : new GameCard[] {BuildingCard.databank, BuildingCard.storageCell, BuildingCard.industrialZone} )
			cardFreqs.put(c, 2.0f);
		
		//Certain low-cost cards are also common
		for( GameCard c : new GameCard[] {BuildingCard.chargingStation, BuildingCard.substation} )
			cardFreqs.put(c, 2.0f); 
		
		//Warehouse is more common still
		cardFreqs.put(BuildingCard.warehouse, 3f);
		
		//Energy Boost and Overload can't be drawn 
		cardFreqs.put( EventCard.energyBoost, 0.0f );
		cardFreqs.put( EventCard.overload, 0.0f );
		
		freqTotal = 0;
		for( Float f : cardFreqs.values() ) freqTotal += f;
	}
	
	GameCard generateCard() { 
		float rand = rng.nextFloat()*freqTotal;
		for( GameCard c : cardFreqs.keySet() ) {
			float freq = cardFreqs.get(c);
			if( rand <= freq ) return c;
			rand -= freq;
		}
		return null;
	}
	
	GameCard[] generateHand( int size ) {
		LinkedList<CardComparer> cardList = new LinkedList<CardComparer>();
		
		for( int i = 0; i < size; ++i ) cardList.add( new CardComparer( generateCard() ) );
		Collections.sort(cardList);

		GameCard[] hand = new GameCard[size];
		for( int i = 0; i < size; ++i ) hand[i] = cardList.get(i).card;
		return hand;
	}
}

