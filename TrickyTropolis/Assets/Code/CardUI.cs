using UnityEngine;
using System.Collections;

public class CardUI : MonoBehaviour {
	public UIManager manager;
	public bool dragEnabled = true;
	public float clickDelay = .15f;
	public float normalScale = 1f / 3;
	internal Vector3 snapPos = Vector3.zero;

	internal bool zoomed = false;
	internal Vector3 zoomPos;
	bool justPressed = false;

	internal int handColumn;
    internal GameCard gameCard;

	// Use this for initialization
	void Start () {

	}

	void OnMouseDown() {
		StopAllCoroutines ();

		//If the manager has another card selected, deselect it
		if (manager.selectedCard != this) {
			manager.deselectCard ();
			manager.selectedCard = this;
		}

		//Set z=0, begin release timer
		transform.position -= new Vector3 (0, 0, transform.position.z);
		justPressed = true;
		StartCoroutine(TimedRelease());

		//If card is zoomed already, don't start dragging until the timed release
		if (zoomed)	return;

		//Otherwise, begin dragging with offset.
		if (dragEnabled) {
			Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			manager.dragOffset = transform.position - mousePos;
			manager.onDragStart ();
		}
	}

	IEnumerator TimedRelease() {
		yield return new WaitForSeconds( clickDelay );
		justPressed = false;

		//If the card is zoomed and we're still holding the button, 
		//unzoom and begin dragging it at the current mouse position
		if (zoomed && dragEnabled) {
			setZoom(false);
			Vector3 mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			transform.position = mousePos - new Vector3(0,0,mousePos.z);
			manager.dragOffset = transform.position - mousePos;
			manager.onDragStart();
		}
	}

	public void flyTo( Vector3 target, float maxSpeed, float dampDistance ) {
		StopAllCoroutines ();
		StartCoroutine( flyToHelper( target, maxSpeed, dampDistance ) );
	}

	private IEnumerator flyToHelper( Vector3 target, float maxSpeed, float dampDistance ) {
		//Begin by setting the z component so that the flight occurs in fixed z plane
		transform.position = new Vector3 (transform.position.x, transform.position.y, target.z);

		for (;;) {
			float dist = Vector3.Distance( target, transform.position );
			if( dist < .001 ) break;

			float speed = maxSpeed;
			if( dist < dampDistance ) speed *= dist / dampDistance;
			transform.position += Vector3.ClampMagnitude(target - transform.position, speed*Time.deltaTime);
			yield return null;
		}
		transform.position = target;
	}

	void OnMouseUp() {
		//On click, toggle the zoom state and deselect if we just unzoomed
		if (justPressed) {
			setZoom (!zoomed);
			if( !zoomed ) manager.selectedCard = null;
			StopAllCoroutines ();
			justPressed = false;
		} else {
		//We just released after a drag
			manager.onDragRelease ();
		}

	}

	internal void setZoom( bool z ) {
		if (zoomed == z) return;
		zoomed = z;
		if (zoomed) {
			transform.localScale = new Vector3 (1, 1, 1);
			transform.position = zoomPos;
		} else {
			transform.localScale *= normalScale;
			transform.position = snapPos;
		}
	}
}
