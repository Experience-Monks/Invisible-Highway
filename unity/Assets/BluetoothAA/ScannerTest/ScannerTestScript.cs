using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScannerTestScript : MonoBehaviour
{
	public float Timeout = 1f;
	public GameObject ScannedItemPrefab;

	private float _timeout;
	private Dictionary<string, ScannedItemScript> _scannedItems;

	// Use this for initialization
	void Start ()
	{
		_scannedItems = new Dictionary<string, ScannedItemScript> ();

		BluetoothLEHardwareInterface.Initialize (true, false, () => {

			_timeout = Timeout;
		}, 
		(error) => {
			
			BluetoothLEHardwareInterface.Log ("Error: " + error);
		});
	}
	
	// Update is called once per frame
	void Update ()
	{
		_timeout -= Time.deltaTime;
		if (_timeout <= 0f)
		{
			_timeout = Timeout;

			BluetoothLEHardwareInterface.StopScan ();
			BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, null, (address, name, rssi, bytes) => {

				BluetoothLEHardwareInterface.Log ("item scanned: " + address);
				if (_scannedItems.ContainsKey (address))
				{
					var scannedItem = _scannedItems[address];
					scannedItem.TextRSSIValue.text = rssi.ToString ();
					BluetoothLEHardwareInterface.Log ("already in list " + rssi.ToString ());
				}
				else
				{
					BluetoothLEHardwareInterface.Log ("item new: " + address);
					var newItem = Instantiate (ScannedItemPrefab);
					if (newItem != null)
					{
						BluetoothLEHardwareInterface.Log ("item created: " + address);
						newItem.transform.parent = transform;

						var scannedItem = newItem.GetComponent<ScannedItemScript> ();
						if (scannedItem != null)
						{
							BluetoothLEHardwareInterface.Log ("item set: " + address);
							scannedItem.TextAddressValue.text = address;
							scannedItem.TextNameValue.text = name;
							scannedItem.TextRSSIValue.text = rssi.ToString ();

							_scannedItems[address] = scannedItem;
						}
					}
				}
			}, true);
		}
	}
}
