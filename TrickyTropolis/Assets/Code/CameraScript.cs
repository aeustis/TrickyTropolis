using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	public void panTo( Vector3 target, float maxSpeed, float dampDistance ) {
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
}
