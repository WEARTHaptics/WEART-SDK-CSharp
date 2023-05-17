# Low-Level C# SDK

Welcome to the Weart Low-Level C# SDK documentation.

The SDK allows to connect to the Weart middleware and perform various actions with the TouchDIVER devices:
* Start and Stop the middleware operations
* Calibrate the device
* Receive tracking data from the devices
* Send haptic effects to the devices

# Architecture
<img src="./architecture.svg" width="100%" alt="C++ SDK Architecture" />

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
	weArtClient.OnCalibrationResultSuccess += (HandSide hand) => Console.WriteLine("Success!");
	weArtClient.OnCalibrationResultFail += (HandSide hand) => Console.WriteLine("Failed");

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

In order to send haptic feedback to the device, an effect must be created by implementing the IWeArtEffect interface.
The class shown here contains the effects without any processing, but it might be extended for other use cases
(e.g. values not directly set, but computed from other parameters).

~~~~~~~~~~~~~{.cs}
public class TouchEffect : IWeArtEffect {
	public event Action OnUpdate;
	public Temperature Temperature { get; private set; } = Temperature.Default;
	public Force Force { get; private set; } = Force.Default;
	public Texture Texture { get; private set; } = Texture.Default;

	public void Set(Temperature temperature, Force force, Texture texture)
	{
		force = (Force)force.Clone();
		texture = (Texture)texture.Clone();

		bool changed = !Temperature.Equals(temperature) || !Force.Equals(force) || !Texture.Equals(texture);

		Temperature = temperature;
		Force = force;
		texture.VelocityZ = 0.5f;
		Texture = texture;

		if (changed)
			OnUpdate?.Invoke();
	}
}
~~~~~~~~~~~~~

After defining the effect class, create the object on which the temperature, force and texture values will be applied:

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

@note When a new effect is added, the last effect applied is replaced by the new effect.

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