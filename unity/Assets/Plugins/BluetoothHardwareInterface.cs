using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class BluetoothLEHardwareInterface
{
	public enum CBCharacteristicProperties
	{
		CBCharacteristicPropertyBroadcast = 0x01,
		CBCharacteristicPropertyRead = 0x02,
		CBCharacteristicPropertyWriteWithoutResponse = 0x04,
		CBCharacteristicPropertyWrite = 0x08,
		CBCharacteristicPropertyNotify = 0x10,
		CBCharacteristicPropertyIndicate = 0x20,
		CBCharacteristicPropertyAuthenticatedSignedWrites = 0x40,
		CBCharacteristicPropertyExtendedProperties = 0x80,
		CBCharacteristicPropertyNotifyEncryptionRequired = 0x100,
		CBCharacteristicPropertyIndicateEncryptionRequired = 0x200,
	};

	public  enum CBAttributePermissions
	{
		CBAttributePermissionsReadable = 0x01,
		CBAttributePermissionsWriteable = 0x02,
		CBAttributePermissionsReadEncryptionRequired = 0x04,
		CBAttributePermissionsWriteEncryptionRequired = 0x08,
	};

	#if UNITY_IPHONE || UNITY_TVOS
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLELog (string message);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEInitialize (bool asCentral, bool asPeripheral);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDeInitialize ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPauseMessages (bool isPaused);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEScanForPeripheralsWithServices (string serviceUUIDsString, bool allowDuplicates, bool rssiOnly);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERetrieveListOfPeripheralsWithServices (string serviceUUIDsString);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopScan ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEConnectToPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEReadCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEWriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLESubscribeCharacteristic (string name, string service, string characteristic);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUnSubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectAll ();

#if !UNITY_TVOS
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPeripheralName (string newName);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateService (string uuid, bool primary);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveService (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveServices ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateCharacteristic (string uuid, int properties, int permissions, byte[] data, int length);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristic (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristics ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStartAdvertising ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopAdvertising ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUpdateCharacteristicValue (string uuid, byte[] data, int length);
#endif
#elif UNITY_ANDROID
	static AndroidJavaObject _android = null;
#endif

	private static BluetoothDeviceScript bluetoothDeviceScript;

	public static void Log (string message)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLELog (message);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothLog", message);
#endif
		}
	}

	public static BluetoothDeviceScript Initialize (bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
	{
		bluetoothDeviceScript = null;

		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver == null)
		{
			bluetoothLEReceiver = new GameObject("BluetoothLEReceiver");

			bluetoothDeviceScript = bluetoothLEReceiver.AddComponent<BluetoothDeviceScript>();
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.InitializedAction = action;
				bluetoothDeviceScript.ErrorAction = errorAction;
			}
		}

		GameObject.DontDestroyOnLoad (bluetoothLEReceiver);

		if (Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.SendMessage ("OnBluetoothMessage", "Initialized");
		}
		else
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEInitialize (asCentral, asPeripheral);
#elif UNITY_ANDROID
			if (_android == null)
			{
				AndroidJavaClass javaClass = new AndroidJavaClass ("com.shatalmic.unityandroidbluetoothlelib.UnityBluetoothLE");
				_android = javaClass.CallStatic<AndroidJavaObject> ("getInstance");
			}

			if (_android != null)
				_android.Call ("androidBluetoothInitialize", asCentral, asPeripheral);
#endif
		}

		return bluetoothDeviceScript;
	}
	
	public static void DeInitialize (Action action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DeinitializedAction = action;

		if (Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.SendMessage ("OnBluetoothMessage", "DeInitialized");
		}
		else
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEDeInitialize ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothDeInitialize");
#endif
		}
	}

	public static void FinishDeInitialize ()
	{
		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver != null)
			GameObject.Destroy(bluetoothLEReceiver);
	}

	public static void PauseMessages (bool isPaused)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEPauseMessages (isPaused);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothPause", isPaused);
#endif
		}
	}
	
	public static void ScanForPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action, Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.DiscoveredPeripheralAction = action;
				bluetoothDeviceScript.DiscoveredPeripheralWithAdvertisingInfoAction = actionAdvertisingInfo;

				if (bluetoothDeviceScript.DiscoveredDeviceList != null)
					bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
			}

			string serviceUUIDsString = null;

			if (serviceUUIDs != null && serviceUUIDs.Length > 0)
			{
				serviceUUIDsString = "";

				foreach (string serviceUUID in serviceUUIDs)
					serviceUUIDsString += serviceUUID + "|";

				serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEScanForPeripheralsWithServices (serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly);
#elif UNITY_ANDROID
			if (_android != null)
			{
				if (serviceUUIDsString == null)
					serviceUUIDsString = "";

				_android.Call ("androidBluetoothScanForPeripheralsWithServices", serviceUUIDsString, rssiOnly);
			}
#endif
		}
	}
	
	public static void RetrieveListOfPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.RetrievedConnectedPeripheralAction = action;
				
				if (bluetoothDeviceScript.DiscoveredDeviceList != null)
					bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
			}
			
			string serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;
			
			foreach (string serviceUUID in serviceUUIDs)
				serviceUUIDsString += serviceUUID + "|";
			
			// strip the last delimeter
			serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLERetrieveListOfPeripheralsWithServices (serviceUUIDsString);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothRetrieveListOfPeripheralsWithServices", serviceUUIDsString);
