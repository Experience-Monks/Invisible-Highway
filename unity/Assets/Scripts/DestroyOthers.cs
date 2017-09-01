using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * A script that destroys any trees that might be overlapping with the groundhog prefab
 */

public class DestroyOthers : MonoBehaviour
{
	void Start ()
	{
		// returns an array with all colliders that are touching or inside the sphere radius
		Collider[] colliders = Physics.OverlapSphere (transform.position, 0.2f);

		foreach (Collider col in colliders) {
			// only destroys trees but can be extended to other prefab tags
			if (col.CompareTag ("Pine") || col.CompareTag ("Trees")) {
				Destroy (col.gameObject);
			}
		}
	}
}
