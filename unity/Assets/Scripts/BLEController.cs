using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using GoogleARCore;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

// This class connects to the Robot via Bluetooth and sends data out to the phone.

public class BLEController : MonoBehaviour
{
	public bool connecting;
	public string mostRecentCommand;

	//Adafruit Feather https://learn.adafruit.com/adafruit-feather-32u4-bluefruit-le/uart-service
	//UART Service Base UUID: 6E400001-B5A3-F393-­E0A9-­E50E24DCCA9E
	private string _FullUID = "6E40****-B5A3-F393-E0A9-E50E24DCCA9E";
	private string _serviceUUID = "0001";
	//Nordic’s UART Service includes the following characteristics:
	//Write Characteristic UUID - Send data back to the sensor node, can be written to by the phone
	private string TX_UUID = "0002";
	//Read Characteristic UUID - Send data out to the phone. Notify can be enabled by the phone so that an alert is raised every time the TX channel is updated.
	private string RX_UUID = "0003";

	private string deviceToConnectTo = "HELP";
	public bool isConnected = false;
	private bool _readFound = false;
	private bool _writeFound = false;
	private string _connectedID = null;
	private string recvBuffer;
	private Dictionary<string, string> _peripheralList;
	private float _subscribingTimeout = 0f;

	// the android permissions we need to get
	const string ANDROID_ACCESS_COARSE_LOCATION_PERMISSION_NAME = "android.permission.ACCESS_COARSE_LOCATION";
	const string ANDROID_BLUETOOTH_ADMIN_PERMISSION_NAME = "android.permission.BLUETOOTH_ADMIN";
	const string ANDROID_BLUETOOTH_PERMISSION_NAME = "android.permission.BLUETOOTH";
	private const int MAX_RETRIES = 100;
	private List<string> permissions;

	// Use this for initialization
	void Start()
	{
		// check for the following permissions, then scan for the device
		permissions = new List<string> (new string[] {
			ANDROID_ACCESS_COARSE_LOCATION_PERMISSION_NAME,
			ANDROID_BLUETOOTH_ADMIN_PERMISSION_NAME,
			ANDROID_BLUETOOTH_PERMISSION_NAME
		});
		StartCoroutine (CheckPermissions ());
	}

	// check for permissions one by one and when done it starts the bluetooth scanning
	private IEnumerator CheckPermissions()
	{
		int retries = 0;

		while (permissions.Count > 0 && retries < MAX_RETRIES) {
			string permission = permissions [0];
			if (AndroidPermissionsManager.IsPermissionGranted (permission)) {
				// done with this permission
				permissions.RemoveAt (0);
			} else if (CheckPermission (permission) == null) {
				// the permission could not be requested, probably another permission was being requested at the time
				retries++;
				Debug.Log ("Permission " + permission + " could not be requested at this time. Retrying (" + retries + "/" + MAX_RETRIES + ").");
				yield return new WaitForSeconds (1f);
			} else {
				// permission requested and we assume accepted - it's just an experiment after all :)
				permissions.RemoveAt (0);
			}
		}

		// we ran out of permissions to check, so let's connect
		Debug.Log ("Done with permissions. Starting Bluetooth.");
		StartBluetooth ();
	}

	// check for a specific android permission using the (WIP) AndroidPermissionManager.RequestPermission() method
	// if the AsyncTask returned is null the permission was not requested and we need to try again, otherwise it is requested
	private AsyncTask<AndroidPermissionsRequestResult> CheckPermission(string permission)
	{
		// Request needed permission and attempt service connection if granted.
		Debug.Log ("Checking permission: " + permission);
		string[] permissionsArray = new string[] { permission };

		// return the AsyncTask object, if it's null we know the request was not processed, otherwise it went through
		return AndroidPermissionsManager.RequestPermission (permissionsArray);
	}

	// start trying to find a connect to the robot
	private void StartBluetooth()
	{
		BluetoothLEHardwareInterface.Initialize (true, false, () => {
		},
			(error) => {
			});
		Invoke ("scan", 1.0f);
		connecting = false;
	}

	// the Unity MonoBehaviour Update() mathod
	void Update()
	{
		if (_readFound && _writeFound) {
			_readFound = false;
			_writeFound = false;
			_subscribingTimeout = 3.0f;
		}

		if (_subscribingTimeout > 0.0f) {
			_subscribingTimeout -= Time.deltaTime;
			if (_subscribingTimeout <= 0.0f) {
				_subscribingTimeout = 0.0f;
				BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (
					_connectedID, FullUUID (_serviceUUID), FullUUID (RX_UUID),
					(deviceAddress, notification) => {
					},
					(deviceAddress2, characteristic, data) => {
						BluetoothLEHardwareInterface.Log ("id: " + _connectedID);
						if (deviceAddress2.CompareTo (_connectedID) == 0) {
							BluetoothLEHardwareInterface.Log (string.Format ("data length: {0}", data.Length));
							if (data.Length == 0) {
								// do nothing 
							} else {
								//Data 
								string s = ASCIIEncoding.UTF8.GetString (data);
								BluetoothLEHardwareInterface.Log ("data: " + s);
								receiveText (s);
							}
						}
					});
			}
		}
	}

