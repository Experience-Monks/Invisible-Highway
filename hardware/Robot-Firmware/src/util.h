#ifndef HIGHWAY_UTIL_H_
#define HIGHWAY_UTIL_H_

// outputs an error message, then halts the program
// we wait for Serial to be connected, so the robot can halt and then be plugged in to view the error.
#define error(err)     \
  while (!Serial)      \
    ;                  \
  Serial.println(err); \
  while (1)            \
    ;

#define sign(x) x >= 0 ? 1 : -1
#define clamp(low, high, val) max(low, min(high, val))

// marked as inline so we can embed it in this header file
inline float clampAngle(float angle)
{
  while (angle > 180)
  {
    angle -= 360;
  }
  while (angle < -180)
  {
    angle += 360;
  }
  return angle;
}
#endif