using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/* 
 * An animation effect for the pieces of the road appearing in the scene
 */
public class AnimationRoad : MonoBehaviour {

	public enum Type {
		Straight,
		Join
	}

	// the type of road
	public Type type;

	// the delay for the animation in seconds
	public float delay = 0f;

	// a start time for the animation
	private float startTime;

	// a reference to the initial scale of the object
	private Vector3 initialScale;

	// the time in seconds after which this script can be safely removed
	private const float KILL_TIME = 7f;

	// Use this for initialization
	void Start () {
		initialScale = gameObject.transform.localScale;
		startTime = Time.time + delay;
		if(type == Type.Straight)
			gameObject.transform.localScale = new Vector3 (initialScale.x, initialScale.y, 0.0f);
		else
			gameObject.transform.localScale = new Vector3 (0.0f, initialScale.y, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
		// animate the scale according to the type
		if(type == Type.Straight)
			gameObject.transform.localScale = Vector3.Lerp(new Vector3(initialScale.x, initialScale.y, 0f), initialScale, (Time.time - startTime));
		else
			gameObject.transform.localScale = Vector3.Lerp(new Vector3(0f, initialScale.y, 0f), initialScale, (Time.time - startTime));

		// remove this behaviour after a while to improve performance
		if(Time.time - startTime > KILL_TIME) Destroy(this);
	}
}
