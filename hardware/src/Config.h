#ifndef CONFIG_H_
  #define CONFIG_H_

  // motor shield communicates over I2C
  // "ID" values are "ports" on motor shield board (not GPIO pins)
  #define L_MOTOR_ID 3
  #define R_MOTOR_ID 4

  #define R_ENCODER_DIRECTION_PIN 22
  #define R_ENCODER_CLOCK_PIN 0
  
  #define L_ENCODER_DIRECTION_PIN 23
  #define L_ENCODER_CLOCK_PIN 1

  #define TICKS_PER_REVOLUTION 109
  #define WHEEL_RADIUS 0.029675 // in meters

  #define GYRO_GAIN 3

  #define MAX_SPEED 150
  #define RAMP_DISTANCE 0.3f // in meters
#endif