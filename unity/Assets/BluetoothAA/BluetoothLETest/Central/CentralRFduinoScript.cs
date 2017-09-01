using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// see the RFduino script at the bottom of this file that needs to be uplaoded
// to the RFduino and will function with this side of things

public class CentralRFduinoScript : MonoBehaviour
{
	public Transform PanelCentral;
	public Text Name;
	public Text Address;
	public Text TextConnectButton;
	public GameObject RFduino;
	public GameObject Button1Highlight;
	public GameObject LEDHighlight;
	public GameObject LEDButton;
	
	private bool _connecting = false;
	private string _connectedID = null;
	private string _serviceUUID = "2220";
	private string _readCharacteristicUUID = "2221";
	private string _writeCharacteristicUUID = "2222";

	bool _connected = false;
	bool Connected
	{
		get { return _connected; }
		set
		{
			_connected = value;
			
			if (_connected)
			{
				TextConnectButton.text = "Disconnect";
				_connecting = false;
			}
			else
			{
				TextConnectButton.text = "Connect";
				_connectedID = null;
				RFduino.SetActive (false);
				Button1Highlight.SetActive (false);
				LEDHighlight.SetActive (false);
				LEDButton.SetActive (false);
				ledON = false;
			}
		}
	}

	public void Initialize (CentralPeripheralButtonScript centralPeripheralButtonScript)
	{
		Connected = false;
		Name.text = centralPeripheralButtonScript.TextName.text;
		Address.text = centralPeripheralButtonScript.TextAddress.text;
	}

	void disconnect (Action<string> action)
	{
		BluetoothLEHardwareInterface.DisconnectPeripheral (Address.text, action);
	}

	public void OnBack ()
	{
		if (Connected)
		{
			disconnect ((Address) => {

				Connected = false;
				BLETestScript.Show (PanelCentral.transform);
			});
		}
		else
			BLETestScript.Show (PanelCentral.transform);
	}
	
	public void OnConnect ()
	{
		if (!_connecting)
		{
			if (Connected)
			{
				disconnect ((Address) => {
					Connected = false;
				});
			}
			else
			{
				BluetoothLEHardwareInterface.ConnectToPeripheral (Address.text, (address) => {
				},
				(address, serviceUUID) => {
				},
				(address, serviceUUID, characteristicUUID) => {

					// discovered characteristic
					if (IsEqual(serviceUUID, _serviceUUID))
					{
						_connectedID = address;

						Connected = true;
						RFduino.SetActive (true);

						if (IsEqual(characteristicUUID, _readCharacteristicUUID))
						{
							BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (_connectedID, _serviceUUID, _readCharacteristicUUID, (deviceAddress, notification) => {
								
							}, (deviceAddress2, characteristic, data) => {

								if (deviceAddress2.CompareTo (_connectedID) == 0)
								{
									if (IsEqual(characteristicUUID, _readCharacteristicUUID))
									{
										if (data.Length == 0)
										{
											Button1Highlight.SetActive (false);
										}
										else
										{
											if (data[0] == 0x01)
												Button1Highlight.SetActive (true);
											else
												Button1Highlight.SetActive (false);
										}
									}
								}
								
							});
						}
						else if (IsEqual(characteristicUUID, _writeCharacteristicUUID))
						{
							LEDButton.SetActive (true);
						}
					}
				}, (address) => {

					// this will get called when the device disconnects
					// be aware that this will also get called when the disconnect
					// is called above. both methods get call for the same action
					// this is for backwards compatibility
					Connected = false;
				});

				_connecting = true;
			}
		}
	}

