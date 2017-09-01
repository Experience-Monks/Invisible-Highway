/*
 * Handles readings from Adafruit IMU
 * https://www.adafruit.com/product/2472
 */

#include <Adafruit_BNO055.h>
#include "Gyro.h"
#include "util.h"

float relativeZero = 0;
float measuredAngle = 0;
Adafruit_BNO055 bno = Adafruit_BNO055(55);

namespace Gyro
{
void init()
{
  // initialize sensor
  if (!bno.begin())
  {
    error(F("No BNO055 detected, check your wiring / I2C address!"));
  }
  // give sensor a few seconds to start
  delay(1000);
}

void update()
{
  // get fresh data from IMU
  sensors_event_t event;
  bno.getEvent(&event);
  // store our fresh data so can access it later without blocking on an I2C read
  measuredAngle = (float)event.orientation.x;
}

float getAbsoluteAngle()
{
  return measuredAngle;
}

// ranges from -180deg -> 180deg clockwise
float getRelativeAngle()
{
  float angle = getAbsoluteAngle() - relativeZero;
  return clampAngle(angle);
}
void zero()
{
  relativeZero = getAbsoluteAngle();
}
}