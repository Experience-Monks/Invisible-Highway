# Robot BLE UART Protocol
## Device to Robot

To assign a path and start executing it, send:
`P:<any number of steps>:S\n`

A step can be any of:

- `P`: a noop, marks the beginning of a path.
- `F<meters>`: move forwards x meters
- `T<degrees>`: turn clockwise x degrees
- `W<seconds>`: wait x seconds
- `S`: halt all movement and stop executing the path

A sample of a complete path: `P:F1.5:T90:F3.2:T-45:S\n`

- Move forwards 1.5 meters
- Turn 90 degrees clockwise
- Move forwards 3.2 meters
- Turn 45 degrees counter-clockwise
- Stop

To load an empty path (and make the robot stop), send:

`x\n`

## Robot to Device

`P:<step index>:<progress>:<rotation>\n`

For example: if the robot is executing the path `P:F1.5:T90:F3.2:T-45\n`, then a packet from the robot containing `P:1:0.48:45\n` signifies that it's on step 1 (`F1.5` or "move forward 1.5m") it's driven 0.48 meters, and its absolute rotation is 45°.