	private bool ledON = false;
	public void OnLED ()
	{
		ledON = !ledON;
		if (ledON)
		{
			SendByte ((byte)0x01);
			LEDHighlight.SetActive (true);
		}
		else
		{
			SendByte ((byte)0x00);
			LEDHighlight.SetActive (false);
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

	string FullUUID (string uuid)
	{
		return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
	}

	bool IsEqual(string uuid1, string uuid2)
	{
		if (uuid1.Length == 4)
			uuid1 = FullUUID (uuid1);
		if (uuid2.Length == 4)
			uuid2 = FullUUID (uuid2);

		return (uuid1.ToUpper().CompareTo(uuid2.ToUpper()) == 0);
	}

	void SendByte (byte value)
	{
		byte[] data = new byte[] { value };
		BluetoothLEHardwareInterface.WriteCharacteristic (_connectedID, _serviceUUID, _writeCharacteristicUUID, data, data.Length, true, (characteristicUUID) => {

			BluetoothLEHardwareInterface.Log ("Write Succeeded");
		});
	}
	
	void SendBytes (byte[] data)
	{
		BluetoothLEHardwareInterface.WriteCharacteristic (_connectedID, _serviceUUID, _writeCharacteristicUUID, data, data.Length, false, null);
	}
}

/*****************************
 * RFduino .ino code. Create a file called LEDButton.ino and paste this code into it

/*
This RFduino sketch demonstrates a full bi-directional Bluetooth Low
Energy 4 connection between an iPhone application and an RFduino.

This sketch works with the rfduinoLedButton iPhone application.

The button on the iPhone can be used to turn the green led on or off.
The button state of button 1 is transmitted to the iPhone and shown in
the application.
*/

/*
 Copyright (c) 2014 OpenSourceRF.com.  All right reserved.

 This library is free software; you can redistribute it and/or
 modify it under the terms of the GNU Lesser General Public
 License as published by the Free Software Foundation; either
 version 2.1 of the License, or (at your option) any later version.

 This library is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 See the GNU Lesser General Public License for more details.

 You should have received a copy of the GNU Lesser General Public
 License along with this library; if not, write to the Free Software
 Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
/*
#include <RFduinoBLE.h>

// pin 3 on the RGB shield is the red led
// (can be turned on/off from the iPhone app)
int led = 3;

// pin 5 on the RGB shield is button 1
// (button press will be shown on the iPhone app)
int button = 5;

// debounce time (in ms)
int debounce_time = 10;

// maximum debounce timeout (in ms)
int debounce_timeout = 100;

void setup() {
	// led turned on/off from the iPhone app
	pinMode(led, OUTPUT);
	
	// button press will be shown on the iPhone app)
	pinMode(button, INPUT);
	
	// this is the data we want to appear in the advertisement
	// (if the deviceName and advertisementData are too long to fix into the 31 byte
	// ble advertisement packet, then the advertisementData is truncated first down to
	// a single byte, then it will truncate the deviceName)
	RFduinoBLE.advertisementData = "ledbtn";
	
	// start the BLE stack
	RFduinoBLE.begin();
}

int debounce(int state)
{
	int start = millis();
	int debounce_start = start;
	
	while (millis() - start < debounce_timeout)
		if (digitalRead(button) == state)
	{
		if (millis() - debounce_start >= debounce_time)
			return 1;
	}
	else
		debounce_start = millis();
	
	return 0;
}

int delay_until_button(int state)
{
	// set button edge to wake up on
	if (state)
		RFduino_pinWake(button, HIGH);
	else
		RFduino_pinWake(button, LOW);
	
	do
		// switch to lower power mode until a button edge wakes us up
		RFduino_ULPDelay(INFINITE);
	while (! debounce(state));
	
	// if multiple buttons were configured, this is how you would determine what woke you up
	if (RFduino_pinWoke(button))
	{
		// execute code here
		RFduino_resetPinWake(button);
	}
}

void loop() {
	delay_until_button(HIGH);
	RFduinoBLE.send(1);
	
	delay_until_button(LOW);
	RFduinoBLE.send(0);
}

void RFduinoBLE_onDisconnect()
{
	// don't leave the led on if they disconnect
	digitalWrite(led, LOW);
}

void RFduinoBLE_onReceive(char *data, int len)
{
	// if the first byte is 0x01 / on / true
	if (data[0])
		digitalWrite(led, HIGH);
	else
		digitalWrite(led, LOW);
}

***********************************/