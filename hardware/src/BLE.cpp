/*
 * Handles the BLE connection between Feather Bluefruit LE and Unity
 * https://www.adafruit.com/product/3235
 */

#include <Adafruit_BLE.h>
#include <Adafruit_BluefruitLE_SPI.h>
#include "BluefruitConfig.h"

#include "Adafruit_BluefruitLE_UART.h"
#include "util.h"

Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

char incomingDataBuffer[MAX_DATA_LEN];
uint8_t incomingBufferIndex;
char outputBuffer[60];

namespace BLE
{
void init()
{
  Serial.println(F("Initialising the Bluefruit LE module:"));

  // do not proceed unless Bluefruit connected
  if (!ble.begin(VERBOSE_MODE))
  {
    error(F("Couldn't find Bluefruit, make sure it's in CoMmanD mode & check wiring?"));
  }
  Serial.println(F("OK!"));

  // perform factory reset to make sure everything is in a known state
  Serial.println(F("> Performing a factory reset: "));
  if (!ble.factoryReset())
  {
    error(F("Couldn't factory reset"));
  }

  // set broadcast device name here (i.e. HELP)
  // if you change the name here, change it in the Unity script BLEController.cs
  if (ble.sendCommandCheckOK(F("AT+GAPDEVNAME=HELP")))
  {
    Serial.println(F("> name changed"));
  }
  delay(250);

  // send ATZ (reset) to apply the name change
  if (ble.sendCommandCheckOK("ATZ"))
  {
    Serial.println(F("> resetting"));
  }
  delay(250);

  // ensure the name change was successful
  ble.sendCommandCheckOK("AT+GAPDEVNAME");

  // disable command echo from Bluefruit
  ble.echo(false);

  // print Bluefruit information
  Serial.println(F("> Requesting Bluefruit info:"));
  ble.info();

  Serial.println(F("> Waiting for a connection"));

  // debug info becomes flooded with messages after connection established
  ble.verbose(false);

  // wait for connection
  while (!ble.isConnected())
  {
    Serial.print(".");
    delay(500);
  }

  // data mode allows us to send and receive arbitrary ASCII text
  Serial.println(F("> Got a connection, switching to DATA mode!"));
  ble.setMode(BLUEFRUIT_MODE_DATA);

  Serial.println(F("*****************"));
}

void update(void (*processCommand)(const char[]))
{
  while (ble.available())
  {
    // read one ASCII char from Unity
    char incomingData = (char)ble.read();

    // append to our buffer
    incomingDataBuffer[incomingBufferIndex] = incomingData;
    incomingBufferIndex++;

    // the end of a command is marked with a newline - see Protocol.md
    if (incomingData == '\n')
    {
      processCommand(incomingDataBuffer);
      incomingBufferIndex = 0;
    }
    if (incomingBufferIndex == MAX_DATA_LEN)
    {
      // messages longer than max buffer size can't be fully read so they are discarded 
      Serial.println(F("buffer overflow, discarding bluetoot buffer!"));
      incomingBufferIndex = 0;
    }
  }
}

// sends a message over Bluetooth
void write(const char input[])
{
  ble.write(input);
}
}