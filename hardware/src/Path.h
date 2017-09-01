enum StepType
{
  Start,
  Drive,
  Rotate,
  Wait,
  Stop
};

// a single action for the robot to execute
struct PathStep
{
  StepType type;
  float amount;
};
namespace Path
{
PathStep getStep();
void nextStep();
void load(const char input[]);
uint8_t getStepIndex();
}