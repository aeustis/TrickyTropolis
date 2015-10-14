using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelButtonScript : MonoBehaviour {
	public GameObject flag, trophy;
	public Text buttonText;

	public void OnClick() {
		SendMessageUpwards ("levelButtonPressed", this);
	}
}
