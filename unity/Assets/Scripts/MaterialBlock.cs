using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * A script used with the material property block shader
 * Allows to draw multiple objects slightly different properties of the same material without actually creating multiple materials/copies
 */

public class MaterialBlock : MonoBehaviour
{
	// MaterialPropertyBlock holds keys and values for the shader properties
	private MaterialPropertyBlock materialPropertyBlock;

	private Renderer currentRenderer;

	private float value;

	void Start ()
	{
		// MaterialPropertyBlock holds keys and values for the shader properties
		materialPropertyBlock = new MaterialPropertyBlock ();
		currentRenderer = GetComponent<Renderer> ();

		// change the color of this specific material instance
		ChangeColor ();
	}

	public void ChangeColor ()
	{
		// retrieve current material property values for this renderer
		currentRenderer.GetPropertyBlock (materialPropertyBlock);

		// set a new value for r,g,b,a (Unity CG defines color in (0.0, 1.0) space) so divide by 255
		value = Random.Range (128.0f, 255.0f) / 255.0f;
		materialPropertyBlock.SetColor ("_Color", new Color (value, value, value, 1.0f));

		// apply the new values to the renderer
		currentRenderer.SetPropertyBlock (materialPropertyBlock);
	}
}
