# Invisible Highway

![image](https://github.com/Jam3/Invisible-Highway/raw/master/media/Invisible_Highway.gif)

Remember how fun remote control cars were to play with as a child? Well, what if you could control where the car goes by drawing invisible highways in your living room? What if you could watch as an imaginative and fun scene is dynamically generated around your car as it drives through the roads you’re creating.

This playful experiment examines just how AR can be used to intuitively control physical objects.

Originally prototyped by Anna Fusté and Judith Amores from the MIT Media Lab, the two researchers who worked with Google Creative Lab over the summer to experiment with AR and developing new and meaningful interaction models with our environment. Google then brought on global design and experience studio [Jam3](http://jam3.com), to design and build out the concept, and add dynamic functionality that would seamlessly meld the physical and digital worlds.

Google Blocks was used for initial modelling. The car was built using an off the shelf model from Adafruit with DIY additions. The app is built in Unity and communicates with the robot via Bluetooth.

You can read about this and other AR Experiments [here](https://experiments.withgoogle.com/ar/invisible-highway).

# Hardware

## Bill of Materials

To create the robot you will need the following:

* 1x [robot](https://www.adafruit.com/product/3235)
* 1x [IMU](https://www.adafruit.com/product/2472)
* 2x [encoders](https://solarbotics.com/product/gmww02/) for the motors
* A [cover](https://github.com/Jam3/Invisible-Highway/raw/master/media/robot_cover.pdf) for the robot. Print it on A4 without scaling to fit, cut and assemble it so that it looks like [this](https://github.com/Jam3/Invisible-Highway/raw/master/media/robot_cover_result.jpg) and paint with a color which is not found in the environment you will be using the robot, so that the Chroma Key shader can pick it up

## Setting up the Robot

* Download VS Code
* Install the PlatformIO extension
* Open the "Robot-Firmware" folder in VS Code
* Connect the robot via USB
* Press the upload button in the bottom left of VS Code

PlatformIO should automatically install the dependencies. if you get build errors due to missing libraries:

* Open the file platformio.ini
* Click the terminal icon on the bottom bar of vs code
* For each number in the lib_deps (?) section of the ini, run in the terminal: `pio install #` where # is the number from lib_deps

Using the Adafruit Bluefruit LE Connect app, connect to the robot and issue a few commands to make sure everything is working as expected.


# Software

## Installation

To install the app on an ARCore-compatible device do the following:

* Install all the ARCore pre-requisites on the device (for instance the ARCore service)
* Pull this repository and using a version of Unity 2017.1 beta9 or later open the `InvisibleHighway.unity` scene
* Ensure all the project settings conform to [ARCore requirements](https://developers.google.com/ar/develop/unity/getting-started).
* Build and Run for your device.

## Usage

* Make sure your Robot is turned on.
* Launch the app, preferably in landscape mode.
* Observe the `blue LED` on the Robot turn on to indicate that the bluetooth connection with the app is established.
* Keep the app pointing to the floor and move it slightly around so that it can recognize the floor as the main ground plane.
* Once the floor is recognized a white reticule will appear on the sceen. Tap anywheer on the screen to add a point to the path that the Robot will follow.
* Keep tapping to add more points and create a full path. Don't create sharp corners as the road assets were not made for that. When done, press the `next` button.
* The wireframe of the Robot's cover has appeared (if you are using a different cover than the one provided you will need to update this mesh in Unity).
* Align perfectly the wireframe with the physical Robot so that the app can understand your Robot's position and orientation and tap on the screen.
* If you are happy with the calibration, tap the `done` button to start the experience!

## Known Issues

### Android Permissions

The first time you run the app you will be asked for the required permissions (camera and bluetooth). After you accept you might see an error that ARCore failed to connect. Just restart the app to resolve this issue.

We believe this will be resolved in future versions of ARCore as hinted in [the documentation](https://developers.google.com/ar/reference/unity/class/google-a-r-core/android-permissions-manager#class_google_a_r_core_1_1_android_permissions_manager_1a2af3849a9675133c702d990f6717833f).

## Additional Credits

We acknowledge and are grateful to these developers for their contributions to open source software. You can find the source code, project links, and license information below.

* [Bluefruit LE Feather Robot Rover](https://learn.adafruit.com/bluefruit-feather-robot/code) by James DeVito
* [ShadowDrawer](https://github.com/keijiro/ShadowDrawer) by Keijiro Takahashi