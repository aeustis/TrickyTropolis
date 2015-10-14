using System.Collections.Generic;
using UnityEngine;
using System;

public class CardComparer : IComparable<CardComparer> {
	public GameCard card;
	public CardComparer( GameCard c ) { card = c; }
	public int CompareTo( CardComparer other ) {
		int cost, otherCost;
		if (card is BuildingCard)
			cost = (card as BuildingCard).energyCost;
		else
			cost = 999;
		if (other.card is BuildingCard)
			otherCost = (other.card as BuildingCard).energyCost;
		else
			otherCost = 999;
		if (cost != otherCost)
			return cost < otherCost ? -1 : 1;

		return ( card.name.CompareTo (other.card.name) < 0 ) ? -1 : 1;
	}
}

public class HandGenerator {
	GameCard[] cards;
	double[] freqs;
	double freqTotal = 0;

	public HandGenerator() {
		cards = GameCard.getAllCards ();
		freqs = new double[ cards.Length ];

		for (int i = 0; i < cards.Length; ++i) {
			if( cards[i] is BuildingCard ) {
				freqs[i] = 30f / ( (cards[i] as BuildingCard).energyCost + 3);
			}
			else if( cards[i] is EventCard ) {
				freqs[i] = 10;
			}

			//Some special exceptions
			if( cards[i] == BuildingCard.warehouse )
				freqs[i] *= 2;
			if( cards[i] == EventCard.energyBoost || cards[i] == BuildingCard.solarPaneling )
				freqs[i] = 0;
			/*if( cards[i] == BuildingCard.storageCell || cards[i] == BuildingCard.industrialZone )
				freqs[i] = 18;*/
			if( cards[i] == BuildingCard.databank )
				freqs[i] *= 1.5;
			if( cards[i] == BuildingCard.chargingStation )
				freqs[i] *= 2;
			if(  cards[i] == BuildingCard.windTurbine )
				freqs[i] *= 1.5;
			if( cards[i] == BuildingCard.constructionYard )
				freqs[i] *= 1.5;
	
			freqTotal += freqs[i];
		}
	}

	public string generate( int n ) {
		SortedList<CardComparer, int> hand = new SortedList<CardComparer, int>();

		for( int i = 0; i < n; ++i ) {
			double val = UnityEngine.Random.value*freqTotal;
			int j = 0;
			while( val > freqs[j] ) {
				val -= freqs[j];
				j++;
			}
			hand.Add( new CardComparer( cards[j] ), 0 );
		}
		string handCodes = "";
		foreach( CardComparer k in hand.Keys ) {
			handCodes += k.card.code + " ";
		}
		return handCodes.Trim(' ');
	}
}
