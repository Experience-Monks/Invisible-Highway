using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * A script that creates the radial animation sequence effect as robot moves through the scene
 * Uses the Unity Legacy animation system, set model import settings accordingly
 */

public class AnimationCollider : MonoBehaviour
{
	private SphereCollider sphereCollider;

	// maximum radius of sphere collider
	private const float SPHERE_RADIUS = 1.25f;

	void Start ()
	{
		// this script and OuterAnimationCollider.cs need to use different collider types
		// this collider is smaller than the one in OuterAnimationCollider.cs to create a staggered animation
		sphereCollider = gameObject.GetComponent<SphereCollider> ();

	}

	void Update ()
	{
		if (sphereCollider.radius < SPHERE_RADIUS) {
			// grow the radius of collider to get a smooth radial intro animation
			// collider needs to keep moving to trigger animations, therefore rotate before actual motion of the collider parent
			sphereCollider.radius = Mathf.Lerp (0.0f, SPHERE_RADIUS, Time.time);
			transform.Rotate (new Vector3 (0.0f, Mathf.Lerp (0.0f, 360.0f, Time.time), 0.0f));
		}
	}

	void OnTriggerEnter (Collider col)
	{
		// for given tags, trigger the intro animation and then cycle to looped idle animation once completed
		// uses the Unity legacy animation system
		if (col.CompareTag ("Trees") || col.CompareTag ("Mountains") || col.CompareTag ("Pine") || col.CompareTag ("Groundhog")) {
			col.GetComponent<Animation> ().Play ("animIn");
			col.GetComponent<Animation> ().PlayQueued ("idle", QueueMode.CompleteOthers);
			// set the tag to spawn so that the animation does not get re-triggered a second time
			col.tag = "Spawn";
		}
	}
}
