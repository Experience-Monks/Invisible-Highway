using UnityEngine;
using UnityEngine.UI;

public class CentralPeripheralButtonScript : MonoBehaviour
{
	public CentralRFduinoScript PanelCentralRFduino;
	public CentralTISensorTagScript PanelCentralTISensorTag;
	public CentralNordicScript PanelCentralNordic;
	public Text TextName;
	public Text TextAddress;
	
	public void OnPeripheralSelected ()
	{
		if (TextName.text.Contains ("RFduino"))
		{
			PanelCentralRFduino.Initialize (this);
			BLETestScript.Show (PanelCentralRFduino.transform);
		}
		else if (TextName.text.Contains ("SensorTag"))
		{
			PanelCentralTISensorTag.Initialize (this);
			BLETestScript.Show (PanelCentralTISensorTag.transform);
		}
		else if (TextName.text.Contains ("Adafruit Bluefruit LE"))
		{
			PanelCentralNordic.Initialize (this);
			BLETestScript.Show (PanelCentralNordic.transform);
		}
	}
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
