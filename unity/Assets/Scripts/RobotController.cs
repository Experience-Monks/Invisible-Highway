using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * Manages the robot
 */

public class RobotController : MonoBehaviour
{
	public GameObject robot;
	public BLEController bleController;

	private bool robotMoving;
	private List<Vector2> moosedPoints;
	private int moosePointIndex;
	private bool mooseTriggered;

	void Start()
	{
		// initializations
		robotMoving = false;
	}

	void Update()
	{
		// update the robot's position
		if (robotMoving) {
			this.UpdateEstimatedRobotPosition ();
		}
	}

	void UpdateEstimatedRobotPosition()
	{
		string command = bleController.mostRecentCommand;
		if (command.StartsWith ("P")) {
			string[] parts = command.Split (':');
			int stepIndex = int.Parse (parts [1]);
			float stepProgress = float.Parse (parts [2]);

			if (stepIndex == 0) {
				// the starting step, do nothing.
				mooseTriggered = false;
				return;
			}

			int moveIndex = stepIndex / 2; // intentional integer division to floor our index

			if (stepIndex % 2 == 0) { // if it's even, we're in a movement command
				Vector2 currentPoint = moosedPoints [moveIndex - 1];
				Vector2 nextPoint = moosedPoints [moveIndex];
				Vector2 direction = (nextPoint - currentPoint).normalized;
				Vector2 offset = direction * (stepProgress - Vector2.Distance (currentPoint, nextPoint));

				Vector3 currentPosition = new Vector3 (nextPoint.x + offset.x, PathController.Instance.FloorHeight () + 0.1f, nextPoint.y + offset.y);
				robot.transform.position = currentPosition;
				robot.transform.rotation = Quaternion.LookRotation (new Vector3 (direction.x, 0.0f, direction.y), Vector3.up);
			}


			if (!mooseTriggered && moveIndex >= moosePointIndex) {
				GameObject.FindGameObjectWithTag ("Moose").GetComponent<Moose> ().StartMooseAnim ();
				mooseTriggered = true;
			}
		}
	}

	// constructs and sends the command string to the robot
	private void SendPathString()
	{
		// for a description of the protocol, refer to Protocol.md in the Robot-Firmware folder
		string robotPath = "P:";
		// keep track of whether the moose has been placed
		bool moosePlaced = false;

		List<Vector2> pathPoints = PathController.Instance.GetPathPoints ();
		moosedPoints = new List<Vector2> (pathPoints);

		// the initial position and rotation are drawn from the user's calibration of the robot
		float heading = PathController.Instance.GetInitialHeading ();

		// construct the command string that will be sent to the robot
		for (int i = 0; i < pathPoints.Count - 1; i++) {
			Vector2 moveDirection = (pathPoints [i + 1] - pathPoints [i]).normalized;
			Vector3 forwardVector = new Vector3 (moveDirection.x, 0.0f, moveDirection.y);
			float newRotation = Quaternion.LookRotation (forwardVector, Vector3.up).eulerAngles.y;
			float angleToTurn = newRotation - heading;
			float distanceToTravel = Vector2.Distance (pathPoints [i + 1], pathPoints [i]);
			robotPath += "T" + WrapAngle (angleToTurn).ToString ("0.00") + ":";
			if (i > 1 && distanceToTravel >= 1.0f && !moosePlaced) {
				robotPath += "F0.75:";	// forward
				robotPath += "W10.0:";	// wait in seconds
				robotPath += "F" + (distanceToTravel - 0.75).ToString ("0.00") + ":";
				moosePlaced = true;
				Vector2 stopPoint = pathPoints [i] + moveDirection * 0.75f;
				moosePointIndex = i + 1;
				moosedPoints.Insert (i + 1, stopPoint);
			} else {
				robotPath += "F" + distanceToTravel.ToString ("0.00") + ":";
			}
			heading += angleToTurn;
		}

		// close off path string after all points added
		robotPath += "S\n";

		// log the path
		Debug.Log (robotPath);

		bleController.sendDataBluetooth (robotPath);
	}

	// start the robot movement
	public void StartRobot()
	{
		Debug.Log ("Sending robot path over bluetooth:");
		SendPathString ();
		robotMoving = true; // Start the moving process
	}

	public void StopRobot()
	{
		// BLE command to stop
		bleController.sendDataBluetooth ("x\n");
	}

	// clamps an angle to the range [-180, +180]
	private float WrapAngle(float angle)
	{
		while (angle > 180) {
			angle -= 360;
		}
		while (angle < -180) {
			angle += 360;
		}
		return angle;
	}

	// stop the robot if we quit/stop
	void OnApplicationQuit()
	{
		StopRobot ();
	}

	void OnDestroy()
	{
		StopRobot ();
	}

	void OnApplicationPause(bool pauseStatus)
	{
		StopRobot ();
	}
}