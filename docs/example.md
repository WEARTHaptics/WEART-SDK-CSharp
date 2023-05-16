# Example Project

An example application is available for download together with the C# and C++ SDK [here](https://weart.it/repository/downloads/sdk-low-level/WEART_Low_Level_API_v1.0.zip).

The application is implemented as a Universal Windows app in C#. The Visual Studio Solution is present inside an archive, at ```C#/WEART_API_Integration_v1.1.zip```.

To execute the application, just open the solution with Visual Studio and Run it (by clicking the play button or pressing F5).

![Example application](./example_app/ExampleApp_Screen.png)

## SDK Integration 

The SDK is integrated in the example application by copying the sdk source and header files into the ```WEART_API_Integration/WEART_SDK``` directory, and adding them to the visual studio project.
Most of the application code can be found in the ```WEART_API_Integration/MainPage.xaml.cs``` file.

### Connection to Middleware
The connection is managed using the WeArtClient class together with events to monitor the connection status. To manage the client class, the application uses a WeArtController. 

In particular, upon loading the main window, the application initialises the sdk client and adds a delegate for when the connection
status changes (connected or disconnected).

~~~~~~~~~~~~~{.cs}
using WeArt.Core;
using WeArt.Components;

private async void PageLoaded(object sender, RoutedEventArgs e) 
{
	WeArtController weArtController = new WeArtController();
	_weartClient = weArtController.Client;
	_weartClient.OnConnectionStatusChanged += OnConnectionChanged;
	...
}

private void OnConnectionChanged(bool connected)
{
	if (connected) {
		// Initialise empty effect
		CreateEffect();
	}
}
~~~~~~~~~~~~~

### Start/Stop middleware
By clicking on the "Start" and "Stop" buttons, it's possible to start and stop the middleware operations.
When clicking on the buttons, the application simply calls the corresponding SDK methods.

~~~~~~~~~~~~~{.cs}
private void StartClient_Click(object sender, RoutedEventArgs e)
{
	_weartClient.Start();
}

private void StopClient_Click(object sender, RoutedEventArgs e)
{
	_weartClient.Stop();
}
~~~~~~~~~~~~~

### Calibration
When the "Calibrate" button is clicked, if the middleware is started, the application will start the calibration procedure.

In addition, the application register to the calibration status and result events, to inform the user about the calibration progress.

~~~~~~~~~~~~~{.cs}
private async void PageLoaded(object sender, RoutedEventArgs e)
{
	...
	// handle calibration
	_weartClient.OnCalibrationStart += OnCalibrationStart;
	_weartClient.OnCalibrationFinish += OnCalibrationFinish;
	_weartClient.OnCalibrationResultSuccess += (HandSide hand) => OnCalibrationResult(hand, true);
	_weartClient.OnCalibrationResultFail += (HandSide hand) => OnCalibrationResult(hand, false);
	...
}

// Start calibration
private void StartCalibration_Click(object sender, RoutedEventArgs e)
{
	_weartClient.StartCalibration();
}

private void OnCalibrationStart(HandSide handSide)
{
	Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
	{
		CalibrationStatusText.Text = $"Calibrating {handSide.ToString().ToLower()} hand...";
		StartCalibration.IsEnabled = false;
	});
}

private void OnCalibrationFinish(HandSide handSide)
{
	Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
	{
		StartCalibration.IsEnabled = true;
	});
}

private void OnCalibrationResult(HandSide handSide, bool success)
{
	Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
	{
		CalibrationStatusText.Text = $"Calibration for {handSide.ToString().ToLower()} hand {(success ? "completed" : "failed")}";
	});
}
~~~~~~~~~~~~~

### Tracking data

The tracking data is read periodically from multiple WeArtThimbleTrackingObject objects,
declared when the application is opened.

~~~~~~~~~~~~~{.cs}
private async void PageLoaded(object sender, RoutedEventArgs e)
{
	...
	// Instantiate thimbles for tracking
	_leftIndexThimble = new WeArtThimbleTrackingObject(_weartClient, HandSide.Left, ActuationPoint.Index);
	_leftThumbThimble = new WeArtThimbleTrackingObject(_weartClient, HandSide.Left, ActuationPoint.Thumb);

	...

	// schedule timer to check tracking closure value
	Timer timer = new Timer();
	timer.Interval = 200; //Milliseconds
	timer.AutoReset = true;
	timer.Elapsed += OnTimerElapsed;
	timer.Start();
	...
}

private void OnTimerElapsed(object sender, ElapsedEventArgs e)
{
	Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
	{
		ValueIndexRightClosure.Text = _rightIndexThimble.Closure.Value.ToString();
		ValueThumbRightClosure.Text = _rightThumbThimble.Closure.Value.ToString();
		ValueThumbRightAbduction.Text = _rightThumbThimble.Abduction.Value.ToString();
		ValueMiddleRightClosure.Text = _rightMiddleThimble.Closure.Value.ToString();

		ValueIndexLeftClosure.Text = _leftIndexThimble.Closure.Value.ToString();
		ValueThumbLeftClosure.Text = _leftThumbThimble.Closure.Value.ToString();
		ValueThumbLeftAbduction.Text = _leftThumbThimble.Abduction.Value.ToString();
		ValueMiddleLeftClosure.Text = _leftMiddleThimble.Closure.Value.ToString();
	});
}
~~~~~~~~~~~~~

### Applying effects

To apply effects to the user hands, the application contains an internal ```TouchEffect``` class, which implements IWeArtEffect.

The application apply a different effect to the index finger when one of the "Add effect sample" buttons is clicked.

For example, when clicking the "Add effect sample 1" button, a slight pressure and cold temperature (without texture vibration)
are applied to the right index finger, as shown below:

~~~~~~~~~~~~~{.cs}
private async void PageLoaded(object sender, RoutedEventArgs e)
{
	...
	// instantiate Haptic Object Right hand for Index Thimble
	_hapticObject = new WeArtHapticObject(_weartClient);
	_hapticObject.HandSides = HandSideFlags.Right;
	_hapticObject.ActuationPoints = ActuationPointFlags.Index;
	...
}

private void AddEffectSample1_Click(object sender, RoutedEventArgs e)
{
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
	_effect.Set(temperature, force, texture);

	// add effect if needed, to thimble 
	if (_hapticObject.ActiveEffect == null)
		_hapticObject.AddEffect(_effect);
}
~~~~~~~~~~~~~