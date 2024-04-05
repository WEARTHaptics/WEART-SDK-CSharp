# Low-Level C# SDK

Welcome to the Weart Low-Level C# SDK documentation.

The SDK allows to connect to the Weart middleware and perform various actions with the TouchDIVER devices:
* Start and Stop the middleware operations
* Calibrate the device
* Receive tracking data from the devices
* Receive raw data from the thimble's motion sensors 
* Receive analog raw data from the thimble's senosrs
* Send haptic effects to the devices

# Architecture
<img src="./architecture.svg" width="100%" alt="C++ SDK Architecture" />

## Links
* Github source code repository, go [here](https://github.com/WEARTHaptics/WEART-SDK-CSharp/)
* an example source code application is available [here](https://github.com/WEARTHaptics/WEART-SDK-CSharp-Example/).

# Setup
The minimum setup to use the weart SDK consists of:
* A PC with the Middleware installed
* A TouchDIVER device
* A C# project using the Low-Level SDK 

The SDK can be downloaded as a zip file containing all the necessary files.
To use it in your C# project, unzip it and move the files in a folder inside your project.
Then, add the sdk files in your project.
In Visual Studio, this can be done by including the sdk directory into the project (Right Click on Solution -> Add -> Existing Directory).

To start using the SDK in your project, start the Middleware application and connect a TouchDIVER device to it.

Import the main namespaces:

~~~~~~~~~~~~~{.cs}
using WeArt.Core;
using WeArt.Components;
using WeArt.Messages;
~~~~~~~~~~~~~

Create the WeArtClient and start the communication with the middleware:
~~~~~~~~~~~~~{.cs}
	// Create weart client
	WeArtClient weArtClient = new WeArtClient { IpAddress = WeArtNetwork.LocalIPAddress, Port = 13031 };

	// Start connection and send start message to middleware
	weArtClient.Start();
~~~~~~~~~~~~~

\note The call to WeArtClient.Start() will also start the middleware, so be sure to have connected the devices before

# Features

## Start/Stop Client
As seen in the previous section, to start the middleware operations, call the Start() method.

~~~~~~~~~~~~~{.cs}
	weArtClient->Start();
~~~~~~~~~~~~~

To stop the middleware, call the Stop() method.

~~~~~~~~~~~~~{.cs}
	weArtClient->Stop();
~~~~~~~~~~~~~

## Devices calibration
After starting the communication with the middleware, it's now possible to calibrate the TouchDIVER devices.
The calibration allows to set the initial offsets of each thimble relative to the control unit position, in order to improve the finger tracking output.

The SDK client allows to add callbacks to monitor the calibration procedure status and result.
For example, to start the calibration and print "Success" or "Failed":

~~~~~~~~~~~~~{.cs}
	weartClient.OnCalibrationStart += Console.WriteLine("Calibration start!");
	weArtClient.OnCalibrationResultSuccess += (HandSide hand) => Console.WriteLine("Success!");
	weArtClient.OnCalibrationResultFail += (HandSide hand) => Console.WriteLine("Failed");
	weartClient.OnCalibrationFinish += (HandSide hand) => Console.WriteLine("Calibration finish!");

	// Start calibraiton on demand
	weArtClient.StartCalibration();
~~~~~~~~~~~~~

## Haptic feedback

The TouchDIVER allows to perform haptic feedback on the user's finger through its *thimbles*.
Every thimble can apply a certain amount of pressure, temperature and vibration based on the processed object and texture.

### Haptic Object

A WeArtHapticObject is the basic object used to apply haptic feedback.
To create one, use the following code:

~~~~~~~~~~~~~{.cs}
	// create haptic object to manage actuation on Right hand and Index Thimble
	WeArtHapticObject hapticObject = new WeArtHapticObject(_weartClient);
	hapticObject.HandSides = HandSideFlags.Right;
	hapticObject.ActuationPoints = ActuationPointFlags.Index;
~~~~~~~~~~~~~

The attirbutes handSideFlag and actuationPointFlag accept multiple values.
The next example presents a single haptic object that, when applied a WeArtEffect, will affect both hands and all fingers.

~~~~~~~~~~~~~{.cs}
	hapticObject.HandSides = HandSideFlags.Right | HandSideFlags.Left;
    hapticObject.ActuationPoints = ActuationPointFlags.Index | ActuationPointFlags.Middle | ActuationPointFlags.Thumb;
~~~~~~~~~~~~~

### Create Effect

The SDK contains a basic TouchEffect class to apply effects to the haptic device.
The TouchEffect class contains the effects without any processing.
For different use cases (e.g. values not directly set, but computed from other parameters), create a different effect class by implementing the IWeArtEffect interface.

Create the object on which the temperature, force and texture values will be applied:

~~~~~~~~~~~~~{.cs}
TouchEffect touchEffect = new TouchEffect();
~~~~~~~~~~~~~

### Add or Update Effect

It's possible to add a new effect to an haptic object, or to update an existing one.

In the example below, the effect created in the previous section is updated with a new temperature, force and texture.
It is then added to the haptic object if not already present (if the effect is already applied to the thimble, it will update it automatically through the OnUpdate event).

~~~~~~~~~~~~~{.cs}
    // create temperature component
	Temperature temperature = Temperature.Default;
	temperature.Active = true; // must be active to work
	temperature.Value = 0.2f;

	// create force component
	Force force = Force.Default;
	force.Active = true;
	force.Value = 0.7f;

	// create texture component
	Texture texture = Texture.Default;
	texture.Active = true;
	texture.TextureType = TextureType.ProfiledAluminiumMeshFast;

	// effect set proporties 
	touchEffect.Set(temperature, force, texture);

	// add effect if needed, to thimble 
	if (hapticObject.ActiveEffect == null)
		hapticObject.AddEffect(touchEffect);
~~~~~~~~~~~~~

@note When a new effect is added, the last effect applied is replaced by the new effect.

### Remove Effect

If an effect is not needed anymore, it can be removed from the haptic object with the RemoveEffect method.

~~~~~~~~~~~~~{.cs}
	hapticObject.RemoveEffect(touchEffect);
~~~~~~~~~~~~~

## Tracking

After starting the middleware and performing the device calibration, it's possible to receive tracking data
related to the TouchDIVER thimbles.

To read these values, create and set a thimble tracker object for monitoring the closure/abduction value of a given finger:
~~~~~~~~~~~~~{.cs}
	WeArtThimbleTrackingObject thumbThimbleTracking = new WeArtThimbleTrackingObject(weArtClient, HandSide.Right, ActuationPoint.Index);
~~~~~~~~~~~~~

Once this object is created, it will start receiving the tracking values.
To access the closure and abduction values, simply use the getters provided by the thimble tracking object.

The closure value ranges from 0 (opened) to 1 (closed).

The abduction value ranges from 0 (finger near the hand's central axis) to 1 (finger far from the hand central axis).

~~~~~~~~~~~~~{.cs}
	float closure = thumbThimbleTracking.Closure.Value;
    float abduction = thumbThimbleTracking.Abduction.Value;
~~~~~~~~~~~~~

@note The **closure** value is available for all thimbles, while the **abduction** value is available only for the thumb (other thimbles will have a value of 0).

## Tracking Raw Data

It's possible to receive the raw data from the sensors on each thimble (and the control unit), in addition to the tracking data.
Each sensor has:
* 3-axis accelerometer
* 3-axis gyroscope
* Time of Flight sensor

To read these values, create a WeArtTrackingRawDataObject and add it to the client.
~~~~~~~~~~~~~{.cs}
	WeArtTrackingRawDataObject rawSensorData = new WeArtTrackingRawDataObject(weArtClient, HandSide.Right, ActuationPoint.Index);
~~~~~~~~~~~~~

Once this object is added to the client, it will listen for raw data messages.
To start receiving raw data from the middleware, call the WeArtClient.StartRawData() method.
To stop receiving raw data, call the WeArtClient.StopRawData() method.

To get the sensors data, get the latest sample (SensorData) from the WeArtRawSensorsDataTrackingObject object.
The sample contains the accelerometer, gyroscope and time of flight data, in addition to the timestamp of its sampling (generated by the middleware and represented as a DateTime value).
~~~~~~~~~~~~~{.cs}
	TrackingRawData sample = rawSensorData.LastSample;
~~~~~~~~~~~~~

@note The Palm (control unit) doesn't contain a Time-Of-Flight sensor, so its value is always set to 0.

In addition to getting the latest sample by polling the tracking object, it's possible to add a callback called whenever a new sensor data sample is
received from the TouchDIVER.

~~~~~~~~~~~~~{.cs}
	rawSensorData.DataReceived += (TrackingRawData data) => {
		// process the sensor data sample
	};
~~~~~~~~~~~~~

## Analog Raw Sensors Data

It's possible to receive the raw data from the sensors on each thimble (and the control unit), instead of the tracking data when this function is activated on the Middleware.
Each sensor has:
* NTC - Negative Temperature Coefficient (raw data and converted degree)
* FSR - force sensing resistor (raw adata and converted newton)

To read these values, create a WeArtAnalogSensorRawDataObject object and add it to the client.
~~~~~~~~~~~~~{.cs}
WeArtAnalogSensorRawDataObject anlogSensorData = new WeArtAnalogSensorRawDataObject(_weartClient, HandSide, ActuationPoint);
~~~~~~~~~~~~~

Once this object is added to the client, it will listen for raw data messages as soon the Middleware is on start.

To get the sensors data, get the latest sample (WeArtAnalogSensorData) from the AnalogSensorRawData object.
The sample contains the accelerometer, gyroscope and time of flight data, in addition to the timestamp of its sampling (generated by the middleware and represented as milliseconds in unix epoch time).
~~~~~~~~~~~~~{.cpp}
AnalogSensorRawData sample = anlogSensorData.LastSample;
sample.NtcTemperatureRaw;
sample.NtcTemperatureConverted;
sample.ForceSensingRaw;
sample.ForceSensingConverted;
sample.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff");
~~~~~~~~~~~~~

@note The Palm (control unit) doesn't contain a analog sensor, so its value is always set to 0.

## Middleware and Devices status tracking

The SDK allows to track and receives updates about the middleware and the connected devices status.

In particular, the information is available through a callback in the WeArtClient object
~~~~~~~~~~~~~{.cs}
weArtClient.OnMiddlewareStatusUpdate += (MiddlewareStatusUpdate message) => {
	... use received middleware status ...
};
~~~~~~~~~~~~~

The middleware status callback will receive the MiddlewareStatusUpdate object, which includes:
* Middleware version
* Middleware status
* Status code and description  
* Whether actuations are enabled or not
* List of the connected devices and their status
	* Mac Address
	* Assigned HandSide
	* Overall battery level
	* Status of each thimble (actuation point, connected or not, status code etc..)

### Status Codes
The MiddlewareListener object allows to get the middleware status, which includes the latest status code sent by the middleware while performing
its operations.

The current status codes (along with their description) are:

| Status Code |   | Description |
|---|---|---|
| 0 | OK | Ok |
| 100 | START_GENERIC_ERROR | Can't start generic error: Stopping |
| 101 | CONNECT_THIMBLE | Unable to start, connect at least one thimble and retry |
| 102 | WRONG_THIMBLES | Unable to start, connect the right thimbles matched to the bracelet and retry |
| 103 | BATTERY_TOO_LOW | Battery is too low, cannot start |
| 104 | FIRMWARE_COMPATIBILITY | Can't start while the devices are connected to the power supply |
| 105 | SET_IMU_SAMPLE_RATE_ERROR | Error while setting IMU Sample Rate! Device Disconnected! |
| 106 | RUNNING_SENSOR_ON_MASK | Inconsistency on Analog Sensors raw data! Please try again or Restart your device/s! |
| 107 | RUNNING_DEVICE_CHARGING | Can't start while the devices are connected to the power supply |
| 200 | CONSECUTIVE_TRACKING_ERRORS | Too many consecutive running sensor errors, stopping session |
| 201 | DONGLE_DISCONNECT_RUNNING | BLE Dongle disconnected while running, stopping session |
| 202 | TD_DISCONNECT_RUNNING | TouchDIVER disconnected while running, stopping session |
| 203 | DONGLE_CONNECTION_ERROR | Error on Dongle during connection phase! |
| 300 | STOP_GENERIC_ERROR | Generic error occurred while stopping session |

@note The description of each status code might change between different Middleware versions, use the status code to check instead of the description.