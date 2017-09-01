using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * A script to handle the movement and animation of the moose
 * Animation is done using the Unity Mecanim animator
 */

public class Moose : MonoBehaviour
{
	// Mecanim animation variables
	private Animator anim;
	private AnimatorStateInfo animState;

	// the maximum speed for the moose translation
	private const float MAX_SPEED = 0.006f;

	// current movement speed
	private float speed;

	// a time to provide consistency across animation lerps
	private float startTime;

	void Start ()
	{
		anim = gameObject.GetComponent<Animator> ();
	}

	void Update ()
	{
		// returns the current state of the Mecanim animator, 
		animState = anim.GetCurrentAnimatorStateInfo (0);

		// "StartWalk" occurs during the first walk loop in a chain of walk sequences
		if (animState.IsTag ("StartWalk")) {
			// ramp up the movement speed to look more natural in animation when moose starts walking
			speed = Mathf.Lerp (0.0f, MAX_SPEED, (Time.time - startTime) * 1.2f);
			transform.Translate (Vector3.forward * speed, Space.Self);
		}

		// "Walk" moves the moose at top speed
		if (animState.IsTag ("Walk")) {
			transform.Translate (Vector3.forward * MAX_SPEED, Space.Self);
		}

		// "Look" triggers the moose to look to its left at the car
		if (animState.IsTag ("Look")) {
			startTime = Time.time;
		}

	}
		
	// sets the "start" bool to bring Mecanim out of its static animation state
	// triggered when the robot reaches the corner prior to the strip of road that the moose is on
	public void StartMooseAnim ()
	{
		anim.SetBool ("start", true);
		startTime = Time.time;
	}

}
