using UnityEngine;
using UnityEngine.UI;

public class PeripheralScript : MonoBehaviour
{
	public Transform PanelTypeSelection;
	public GameObject ButtonStartAdvertising;
	public Text TextButtonStartAdvertising;
	public GameObject RFduino;
	public GameObject Button1Highlight;
	public GameObject LEDHighlight;
	public GameObject Button1;

	bool _advertising = false;
	bool IsAdvertising
	{
		get { return _advertising; }
		set
		{
			if (_advertising != value)
			{
				_advertising = value;

				if (_advertising)
				{
					TextButtonStartAdvertising.text = "Stop Advertising";
					Button1.SetActive (true);
				}
				else
				{
					TextButtonStartAdvertising.text = "Start Advertising";
					Button1.SetActive (false);
				}
			}
		}
	}

	public void Initialize ()
	{
		ButtonStartAdvertising.SetActive (false);
		Button1Highlight.SetActive (false);
		LEDHighlight.SetActive (false);
		Button1.SetActive (false);

		BluetoothLEHardwareInterface.Initialize (false, true, () => {

			BluetoothLEHardwareInterface.PeripheralName ("Simulated RFduino");
			BluetoothLEHardwareInterface.CreateCharacteristic ("2221", BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyRead |
			                                                   BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyNotify, 
			                                                   BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsReadable, null, 0, null);

			BluetoothLEHardwareInterface.CreateCharacteristic ("2222", BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyWrite,  
			                                                   BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsWriteable, null, 0, 
			(characteristicUUID, bytes) => {

				if (bytes.Length > 0)
				{
					if (bytes[0] == 0x01)
						LEDHighlight.SetActive (true);
					else
						LEDHighlight.SetActive (false);
				}
			});

			BluetoothLEHardwareInterface.CreateService ("2220", true, (serviceUUID) => {
				
				ButtonStartAdvertising.SetActive (true);
			});

		}, (error) => {
		});
	}
	
	public void OnBack ()
	{
		BluetoothLEHardwareInterface.DeInitialize (() => {
			BLETestScript.Show (PanelTypeSelection);
		});
	}

	public void OnStartAdvertising ()
	{
		if (IsAdvertising)
		{
			BluetoothLEHardwareInterface.StopAdvertising (() => {
				
				IsAdvertising = false;
			});
		}
		else
		{
			BluetoothLEHardwareInterface.StartAdvertising (() => {

				IsAdvertising = true;
			});
		}
	}

	public void OnButton1 ()
	{
		Button1Highlight.SetActive (!Button1Highlight.activeSelf);
		byte b = (byte)(Button1Highlight.activeSelf ? 0x01 : 0x00);
		BluetoothLEHardwareInterface.UpdateCharacteristicValue ("2221", new byte[] { b }, 1);
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
