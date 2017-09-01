/*
 * This is based on one of the examples for the nRF51822 based Bluefruit LE modules https://learn.adafruit.com/adabox002/code-for-your-robot
 * Modified to drive a 3-wheeled BLE Robot Rover by http://james.devi.to
 * Modified by Judith Amores & Anna Fuste to drive for Unity & Android for UART communication (based on bleuart_datamode)
 * Modified by Ari Lotter to drive specified paths with encoders and a gyroscope
 * MIT license, check LICENSE for more information
 * All text above, and the splash screen below must be included in any redistribution
 */

// Modified by Jam3 - http://www.jam3.com/

#include <Arduino.h>
#include <Adafruit_MotorShield.h>
#include "Config.h"
#include "BLE.h"
#include "Path.h"
#include "Encoders.h"
#include "Gyro.h"
#include "util.h"

// by not passing the constructor a number, we're using the default I2C address
Adafruit_MotorShield MotorShield = Adafruit_MotorShield();

Adafruit_DCMotor *L_MOTOR = MotorShield.getMotor(L_MOTOR_ID);
Adafruit_DCMotor *R_MOTOR = MotorShield.getMotor(R_MOTOR_ID);

// current position of the robot on a path step which is reported over Bluetooth periodically
double currentStepProgress = 0;

// allows us to reference the function before its body is defined
void processCommand(const char[]);

// incremented periodically and when it reaches a value we send updates over Bluetooth
uint8_t bluetoothUpdateCounter = 0;

unsigned long waitStartTime;

//called once when the microcontroller is initialized
void setup()
{
  // 115200 is one of the faster baud rates, so we spend less time sending messages over Serial
  Serial.begin(115200);

  // print a nice startup message!
  Serial.println(F("Invisible Highway Robot Initializing (♥_♥)"));
  Serial.println(F("------------------------------------------"));

  // initialize the motor shield with the default frequency of 1.6KHz
  MotorShield.begin();

  // initialize all our sensors
  Encoders::init();
  Gyro::init();

  // load a path that will immediately stop motor movement and wait for commands
  Path::load("P");

  // initializing the BLE module is done last, because execution pauses here until the phone connects
  BLE::init();
}

// reports data about the robot's current status over Bluetooth
// protocol is documented in `Protocol.md
void updateInfo()
{
  BLE::write("P:");
  BLE::write(String(Path::getStepIndex()).c_str());
  BLE::write(":");
  BLE::write(String(currentStepProgress).c_str());
  BLE::write(":");
  BLE::write(String(Gyro::getAbsoluteAngle()).c_str());
  BLE::write("\n");
}

bool driveTo(float targetDistance)
{
  double leftDistance = Encoders::getLeftDistance();
  double rightDistance = Encoders::getRightDistance();
  double currentDistance = max(leftDistance, rightDistance);

  // this will be reported via bluetooth later
  currentStepProgress = currentDistance;

  // trapezoidal velocity path is used to smoothly accelerate robot to speed
  // ramps up over rampDitance until MAX_SPEED and ramps down over rampDistance again
  // ramping function can be visualized here: https://www.desmos.com/calculator/vu3uug0q2n
  float rampDistance = min(RAMP_DISTANCE, targetDistance / 2.0f);
  float error = abs(targetDistance - currentDistance);
  float speed;
  float gyroGain = GYRO_GAIN;
  if (currentDistance < rampDistance)
  {
    speed = (MAX_SPEED / 2.0f) + (currentDistance * MAX_SPEED) / (2.0f * rampDistance);
    gyroGain *= 1.2f;
  }
  else if (error < rampDistance)
  {
    speed = (MAX_SPEED / 4.0f) + (3.0f * error * MAX_SPEED) / (4.0f * rampDistance);
    gyroGain *= 1.5f;
  }
  else
  {
    speed = MAX_SPEED;
  }

  // adjust power to each motor based on gyro readings to correct the robot straying from its heading
  float gyroAngle = Gyro::getRelativeAngle();
  float gyroSpeedOffset = gyroAngle * gyroGain;

  // motor values are clamped between 0 and 255 to prevent the speed value from over/underflowing
  L_MOTOR->setSpeed(clamp(0, 255, speed - gyroSpeedOffset));
  R_MOTOR->setSpeed(clamp(0, 255, speed + gyroSpeedOffset));
  L_MOTOR->run(FORWARD);
  R_MOTOR->run(FORWARD);

  return currentDistance >= targetDistance;
}

bool rotateTo(float angle)
{
  // all angles might not be in the same range, so they're normalized to -180deg ->180deg
  float clampedAngle = clampAngle(angle);
  float gyroAngle = Gyro::getRelativeAngle();
  float error = clampedAngle - gyroAngle;
  float errorMagnitude = abs(error);

  // this will be reported via bluetooth later
  currentStepProgress = gyroAngle;

  // rotating at a lower speed near our setpoint allows us to more accurately stop on it
  // equation chosen by fair dice roll
  // https://www.desmos.com/calculator/vom9jgp0mu
  float speed = 5.917f * errorMagnitude - 0.3f * powf(errorMagnitude, 2) + 0.006f * powf(errorMagnitude, 3);
  speed = min(speed, 70);
  L_MOTOR->setSpeed(speed);
  R_MOTOR->setSpeed(speed);

  bool isTurningRight = error > 0;
  L_MOTOR->run(isTurningRight ? FORWARD : BACKWARD);
  R_MOTOR->run(isTurningRight ? BACKWARD : FORWARD);

  return errorMagnitude <= 0.25f;
}

void periodicUpdate()
{
  bluetoothUpdateCounter++;
  if (bluetoothUpdateCounter % 10 == 0)
  {
    bluetoothUpdateCounter = 0;
    updateInfo();
  }
}

void stopMotors()
{
  L_MOTOR->setSpeed(0);
  L_MOTOR->run(RELEASE);
  R_MOTOR->setSpeed(0);
  R_MOTOR->run(RELEASE);
}

void loop()
{
  Gyro::update();
  bool done = false;
  PathStep currentStep = Path::getStep();

  switch (currentStep.type)
  {
  case StepType::Start:
    done = true;
    break;
  case StepType::Drive:
    done = driveTo(currentStep.amount);
    periodicUpdate();
    break;
  case StepType::Rotate:
    done = rotateTo(currentStep.amount);
    periodicUpdate();
    break;
  case StepType::Wait:
    done = millis() - waitStartTime > currentStep.amount * 1000;
    currentStepProgress = (millis() - waitStartTime);
    periodicUpdate();
    break;
  case StepType::Stop:
  default:
    stopMotors();
    BLE::update(processCommand);
    break;
  }
  if (done)
  {
    stopMotors();
    updateInfo();
    currentStepProgress = 0;
    waitStartTime = millis();
    Encoders::reset();
    Gyro::zero();
    Path::nextStep();
  }
}

void processCommand(const char command[])
{
  switch (command[0])
  {
  case 'P':
    Path::load(command);
    break;
  case 'x':
    Path::load("P");
    break;
  default:
    Serial.print("Invalid command: ");
    Serial.println(command);
    break;
  }
}
