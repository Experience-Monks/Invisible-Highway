using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * The bahaviour of the UndoButton that is attached to the latest path point placed.
 * It always turns to face the camera and once tapped removes that last point
 */

public class UndoButton : MonoBehaviour
{
	// keep the asset rotating on the Y axis to face the camera
	void Update()
	{
		Vector3 fwd = Camera.main.transform.forward;
		fwd.y = 0.0f;
	
		// multiply by (0,90,0) to rotate the asset 90 to the camera
		transform.rotation = Quaternion.LookRotation (fwd, Vector3.up) * Quaternion.Euler (Vector3.up * 90.0f);
	}

	// handle the tap
	void OnMouseDown()
	{
		PathController.Instance.Undo ();
	}
}
