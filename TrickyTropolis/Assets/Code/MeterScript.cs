using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MeterScript : MonoBehaviour {
	public RectTransform meterTicks;
	public Image meterFill;
	public Image meterFillGhost;
	public Text energyText, maxEnergyText;
	public float fillTime = 2f;
	public Color normalFillColor, completeFillColor, bonusFillColor;
	public GameObject flagImage, trophyImage;

	int max = 20;
	float targetFill = 0;

	public int getMax() {
		return max;
	}

	public void setMax( int max ) {
		meterFill.fillAmount *= (float)this.max / max;
		targetFill *= (float)this.max / max;
		this.max = max;
		meterTicks.localScale = new Vector3 (1f, 20f / max, 1f);
		meterTicks.sizeDelta = new Vector2 (meterTicks.sizeDelta.x, 1000f * max / 20);
	}

	public void setGhost( int amt ) {
		meterFillGhost.fillAmount = (float)amt / max;
	}

	public void setCurrent( int amt ) {
		StopAllCoroutines ();
		energyText.text = "" + amt;
		targetFill = (float)amt / max;
		StartCoroutine (adjustFill ());
	}

	public void setFlagHeight( int amt ) {
		Vector3 p = flagImage.transform.localPosition;
		flagImage.transform.localPosition = new Vector3( p.x, ((float)amt / max - .5f)*1000, p.z );
	}

	// Use this for initialization
	void Start () {
	}

	IEnumerator adjustFill() {
		for (;;) {
			float delta = Time.deltaTime / fillTime;
			float currentFill = meterFill.fillAmount;
			float sign = Mathf.Sign( targetFill - currentFill);
			if( sign == 0f ) break; //This shouldn't ever happen
			currentFill += delta * sign;
			meterFill.fillAmount = currentFill;
			energyText.rectTransform.localPosition = new Vector3(0, (currentFill - .5f)*1000,1);
			if( Mathf.Sign ( targetFill - currentFill) != sign ) 
			{
				meterFill.fillAmount = targetFill;
				break;
			}
			yield return null;
		}
	}
			
	// Update is called once per frame
	void Update () {
	
	}
}
