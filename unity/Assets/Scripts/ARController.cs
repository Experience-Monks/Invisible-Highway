//-----------------------------------------------------------------------
//
// This file is created from the HelloARController.cs
// provided by Google in the ARCore Unity Examples folder.
// Please refer to that file for more information.
//
//-----------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

// A base controller that handles finding a ground floor, based on the ARCore HelloAR example app

public class ARController : MonoBehaviour
{
    // The first-person camera being used to render the passthrough camera.
    public Camera m_firstPersonCamera;

    // A gameobject parenting UI for displaying the "searching for planes" snackbar.
    public GameObject m_searchingForPlaneUI;

	// Returns the first plane found
	public TrackedPlane FirstGroundPlane {
		get {
			if(m_allPlanes.Count > 0) return m_allPlanes[0];
			else return null;
		}
	}

    private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

    // The Unity Update() method.
    public void Update ()
    {
        QuitOnConnectionErrors();

        // The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
        if (Frame.TrackingState != FrameTrackingState.Tracking)
        {
            const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
            Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Disable the snackbar UI when no planes are valid.
        bool showSearchingUI = true;

        Frame.GetAllPlanes(ref m_allPlanes);
        for (int i = 0; i < m_allPlanes.Count; i++)
        {
            if (m_allPlanes[i].IsValid)
            {
                showSearchingUI = false;
                break;
            }
        }
        m_searchingForPlaneUI.SetActive(showSearchingUI);
    }

    // Quit the application if there was a connection error for the ARCore session.
    private void QuitOnConnectionErrors()
    {
        // Do not update if ARCore is not tracking.
        if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported)
        {
            ShowAndroidToastMessage("This device does not support ARCore.");
            Application.Quit();
        }
        else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
        {
            ShowAndroidToastMessage("Camera permission is needed to run this application.");
            Application.Quit();
        }
        else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
        {
            ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            Application.Quit();
        }
    }

    // Show an Android toast message
    private static void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}