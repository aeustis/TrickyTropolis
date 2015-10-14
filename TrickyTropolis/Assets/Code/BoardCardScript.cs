using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BoardCardScript : MonoBehaviour {
	public Text energyText;
	public Text attrText;
	public IUnitInfo unit;

	public void updateEnergy() {
		energyText.text = "" + unit.getEnergy();
	}

	public void updateAttributes() {
		attrText.text = "";
		foreach (string attr in unit.getAttributes()) {
			attrText.text += attr; 
			if( unit.getAttrValue(attr) != 1 ) attrText.text += " " + unit.getAttrValue(attr);
			attrText.text += "\n";
		}
	}
}
