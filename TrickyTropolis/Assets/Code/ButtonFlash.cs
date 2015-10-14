using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonFlash : MonoBehaviour {
	public Color flashColor;
	public float flashTime;
	public float maxFlashTime;

	Image img;
	Color normalColor;

	public void beginFlash() {
		StartCoroutine (flash ());
	}
	                   
	IEnumerator flash() {
		float startTime = Time.time;
		img = GetComponent<Image> ();
		normalColor = img.color;
		bool highlighted = false;

		while( Time.time - startTime < maxFlashTime ) {
			highlighted = !highlighted;
			img.color = highlighted ? flashColor : normalColor;
			yield return new WaitForSeconds(flashTime);
		}
		endFlash ();
	}

	public void endFlash() {
		if (img != null) {
			img.color = normalColor;
			StopAllCoroutines ();
		}
	}
}
