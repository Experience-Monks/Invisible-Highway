using UnityEngine;
using UnityEngine.UI;

public class BusyScript : MonoBehaviour
{
	public Image ImageBusy;
	public bool Busy = false;

	static public BusyScript BusyScriptObject;
	static public bool IsBusy
	{
		get { return BusyScriptObject != null ? BusyScriptObject.Busy : false; }
		set
		{
			if (BusyScriptObject != null)
			{
				BusyScriptObject.Busy = value;
				BusyScriptObject.ImageBusy.gameObject.SetActive (BusyScriptObject.Busy);
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		BusyScriptObject = this;
	}
	
	// Update is called once per frame
	void Update ()
	{
		ImageBusy.transform.Rotate (0f, 0f, 20f * Time.deltaTime);
	}
}
