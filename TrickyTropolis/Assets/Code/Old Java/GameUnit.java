package sequence;

import java.util.HashMap;
import java.util.Map;
import java.util.Set;

interface UnitInfo {
	int getEnergy();
	String getName();
	BuildingCard originCard();
	Integer getAttrValue( String attr );
	Set<String> attrSet();
}

public class GameUnit implements UnitInfo {
	String name;
	int energy;
	BuildingCard originCard;
	private Map<String, Integer> attributes = new HashMap<String, Integer>();
	
	public int getEnergy() { return energy; }
	public String getName() { return name; }
	public BuildingCard originCard() { return originCard; }
	public Integer getAttrValue( String attr ) {
		return attributes.get(attr);
	}
	public Set<String> attrSet() { 
		return attributes.keySet();
	}
	public void addAttribute( String attr, int amount ) {
		if( !attributes.containsKey(attr) ) 
			attributes.put(attr, amount);
		else attributes.put( attr, attributes.get(attr) + amount );
	}
}
