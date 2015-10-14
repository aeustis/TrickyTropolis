using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class MessageBoxScript : MonoBehaviour {
	public Text titleText, messageText, bottomText;
	bool isOpen = false;

	//Display a message box with the given text and title.  The messages under
	//"text" will be displayed sequentially (per click).
	//Dragging a card will automatically kill all message boxes;
	string[] messages;
	int messageIndex;

	public void messageBox( string title, string text ) {
		messageBox (title, new string[] {text});
	}

	public void messageBox( string title, string[] text ) {
		if (!isOpen) open ();

		messages = text;
		messageIndex = -1;
		titleText.text = title;
		bottomText.gameObject.SetActive (true);
		OnMouseDown ();
	}

	public void OnMouseDown() {
		++messageIndex;
		if (messageIndex >= messages.Length) {
			close ();
			return;
		}
		if( messageIndex == messages.Length-1 ) {
			bottomText.gameObject.SetActive(false);
		}
		messageText.text = messages [messageIndex];
	}

	void open() {
		//TODO: animate this
		gameObject.SetActive (true); 
		isOpen = true;
	}

	public void close() {
		//TODO: animate this
		gameObject.SetActive (false); 
		isOpen = false;
	}
}