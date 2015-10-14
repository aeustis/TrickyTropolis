using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DifficultyButtonScript : MonoBehaviour {
	public Text buttonText, flagText, trophyText;
	public GameObject padlock, flag, trophy;

	public void OnClick() {
		SendMessageUpwards ("difficultyButtonPressed", this);
	}
}
