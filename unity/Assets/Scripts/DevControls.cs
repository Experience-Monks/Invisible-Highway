using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * Dev Controls mainly to help adjust the Chroma Key color and threshold
 */

public class DevControls : MonoBehaviour {

	// padding for the GUI controls
	private const int TOP = 10;
	private const int LEFT = 10;
	private const int WIDTH = 100;
	private const int HEIGHT = 30;

	// start inactive
	public bool Active = false;

	// singleton
	private static DevControls _instance = null;
	public static DevControls Instance {
		get {return _instance; }
	}
	void Awake() {
		_instance = this;
	}

	// draw GUI
	void OnGUI() {

		if(!Active) return;

		Color c = ARChromakeyHelper.Instance.maskColor;

		// scale up GUI
		float scaleX, scaleY;

		if (Screen.orientation == ScreenOrientation.Portrait) {
			scaleX = Screen.width / 360.0f;
			scaleY = Screen.height / 640.0f;
		} else {
			scaleX = Screen.width / 640.0f;
			scaleY = Screen.height / 360.0f;
		}

		GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(scaleX, scaleY, 1));
		
		// red
		c.r = GUI.HorizontalSlider(new Rect(LEFT, TOP + HEIGHT * 0, WIDTH, HEIGHT), c.r, 0f, 1f);
		GUI.Label(new Rect(LEFT + WIDTH + LEFT, TOP + HEIGHT * 0, WIDTH, HEIGHT), "R " + c.r.ToString("F2"));
		// green
		c.g = GUI.HorizontalSlider(new Rect(LEFT, TOP + HEIGHT * 1, WIDTH, HEIGHT), c.g, 0f, 1f);
		GUI.Label(new Rect(LEFT + WIDTH + LEFT, TOP + HEIGHT * 1, WIDTH, HEIGHT), "G " + c.g.ToString("F2"));
		// blue
		c.b = GUI.HorizontalSlider(new Rect(LEFT, TOP + HEIGHT * 2, WIDTH, HEIGHT), c.b, 0f, 1f);
		GUI.Label(new Rect(LEFT + WIDTH + LEFT, TOP + HEIGHT * 2, WIDTH, HEIGHT), "B " + c.b.ToString("F2"));
		// thresolh
		ARChromakeyHelper.Instance.threshold = GUI.HorizontalSlider(new Rect(LEFT, TOP + HEIGHT * 3, WIDTH, HEIGHT), ARChromakeyHelper.Instance.threshold, 0f, 1f);
		GUI.Label(new Rect(LEFT + WIDTH + LEFT, TOP + HEIGHT * 3, WIDTH, HEIGHT), "Threshold " + ARChromakeyHelper.Instance.threshold.ToString("F2"));
		// color
		DrawColor(new Rect(LEFT, TOP + HEIGHT * 4, WIDTH, HEIGHT), c);
		// update button
		if(GUI.Button(new Rect(LEFT, TOP + HEIGHT * 5, WIDTH, HEIGHT), "Update Materials")) {
			ARChromakeyHelper.Instance.UpdateMaterials();
		}

		ARChromakeyHelper.Instance.maskColor = c;
	}

	void DrawColor(Rect position, Color color) {
		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0, 0, color);
		texture.Apply();
		GUI.skin.box.normal.background = texture;
		GUI.Box(position, GUIContent.none);
	}

}
