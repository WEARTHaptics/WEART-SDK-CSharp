# Example Project

An example source code application is available [here](https://github.com/WEARTHaptics/WEART-SDK-CSharp-Example).

The application is implemented as a Universal Windows app in C#. To execute the application, just open the solution with Visual Studio and Run it (by clicking the play button or pressing F5).

![Example application](./example_app/ExampleApp_Screen.png)

## SDK Integration 

The SDK is integrated in the example application by copying the sdk source and header files into the ```WEART_API_Integration/WEART_SDK``` directory, and adding them to the visual studio project.

In addition, the application project includes the nuget package "Newtonsoft.Json" version 13.0.3, as it is a required dependency of the SDK.

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

### Tracking raw data

![Raw Sensors Data Panel](./example_app/ExampleApp_RawData.png)

In the right section of the window, the application displays the raw data of the different sensors aboard the TouchDIVER.
In particular, it's possible to choose the hand and actuation point from which to visualize:
* Timestamp of the last sample received
* Accelerometer data (on the x,y,z axis)
* Gyroscope data (on the x,y,z axis)
* Time of Flight distance (in mm)

To start receiving raw data, click on the "Start Raw Data" button, and to stop click on the "Stop Raw Data" button.

When it's loaded, the application creates a WeArt.Components.WeArtTrackingRawDataObject for each pair of (HandSide, ActuationPoint).
When one of the combo boxes values changes, the application adds a callback to the corresponding tracking object.
The callback is responsible for displaying the received sample:

~~~~~~~~~~~~~{.cs}
private void RenderRawDataAsync(TrackingRawData data)
{
    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
    {
        Acc_X.Text = data.Accelerometer.X.ToString();
        Acc_Y.Text = data.Accelerometer.Y.ToString();
        Acc_Y.Text = data.Accelerometer.Z.ToString();

        Gyro_X.Text = data.Gyroscope.X.ToString();
        Gyro_Y.Text = data.Gyroscope.Y.ToString();
        Gyro_Y.Text = data.Gyroscope.Z.ToString();

        TimeOfFlight.Text = data.TimeOfFlight.Distance.ToString();

        LastSampleTime.Text = data.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff");
    });
}
~~~~~~~~~~~~~

### Analog Raw Sensors Data 

![Analog Sensors Data Panel](./example_app/ExampleApp_AnalogSensorData.png)

In the right section of the window, the application displays the anlog raw data of the different sensors aboard the TouchDIVER.
In particular, it's possible to choose the hand and actuation point from which to visualize:
* Timestamp of the last sample received
* NTC - Negative Temperature Coefficient (raw data and converted degree)
* FSR - force sensing resistor (raw adata and converted newton)

To start receiving analog sensor data, active this function on the Middleware and click on the "Start Raw Data" button, and to stop click on the "Stop Raw Data" button. In this modality the other tracking data will not received by the SDK.

When it's loaded, the application creates a WeArt.Components.WeArtAnalogSensorRawDataObject for each pair of (HandSide, ActuationPoint).
Using a timer, the application polls the chosen sensor and displays its data:

~~~~~~~~~~~~~{.cpp}
private void RenderAanlogSensorRawDataAsync(AnalogSensorRawData data)
{
    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
    {
        ntcTempRawValue.Text = data.NtcTemperatureRaw.ToString();
        ntcTempRawConvertedValue.Text = data.NtcTemperatureConverted.ToString();
        forceSensingRawValue.Text = data.ForceSensingRaw.ToString();
        forceSensingConvertedValue.Text = data.ForceSensingConverted.ToString();

        LastSampleTime.Text = data.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff");
    });
}
~~~~~~~~~~~~~

### Middleware and connected devices status

![Middleware/Devices Status Panel](./example_app/ExampleApp_MiddlewareStatus.png)

In the leftmost section of the window, the application displays the middleware and the connected devices status, as sent by the middleware during the connection.

To track and show the middleware and devices status, the example application uses the related callbacks on the WeArtClient object:
~~~~~~~~~~~~~{.cs}
_weartClient.OnMiddlewareStatusUpdate += UpdateUIBasedOnStatus;
 _weartClient.OnMiddlewareStatusUpdate += UpdateDevicesStatus;
~~~~~~~~~~~~~

In this way, the UI is updated on every middleware and devices status change sent by the middleware.
For the devices status, the example app uses a custom user control which receives the status of a device and shows it.

~~~~~~~~~~~~~{.cs}
// Update buttons and middleware status
private void UpdateUIBasedOnStatus(MiddlewareStatusUpdate statusUpdate)
{
	if (statusUpdate is null)
		return;

	MiddlewareStatus status = statusUpdate.Status;
	bool isRunning = status == MiddlewareStatus.RUNNING;

	Color statusColor = MiddlewareStatusColor(status);
	bool isStatusOk = statusUpdate.StatusCode == 0;

	Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (Windows.UI.Core.DispatchedHandler)(() =>
	{
		// Update buttons
		StartClient.IsEnabled = status != MiddlewareStatus.RUNNING && status != MiddlewareStatus.STARTING;
		StopClient.IsEnabled = isRunning;
		StartCalibration.IsEnabled = status == MiddlewareStatus.RUNNING;

		AddEffectSample1.IsEnabled = isRunning;
		AddEffectSample2.IsEnabled = isRunning;
		AddEffectSample3.IsEnabled = isRunning;
		RemoveEffects.IsEnabled = isRunning;
		ButtonStartRawData.IsEnabled = isRunning;
		ButtonStopRawData.IsEnabled = isRunning;


		// Update middleware status panel
		MiddlewareStatus_Text.Text = status.ToString();
		MiddlewareStatus_Text.Foreground = new SolidColorBrush(statusColor);

		if(statusUpdate.Version != null)
			MiddlewareVersion_Text.Text = statusUpdate.Version;

		Brush statusCodeBrush = new SolidColorBrush(isStatusOk ? Colors.Green : Colors.Red);
		MwStatusCode.Text = statusUpdate.StatusCode.ToString();
		MwStatusCode.Foreground = statusCodeBrush;
		MwStatusCodeDesc.Text = isStatusOk ? "OK" : (statusUpdate.ErrorDesc != null ? statusUpdate.ErrorDesc : "");
		MwStatusCodeDesc.Foreground = statusCodeBrush;

		ConnectedDevicesNum_Text.Text = statusUpdate.Devices.Count.ToString();

		AddEffectSample1.IsEnabled = isRunning;
		AddEffectSample2.IsEnabled = isRunning;
		AddEffectSample3.IsEnabled = isRunning;
		RemoveEffects.IsEnabled = isRunning;
		ButtonStartRawData.IsEnabled = isRunning;
		ButtonStopRawData.IsEnabled = isRunning;
	}));
}

private void UpdateDevicesStatus(MiddlewareStatusUpdate statusUpdate)
{
	LeftHand.Connected = false;
	RightHand.Connected = false;
	foreach (DeviceStatus device in statusUpdate.Devices)
	{
		if(device.HandSide == HandSide.Left)
		{
			LeftHand.Device = device;
			LeftHand.Connected = true;
		} else
		{
			RightHand.Device = device;
			RightHand.Connected = true;
		}
	}
	LeftHand.Refresh();
	RightHand.Refresh();
}
~~~~~~~~~~~~~