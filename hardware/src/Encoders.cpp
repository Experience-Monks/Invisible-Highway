/*
 * Handles readings from the wheel encoders
 * https://solarbotics.com/product/gmww02/
 */

#include <Arduino.h>
#include "Config.h"

// variables must be marked volatile because theya re modified from an interrupt method
volatile long rightEncoderTicks;
volatile long leftEncoderTicks;

// interrupt method run whenever left encoder clock pin changes state
void leftEncoderChanged()
{
  leftEncoderTicks -= digitalRead(L_ENCODER_DIRECTION_PIN) * 2 - 1;
}

// interrupt method run whenever right encoder clock pin changes state
void rightEncoderChanged()
{
  rightEncoderTicks -= digitalRead(R_ENCODER_DIRECTION_PIN) * 2 - 1;
}

// arc length distance travelled in meters based on wheel circumference and encoder ticks
float ticksToMeters(int ticks)
{
  // constants determined experimentally
  const float scale = 1.0960f;
  return scale * TWO_PI * WHEEL_RADIUS * ((float)ticks / (float)TICKS_PER_REVOLUTION);
}

namespace Encoders
{
void init()
{
  // all encoder pins need to be set up as input to read them
  pinMode(L_ENCODER_DIRECTION_PIN, INPUT);
  pinMode(L_ENCODER_CLOCK_PIN, INPUT);
  pinMode(R_ENCODER_DIRECTION_PIN, INPUT);
  pinMode(R_ENCODER_CLOCK_PIN, INPUT);

  // attaching interrupts causes a method to be run every time pin state changes
  // used to keep up with changes in encoder clock state to accurately record distances
  attachInterrupt(digitalPinToInterrupt(L_ENCODER_CLOCK_PIN), leftEncoderChanged, CHANGE);
  attachInterrupt(digitalPinToInterrupt(R_ENCODER_CLOCK_PIN), rightEncoderChanged, CHANGE);
}

float getLeftDistance()
{
  return ticksToMeters(leftEncoderTicks);
}

float getRightDistance()
{
  return ticksToMeters(rightEncoderTicks);
}

// call this before measuring a relative distance instead of total distance wheel has ever travelled
void reset()
{
  leftEncoderTicks = 0;
  rightEncoderTicks = 0;
}
}