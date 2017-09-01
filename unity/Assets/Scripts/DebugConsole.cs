using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * A script to show debug console logs in the built Unity app
 */


public class DebugConsole : MonoBehaviour
{
	// color coding for different log types
	private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color> () {
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};

	private struct Log
	{
		public string message;
		public string stackTrace;
		public LogType type;
	}

	private List<Log> logs;
	private Vector2 scrollPosition;
	private bool show, collapse;

	private const int MARGIN = 20;
	private const int TOP_MARGIN = 500;

	private Rect windowRect, titleBarRect;
	private GUIContent clearLabel, collapseLabel;

	void OnEnable ()
	{
		// variable initializations
		windowRect = new Rect (MARGIN, TOP_MARGIN, Screen.width - (MARGIN * 2), Screen.height - (MARGIN * 35));
		titleBarRect = new Rect (0, 0, 10000, 20);
		clearLabel = new GUIContent ("Clear", "Clear the contents of the console.");
		collapseLabel = new GUIContent ("Collapse", "Hide repeated messages.");
		logs = new List<Log> ();

		show = false;
		Application.logMessageReceived += HandleLog;
	}

	void OnDisable ()
	{
		Application.logMessageReceived -= HandleLog;
	}

	void OnGUI ()
	{
		if (!show) {
			return;
		}
		// creates the GUI pop-up window with debug logs
		windowRect = GUILayout.Window (123456, windowRect, ConsoleWindow, "Console");
	}

	// the window that displays the console logs from Unity
	void ConsoleWindow (int windowID)
	{
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);

		// iterate through recorded logs
		for (int i = 0; i < logs.Count; i++) {
			Log log = logs [i];

			// collapse identical logs
			if (collapse) {
				bool messageSameAsPrevious = i > 0 && log.message == logs [i - 1].message;
				if (messageSameAsPrevious) {
					continue;
				}
			}

			GUI.contentColor = logTypeColors [log.type];
			GUILayout.Label (log.message);
		}

		GUILayout.EndScrollView ();
		GUI.contentColor = Color.white;
		GUILayout.BeginHorizontal ();

		if (GUILayout.Button (clearLabel)) {
			logs.Clear ();
		}

		// toggles the collapse and un-collapse of log messages
		collapse = GUILayout.Toggle (collapse, collapseLabel, GUILayout.ExpandWidth (false));

		GUILayout.EndHorizontal ();

		// dragging the title bar moves the debug window
		GUI.DragWindow (titleBarRect);
	}

	// called by button in scene UI Canvas
	public void ShowConsole (bool toggle)
	{
		show = toggle;
		DevControls.Instance.Active = show;
	}

	// record full log from callback
	void HandleLog (string message, string stackTrace, LogType type)
	{
		logs.Add (new Log () {
			message = message,
			stackTrace = stackTrace,
			type = type,
		});
	}
}