#endif
		}
	}

	public static void StopScan ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEStopScan ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothStopScan");
#endif
		}
	}

	public static void DisconnectAll ()
	{
		if (!Application.isEditor) {
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEDisconnectAll ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothDisconnectAll");
#endif
		}
	}

	public static void ConnectToPeripheral (string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.ConnectedPeripheralAction = connectAction;
				bluetoothDeviceScript.DiscoveredServiceAction = serviceAction;
				bluetoothDeviceScript.DiscoveredCharacteristicAction = characteristicAction;
				bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = disconnectAction;
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEConnectToPeripheral (name);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothConnectToPeripheral", name);
#endif
		}
	}
	
	public static void DisconnectPeripheral (string name, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.DisconnectedPeripheralAction = action;
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEDisconnectPeripheral (name);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothDisconnectPeripheral", name);
#endif
		}
	}

	public static void ReadCharacteristic (string name, string service, string characteristic, Action<string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();

#if UNITY_IPHONE || UNITY_TVOS
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEReadCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidReadCharacteristic", name, service, characteristic);
#endif
		}
	}
	
	public static void WriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.DidWriteCharacteristicAction = action;
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEWriteCharacteristic (name, service, characteristic, data, length, withResponse);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidWriteCharacteristic", name, service, characteristic, data, length, withResponse);
#endif
		}
	}
	
	public static void SubscribeCharacteristic (string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				name = name.ToUpper ();
				service = service.ToUpper ();
				characteristic = characteristic.ToUpper ();
				
#if UNITY_IPHONE || UNITY_TVOS
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [characteristic] = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [FullUUID (characteristic).ToLower ()] = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
		}
	}
	
	public static void SubscribeCharacteristicWithDeviceAddress (string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				name = name.ToUpper ();
				service = service.ToUpper ();
				characteristic = characteristic.ToUpper ();

#if UNITY_IPHONE || UNITY_TVOS
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][characteristic] = action;
#elif UNITY_ANDROID
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = notificationAction;
				
				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = action;
#endif
			}
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
		}
	}

	public static void UnSubscribeCharacteristic (string name, string service, string characteristic, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				name = name.ToUpper ();
				service = service.ToUpper ();
				characteristic = characteristic.ToUpper ();
				
#if UNITY_IPHONE || UNITY_TVOS
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = null;

				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = null;
#elif UNITY_ANDROID
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = null;
				
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][FullUUID (characteristic).ToLower ()] = null;
#endif
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEUnSubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidUnsubscribeCharacteristic", name, service, characteristic);
#endif
		}
	}

	public static void PeripheralName (string newName)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLEPeripheralName (newName);
#endif
		}
	}

	public static void CreateService (string uuid, bool primary, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.ServiceAddedAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLECreateService (uuid, primary);
#endif
		}
	}
	
	public static void RemoveService (string uuid)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLERemoveService (uuid);
#endif
		}
	}

	public static void RemoveServices ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLERemoveServices ();
#endif
		}
	}

	public static void CreateCharacteristic (string uuid, CBCharacteristicProperties properties, CBAttributePermissions permissions, byte[] data, int length, Action<string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.PeripheralReceivedWriteDataAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLECreateCharacteristic (uuid, (int)properties, (int)permissions, data, length);
#endif
		}
	}

	public static void RemoveCharacteristic (string uuid)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.PeripheralReceivedWriteDataAction = null;

#if UNITY_IPHONE
			_iOSBluetoothLERemoveCharacteristic (uuid);
#endif
		}
	}

	public static void RemoveCharacteristics ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLERemoveCharacteristics ();
#endif
		}
	}
	
	public static void StartAdvertising (Action action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.StartedAdvertisingAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLEStartAdvertising ();
#endif
		}
	}
	
	public static void StopAdvertising (Action action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.StoppedAdvertisingAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLEStopAdvertising ();
#endif
		}
	}
	
	public static void UpdateCharacteristicValue (string uuid, byte[] data, int length)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLEUpdateCharacteristicValue (uuid, data, length);
#endif
		}
	}
	
	public static string FullUUID (string uuid)
	{
		if (uuid.Length == 4)
			return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
		return uuid;
	}
}
