using UnityEngine;
using System.Collections;

public class TypeSelectionScript : MonoBehaviour
{
	public CentralScript PanelCentral;
	public PeripheralScript PanelPeripheral;

	public GameObject ButtonPeripheral;

	public void OnCentral ()
	{
		PanelCentral.Initialize ();
		BLETestScript.Show (PanelCentral.gameObject.transform);
	}

	public void OnPeripheral ()
	{
		PanelPeripheral.Initialize ();
		BLETestScript.Show (PanelPeripheral.gameObject.transform);
	}

	void Start ()
	{
#if UNITY_ANDROID
		ButtonPeripheral.SetActive (false);
#endif
	}
}
