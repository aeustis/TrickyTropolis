using UnityEngine;
using System.Collections.Generic;

public interface IUnitInfo {
	int getEnergy();
	string getName();
	BuildingCard getOriginCard();
	int? getAttrValue( string attr );
	IEnumerable<string> getAttributes();
}

public class GameUnit : IUnitInfo {
	public string name;
	public int energy;
	public BuildingCard originCard;
	Dictionary<string,int> attributes = new Dictionary<string,int>();
	
	public int getEnergy() { return energy; }
	public string getName() { return name; }
	public BuildingCard getOriginCard() { return originCard; }
	public int? getAttrValue( string attr ) {
		int val;
		if (attributes.TryGetValue (attr, out val))
			return val;
		return null;
	}
	public IEnumerable<string> getAttributes() { 
		return attributes.Keys;
	}

	public void addAttribute( string attr, int val ) {
		if (attributes.ContainsKey (attr)) {
			int currentVal;
			attributes.TryGetValue (attr, out currentVal);
			attributes.Remove( attr );
			attributes.Add (attr, currentVal + val);
		} else {
			attributes.Add (attr, val);
		}
	}
}
