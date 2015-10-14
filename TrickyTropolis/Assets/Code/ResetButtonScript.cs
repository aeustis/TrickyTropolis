using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResetButtonScript : MonoBehaviour {
	public Image icon, panel, border;
	public float resetDelayTime;
	public UIManager manager;

	public void OnMouseDown() {
		StartCoroutine (circleFill());
	}

	IEnumerator circleFill() {
		icon.fillAmount = 0;
		float startTime = Time.time;
		while (Time.time - startTime < resetDelayTime) {
			icon.fillAmount = (Time.time - startTime) / resetDelayTime;
			yield return null;
		}
		icon.fillAmount = 1;
		manager.startGame();
	}

	public void OnMouseUp() {
		if (icon.fillAmount < 1) {
			icon.fillAmount = 1;
			StopAllCoroutines();
		}
	}
}
