using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CentralScript : MonoBehaviour
{
	public Transform PanelTypeSelection;
	public Transform PanelScrollContents;
	public CentralRFduinoScript PanelCentralRFduino;
	public CentralTISensorTagScript PanelCentralTISensorTag;
	public CentralNordicScript PanelCentralNordic;
	public GameObject CentralPeripheralButtonPrefab;
	public Text TextScanButton;

	private Dictionary<string, CentralPeripheralButtonScript> _peripheralList;
	private bool _scanning = false;

	public void Initialize ()
	{
		BluetoothLEHardwareInterface.Initialize (true, false, () => {
			
		}, (error) => {
		});
	}

	public void OnBack ()
	{
		RemovePeripherals ();

		if (_scanning)
			OnScan (); // this will stop scanning

		BluetoothLEHardwareInterface.DeInitialize (() => {
			BLETestScript.Show (PanelTypeSelection);
		});
	}

	protected string BytesToString (byte[] bytes)
	{
		string result = "";

		foreach (var b in bytes)
			result += b.ToString ("X2");

		return result;
	}

	public void OnScan ()
	{
		if (_scanning)
		{
			BluetoothLEHardwareInterface.StopScan ();
			TextScanButton.text = "Start Scan";
			_scanning = false;
		}
		else
		{
			RemovePeripherals ();

			// the first callback will only get called the first time this device is seen
			// this is because it gets added to a list in the BluetoothDeviceScript
			// after that only the second callback will get called and only if there is
			// advertising data available
			BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, name) => {

				AddPeripheral (name, address);

			}, (address, name, rssi, advertisingInfo) => {

				if (advertisingInfo != null)
					BluetoothLEHardwareInterface.Log (string.Format ("Device: {0} RSSI: {1} Data Length: {2} Bytes: {3}", name, rssi, advertisingInfo.Length, BytesToString (advertisingInfo)));
			});

			TextScanButton.text = "Stop Scan";
			_scanning = true;
		}
	}

	void RemovePeripherals ()
	{
		for (int i = 0; i < PanelScrollContents.childCount; ++i)
		{
			GameObject gameObject = PanelScrollContents.GetChild (i).gameObject;
			Destroy (gameObject);
		}
		
		if (_peripheralList != null)
			_peripheralList.Clear ();
	}

	void AddPeripheral (string name, string address)
	{
		if (_peripheralList == null)
			_peripheralList = new Dictionary<string, CentralPeripheralButtonScript> ();

		if (!_peripheralList.ContainsKey (address))
		{
			GameObject peripheralObject = (GameObject)Instantiate (CentralPeripheralButtonPrefab);
			CentralPeripheralButtonScript script = peripheralObject.GetComponent<CentralPeripheralButtonScript> ();
			script.TextName.text = name;
			script.TextAddress.text = address;
			script.PanelCentralRFduino = PanelCentralRFduino;
			script.PanelCentralTISensorTag = PanelCentralTISensorTag;
			script.PanelCentralNordic = PanelCentralNordic;
			peripheralObject.transform.SetParent (PanelScrollContents);
			peripheralObject.transform.localScale = new Vector3 (1f, 1f, 1f);

			_peripheralList[address] = script;
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
