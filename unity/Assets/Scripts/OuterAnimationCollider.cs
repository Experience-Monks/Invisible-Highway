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

public class OuterAnimationCollider : MonoBehaviour
{
	// the robot to follow in the scene
	public GameObject robot;

	private CapsuleCollider capsuleCollider;

	// maximum radius of sphere collider
	private const float CAPSULE_RADIUS = 0.05f;

	// maximum height of sphere collider
	private const float CAPSULE_HEIGHT = 0.35f;

	void Start ()
	{
		// this script and AnimationCollider.cs need to use different collider types
		// this collider is larger than the one in AnimationCollider.cs to create a staggered animation
		capsuleCollider = gameObject.GetComponent<CapsuleCollider> ();
	}

	void LateUpdate ()
	{
		if (capsuleCollider.radius < CAPSULE_RADIUS) {
			// grow the radius of collider to get a smooth radial intro animation
			// collider needs to keep moving to trigger animations, therefore rotate before actual motion
			capsuleCollider.radius = Mathf.Lerp (0.0f, CAPSULE_RADIUS, (Time.time) * 0.75f);
			capsuleCollider.height = Mathf.Lerp (0.0f, CAPSULE_HEIGHT, (Time.time) * 0.75f);
			transform.Rotate (new Vector3 (0.0f, 0.0f, Mathf.Lerp (0.0f, 360.0f, (Time.time) * 0.5f)));
		} else {
			// collider follows the robot gameObject, hence in LateUpdate()
			transform.position = robot.transform.position;
			transform.rotation = Quaternion.Euler (robot.transform.forward);
		}
	}

	void OnTriggerEnter (Collider col)
	{
		// set the mesh renderer to enabled since there is no animation
		// mesh renderers for these objects should be set to false in prefabs
		if (col.CompareTag ("Rocks")) {
			col.GetComponent<MeshRenderer> ().enabled = true;
			col.tag = "Spawn";
		}
	}
}
