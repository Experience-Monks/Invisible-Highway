using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * A script to move the balloon in a pseudo-random flight path above the environment
 */

public class MoveBalloon : MonoBehaviour
{
	// the speed at which the balloon flies
	public float speed;

	// controls the variablility in height that the balloon flies between
	public float amplitude;

	// offsets the center of flight path
	private float xOffset, zOffset;

	// the range of values that the flight can deviate from the center
	private float xRange, zRange;

	// average height of the balloon above the environment
	private const float BALLOON_HEIGHT = 0.75f;

	// speed at which the balloon rotates
	private const float SPIN_SPEED = 15.0f;

	public void SetOffsetAndRange (Vector3 offset, Vector2 range)
	{
		// moves the center of the flight path to the center of the plane defining the environment
		xOffset = offset.x;
		zOffset = offset.z;

		// uses half the range but applied to both sides since perlin noise is from (-1, 1)
		xRange = range.x * 0.5f;
		zRange = range.y * 0.5f;
	}

	void Update ()
	{
		//perlin noise needs to be converted from (0, 1) to (-1, 1) space

		// uses the x-axis of the perlin noise sample and offset to z-center of environment plane
		float posX = xRange * (Mathf.PerlinNoise (Time.time * speed, 0.0f) * 2.0f - 1.0f) + xOffset;

		// modulates the height of the balloon based on detected floor height
		float posY = amplitude * Mathf.Sin (Time.time) + PathController.Instance.FloorHeight () + BALLOON_HEIGHT;

		// uses the y-axis of the perlin noise sample and offset to z-center of environment plane
		float posZ = zRange * (Mathf.PerlinNoise (0.0f, Time.time * speed) * 2.0f - 1.0f) + zOffset;

		// update the position of the balloon and spin it around about its vertical axis
		transform.position = new Vector3 (posX, posY, posZ);
		transform.RotateAround (Vector3.zero, Vector3.up, Time.deltaTime * SPIN_SPEED);
	}
}
