/*
 * Handles the robot path string that is sent from Unity RobotController.cs and BLEController.cs
 * https://www.adafruit.com/product/3235
 */

#include <Arduino.h>
#include "Path.h"
#include "util.h"

// length of this array should be defined as: (1 + maximum number of points the user can draw) * 2
// if it exceeds 255, currentStep type needs to be swapped for something bigger
PathStep *steps = new PathStep[40];
uint8_t currentStep = 0;

namespace Path
{
PathStep getStep()
{
  return steps[currentStep];
}

void nextStep()
{
  currentStep++;
}

uint8_t getStepIndex()
{
  return currentStep;
}

void load(const char path[])
{
  Serial.println(F("Loading Path: "));
  Serial.println(path);
  char *input = strdup(path);
  uint8_t stepIndex = 0;
  char *split;
  split = strtok(input, ":");
  while (split != NULL)
  {
    char command = split[0];
    float amount = String(split).substring(1).toFloat();
    switch (command)
    {
    case 'P': // start of a path
      steps[stepIndex].type = StepType::Start;
      Serial.println(F("| Start "));
      break;
    case 'F': // drive forwards x meters
      steps[stepIndex].type = StepType::Drive;
      steps[stepIndex].amount = amount;
      Serial.print(F("| Forwards "));
      Serial.print(amount);
      Serial.println(F("m"));
      break;
    case 'T': // turn x degrees
      steps[stepIndex].type = StepType::Rotate;
      steps[stepIndex].amount = amount;
      Serial.print(F("| Turn "));
      Serial.print(amount);
      Serial.println(F("°"));
      break;
    case 'W': // wait x seconds
      steps[stepIndex].type = StepType::Wait;
      steps[stepIndex].amount = amount;
      Serial.print(F("| Wait "));
      Serial.print(amount);
      Serial.println(F("s"));
      break;
    case 'S': // end of a path
      steps[stepIndex].type = StepType::Stop;
      Serial.println(F("| Stop"));
    default:
      break;
    }
    stepIndex++;
    split = strtok(NULL, ":");
  }

  // ensures robot does not read uninitialized memory as path points if not instructed to stop at end of path
  if (steps[stepIndex - 1].type != StepType::Stop)
  {
    steps[stepIndex].type = StepType::Stop;
  }
  currentStep = 0;
}
}