	void receiveText(string s)
	{
		recvBuffer = recvBuffer + s;
		if (recvBuffer.Contains ("\n")) {
			var split = recvBuffer.Split ("\n".ToCharArray (), 2);
			recvBuffer = split [1];
			var command = split [0];
			mostRecentCommand = command;
		}
	}

	public void sendDataBluetooth(string sData)
	{
		if (sData.Length > 0) {
			byte[] bytes = ASCIIEncoding.UTF8.GetBytes (sData);
			if (bytes.Length > 0) {
				sendBytesBluetooth (bytes);
			}
		}
	}

	void sendBytesBluetooth(byte[] data)
	{
		// split packet into 20 byte chunks for BLE UART, alternatively can use long write but will have overhead in byte buffer
		byte[] toSend = new byte[20];
		byte[] nextPacket = new byte[1];
		bool needsNextPacket = data.Length > 20;
		if (needsNextPacket) {
			nextPacket = new byte[data.Length - 20];
			for (int i = 0; i < data.Length; i++) {
				if (i < 20) {
					toSend [i] = data [i];
				} else {
					nextPacket [i - 20] = data [i];
				}
			}
		} else {
			toSend = data;
		}

		BluetoothLEHardwareInterface.Log (string.Format ("data length: {0} uuid {1}", data.Length.ToString (), FullUUID (TX_UUID)));
		BluetoothLEHardwareInterface.WriteCharacteristic (_connectedID, FullUUID (_serviceUUID), FullUUID (TX_UUID),
			toSend, toSend.Length, true, (characteristicUUID) => {
			BluetoothLEHardwareInterface.Log ("Write succeeded");
			if (needsNextPacket) {
				sendBytesBluetooth (nextPacket);
			}
		}
		);
	}

	void scan()
	{
		// the first callback will only get called the first time this device is seen 
		// this is because it gets added to a list in the BluetoothDeviceScript 
		// after that only the second callback will get called and only if there is 
		// advertising data available 
		Debug.Log ("Starting scan \r\n");

		BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, name) => {
			AddPeripheral (name, address);
		}, (address, name, rssi, advertisingInfo) => {
		});
	}

	void AddPeripheral(string name, string address)
	{
		if (_peripheralList == null) {
			_peripheralList = new Dictionary<string, string> ();
		}
		if (!_peripheralList.ContainsKey (address) && !connecting) {

			if (name == deviceToConnectTo) {
				Debug.Log ("Found " + name + " \r\n");
			}

			_peripheralList [address] = name;
			if (name.Trim ().ToLower () == deviceToConnectTo.Trim ().ToLower ()) {
				//BluetoothLEHardwareInterface.StopScan (); 
				Debug.Log ("Connecting to: " + address + "\n");
				connectBluetooth (address, name);

			} else {
				if (!connecting) {
					Debug.Log ("Not the bluetooth device we are looking for");
				}
			}
		} else {
			Debug.Log ("No address found");
		}
	}

	void connectBluetooth(string addr, string name)
	{
		Debug.Log ("Connection to ..." + name + "with address: " + addr + ", in progress... \n");
		connecting = true;
		BluetoothLEHardwareInterface.ConnectToPeripheral (addr, (address) => {
		},
			(address, serviceUUID) => {
			},
			(address, serviceUUID, characteristicUUID) => {
				// discovered characteristic 
				if (IsEqual (serviceUUID, _serviceUUID)) {
					_connectedID = address;
					isConnected = true;

					if (IsEqual (characteristicUUID, RX_UUID)) {
						Debug.Log ("Read Characteristic");
						_readFound = true;
					}
					if (IsEqual (characteristicUUID, TX_UUID)) {
						Debug.Log ("Write Characteristic");
						_writeFound = true;
					}
					Debug.Log ("Connected");

					BluetoothLEHardwareInterface.StopScan ();
				}
			}, (address) => {
			// this will get called when the device disconnects 
			// be aware that this will also get called when the disconnect 
			// is called above. both methods get call for the same action 
			// this is for backwards compatibility 
			isConnected = false;
			connecting = false;
		});

	}

	// helper function for handling connection strings
	string FullUUID(string uuid)
	{
		return _FullUID.Replace ("****", uuid);
	}

	//  helper function for handling connection strings
	bool IsEqual(string uuid1, string uuid2)
	{
		if (uuid1.Length == 4) {
			uuid1 = FullUUID (uuid1);
		}
		if (uuid2.Length == 4) {
			uuid2 = FullUUID (uuid2);
		}
		return (uuid1.ToUpper ().CompareTo (uuid2.ToUpper ()) == 0);
	}

}