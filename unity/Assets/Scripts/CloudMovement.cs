using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A script to move the clouds back and forth along a given direction
 */

public class CloudMovement: MonoBehaviour
{
	// directions to move in
	private float moveX, moveZ;

	// frequency with which to switch directions of movement
	private const float SWITCH_TIME = 30.0f;

	void Start ()
	{
		// decide a random movement direction in (+/+) quadrant to translate
		moveX = Random.Range (0.0002f, 0.0005f);
		moveZ = Random.Range (0.0002f, 0.0005f);
		// switch the motion of direction
		InvokeRepeating ("SwitchDirection", SWITCH_TIME, SWITCH_TIME);
	}

	void Update ()
	{
		transform.Translate (new Vector3 (moveX, 0.0f, moveZ), Space.World);
	}

	void SwitchDirection ()
	{
		// reverse direction of the movement path set in Start()
		moveX *= -1.0f;
		moveZ *= -1.0f;
	}
}
