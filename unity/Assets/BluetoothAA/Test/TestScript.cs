using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour
{
	public GUISkin skin;

	private byte[] data;
	private BluetoothDeviceScript bluetoothDeviceScript;

	private string serviceUUID = "1851";
	private string characteristicUUID = "2AFA";

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnGUI ()
	{
		GUI.skin = skin;

		if (GUI.Button (new Rect (10, 0, 600, 100), "DeInitialize"))
			BluetoothLEHardwareInterface.DeInitialize (null);
		
		if (GUI.Button (new Rect (10, 100, 300, 50), "Initialize Central"))
			bluetoothDeviceScript = BluetoothLEHardwareInterface.Initialize (true, false, null, null);

		if (GUI.Button (new Rect (10, 150, 300, 50), "Scan for 1851"))
			BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (new string[] { serviceUUID }, null);
		
		if (GUI.Button (new Rect (10, 200, 300, 50), "Scan for Any"))
			BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, null);
		
		if (GUI.Button (new Rect (10, 250, 300, 50), "Retrieve Connected"))
			BluetoothLEHardwareInterface.RetrieveListOfPeripheralsWithServices (new string[] { serviceUUID }, null);

		if (GUI.Button (new Rect (10, 300, 300, 50), "Stop Scan"))
			BluetoothLEHardwareInterface.StopScan ();
		
		if (GUI.Button (new Rect (10, 350, 300, 50), "Connect") && bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0)
			BluetoothLEHardwareInterface.ConnectToPeripheral (bluetoothDeviceScript.DiscoveredDeviceList[0], null, null, null);
		
		if (GUI.Button (new Rect (10, 400, 300, 50), "Disconnect") && bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0)
			BluetoothLEHardwareInterface.DisconnectPeripheral (bluetoothDeviceScript.DiscoveredDeviceList[0], null);

		if (GUI.Button (new Rect (10, 450, 300, 50), "Read Characteristic") && bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0)
			BluetoothLEHardwareInterface.ReadCharacteristic (bluetoothDeviceScript.DiscoveredDeviceList[0], serviceUUID, characteristicUUID, null);
		
		if (GUI.Button (new Rect (10, 500, 300, 50), "Write Characteristic") && bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0)
		{
			if (data == null)
			{
				data = new byte[64];
				for (int i = 0; i < 64; ++i)
					data[i] = (byte)i;
			}

			BluetoothLEHardwareInterface.WriteCharacteristic (bluetoothDeviceScript.DiscoveredDeviceList[0], serviceUUID, characteristicUUID, data, data.Length, true, null);
		}

		if (GUI.Button (new Rect (10, 550, 300, 50), "Subscribe Characteristic") && bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0)
			BluetoothLEHardwareInterface.SubscribeCharacteristic (bluetoothDeviceScript.DiscoveredDeviceList[0], serviceUUID, characteristicUUID, null, null);

		if (GUI.Button (new Rect (10, 600, 300, 50), "UnSubscribe Characteristic") && bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0)
			BluetoothLEHardwareInterface.UnSubscribeCharacteristic (bluetoothDeviceScript.DiscoveredDeviceList[0], serviceUUID, characteristicUUID, null);
		
		if (GUI.Button (new Rect (310, 100, 300, 100), "Initialize Peripheral"))
			BluetoothLEHardwareInterface.Initialize (false, true, null, null);

		if (GUI.Button (new Rect (310, 200, 300, 100), "Create Service\nand Characteristic"))
		{
			BluetoothLEHardwareInterface.PeripheralName ("Test Device");

			if (data == null)
			{
				data = new byte[64];
				for (int i = 0; i < 64; ++i)
					data[i] = (byte)i;
			}

			BluetoothLEHardwareInterface.CreateCharacteristic (characteristicUUID, 
			                                                   BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyRead | 
			                                                   BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyWrite | 
			                                                   BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyNotify, 
			                                                   BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsReadable |
			                                                   BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsWriteable, 
			                                                   null, 0, null);

			BluetoothLEHardwareInterface.CreateService (serviceUUID, true, null);
		}
		
		if (GUI.Button (new Rect (310, 300, 300, 100), "Start Advertising"))
			BluetoothLEHardwareInterface.StartAdvertising (null);
		
		if (GUI.Button (new Rect (310, 400, 300, 100), "Stop Advertising"))
			BluetoothLEHardwareInterface.StopAdvertising (null);
		
		if (GUI.Button (new Rect (310, 500, 300, 100), "Update Characteristic Value"))
		{
			for (int i = 0; i < data.Length; ++i)
				data[i] = (byte)(data[i] + 1);

			BluetoothLEHardwareInterface.UpdateCharacteristicValue (characteristicUUID, data, data.Length);
		}
	}
}
