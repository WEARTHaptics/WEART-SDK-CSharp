using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using WeArt.Core;
using static WeArt.Messages.WeArtMessageCustomSerializer;
using System.Numerics;

namespace WeArt.Messages
{
    /// <summary>
    /// Interface for all messages sent or received on communicating with the middleware
    /// </summary>
    public interface IWeArtMessage { }

    /// <summary>
    /// Interface for all messages that need a custom serialization procedure
    /// (e.g. non-serializable fields, custom serialization based on type or fields)
    /// </summary>
    public interface IWeArtMessageCustomSerialization
    {
        string[] Serialize();

        bool Deserialize(string[] fields);
    }

    public class WeArtJsonMessage : IWeArtMessage
    {
        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Message that requests the middleware to start and to turn on the hardware
    /// </summary>
    [WeArtMiddlewareMessageID("StartFromClient")]
    public class StartFromClientMessage : IWeArtMessage, IWeArtMessageCustomSerialization
    {
        public string SdkType = WeArtConstants.WEART_SDK_TYPE;
        public string SdkVersion = WeArtConstants.WEART_SDK_VERSION;
        public TrackingType TrackingType = TrackingType.WEART_HAND;

        public bool Deserialize(string[] fields)
        {
            // Default (deprecated)
            if (fields.Length == 1)
            {
                TrackingType = TrackingType.DEFAULT;
            }
            // Other (newer) types
            else
            {
                SdkType = fields[1];
                SdkVersion = fields[2];
                TrackingType = TrackingTypeExtension.Deserialize(fields[3]);
            }
            return true;
        }

        public string[] Serialize()
        {
            if (TrackingType == TrackingType.DEFAULT)
                return new string[] { "StartFromClient" };
            else
                return new string[] { "StartFromClient", SdkType, SdkVersion, TrackingType.Serialize() };
        }
    }


    /// <summary>
    /// Message that requests the middleware to stop and to turn off the hardware
    /// </summary>
    [WeArtMiddlewareMessageID("StopFromClient")]
    public class StopFromClientMessage : IWeArtMessage { }

    /// <summary>
    /// Message that requests the middleware to start the calibration procedure
    /// </summary>
    [WeArtMiddlewareMessageID("StartCalibration")]
    public class StartCalibrationMessage : IWeArtMessage { }

    /// <summary>
    /// Message that requests the middleware to stop the calibration procedure
    /// </summary>
    [WeArtMiddlewareMessageID("StopCalibration")]
    public class StopCalibrationMessage : IWeArtMessage { }

    /// <summary>
    /// Message that requests the middleware to reset the calibration procedure
    /// </summary>
    [WeArtMiddlewareMessageID("ResetCalibration")]
    public class ResetCalibrationMessage : IWeArtMessage { }

    /// <summary>
    /// Message received from the middleware containing the current calibration procedure status
    /// </summary>
    [WeArtMiddlewareMessageID("CalibrationStatus")]
    public class TrackingCalibrationStatus : IWeArtMessage
    {
        private byte _handSide;
        private byte _status;

        public HandSide HandSide
        {
            get => _handSide == 0 ? HandSide.Left : HandSide.Right;
            set => _handSide = value == HandSide.Left ? (byte)0 : (byte)1;
        }
        public CalibrationStatus Status
        {
            get => (CalibrationStatus)_status;
            set => _status = (byte)value;
        }
    }

    /// <summary>
    /// Message received from the middleware containing the result of the calibration procedure
    /// </summary>
    [WeArtMiddlewareMessageID("CalibrationResult")]
    public class TrackingCalibrationResult : IWeArtMessage
    {
        private byte _handSide;
        private byte _success;

        public HandSide HandSide
        {
            get => _handSide == 0 ? HandSide.Left : HandSide.Right;
            set => _handSide = value == HandSide.Left ? (byte)0 : (byte)1;
        }
        public bool Success
        {
            get => _success == 0;
            set => _success = value ? (byte)0 : (byte)1;
        }
    }

    /// <summary>
    /// Message received from the middleware upon closing it
    /// </summary>
    [WeArtMiddlewareMessageID("exit")]
    public class ExitMessage : IWeArtMessage { }


    /// <summary>
    /// Message received from the middleware upon disconnection
    /// </summary>
    [WeArtMiddlewareMessageID("disconnect")]
    public class DisconnectMessage : IWeArtMessage { }


    /// <summary>
    /// Message sent to the middleware to set the temperature of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("temperature")]
    public class SetTemperatureMessage : IWeArtMessage
    {
        public float Temperature;
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to stop the temperature actuator of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("stopTemperature")]
    public class StopTemperatureMessage : IWeArtMessage
    {
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to set the pressure force of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("force")]
    public class SetForceMessage : IWeArtMessage
    {
        public float[] Force;
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to stop the force actuator of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("stopForce")]
    public class StopForceMessage : IWeArtMessage
    {
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to set the haptic texture of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("texture")]
    public class SetTextureMessage : IWeArtMessage
    {
        public int TextureIndex;
        public float[] TextureVelocity;
        public float TextureVolume;
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to stop the haptic texture actuator of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("stopTexture")]
    public class StopTextureMessage : IWeArtMessage
    {
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message received from the middleware containing the closure amount of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("Tracking")]
    public class TrackingMessage : IWeArtMessage, IWeArtMessageCustomSerialization
    {
        public TrackingType TrackingType { get; set; }
        private readonly Dictionary<(HandSide, ActuationPoint), byte> Closures = new Dictionary<(HandSide, ActuationPoint), byte>();
        private readonly Dictionary<(HandSide, ActuationPoint), byte> Abductions = new Dictionary<(HandSide, ActuationPoint), byte>();

        /// <summary>
        /// Allows to get the closure value for agiven actuation point on a given hand.
        /// </summary>
        /// <param name="handSide">Hand from which to select the actuation point</param>
        /// <param name="actuationPoint">Actuation point from which to take the closure</param>
        /// <returns>the closure value of the given point and hand</returns>
        public Closure GetClosure(HandSide handSide, ActuationPoint actuationPoint)
        {
            float closureMaxValue = 255;
            if (Closures.ContainsKey((handSide, actuationPoint)))
                return new Closure { Value = Closures[(handSide, actuationPoint)] / closureMaxValue };
            return new Closure { Value = 0 };
        }

        /// <summary>
        /// Allows to get the abduction value for agiven actuation point on a given hand.
        /// The presence of this data depends on the type of tracking message received,
        /// which depends on the parameters sent to the middleware with the start command.
        /// In particular:
        /// * DEFAULT: No abduction values
        /// * CLAP_HAND: Abduction values for index and middle finger
        /// </summary>
        /// <param name="handSide">Hand from which to select the actuation point</param>
        /// <param name="actuationPoint">Actuation point from which to take the closure</param>
        /// <returns>the closure value of the given point and hand</returns>
        public Abduction GetAbduction(HandSide handSide, ActuationPoint actuationPoint)
        {
            float abductionMaxValue = 255;
            if (Abductions.ContainsKey((handSide, actuationPoint)))
                return new Abduction { Value = Abductions[(handSide, actuationPoint)] / abductionMaxValue };
            return new Abduction { Value = 0 };
        }

        public bool Deserialize(string[] fields)
        {
            try
            {
                TrackingType = TrackingTypeExtension.Deserialize(fields[1]);
                switch (TrackingType)
                {
                    case TrackingType.DEFAULT: DeserializeDefault(fields); break;
                    case TrackingType.WEART_HAND: DeserializeWeart(fields); break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string[] Serialize()
        {
            switch (TrackingType)
            {
                case TrackingType.DEFAULT: return SerializeDefault();
                case TrackingType.WEART_HAND: return SerializeWeart();
            }
            return new string[] { };
        }

        // Serialization/Deserialization utils

        private void DeserializeDefault(string[] fields)
        {
            Closures[(HandSide.Right, ActuationPoint.Thumb)] = byte.Parse(fields[1]);
            Closures[(HandSide.Right, ActuationPoint.Index)] = byte.Parse(fields[2]);
            Closures[(HandSide.Right, ActuationPoint.Middle)] = byte.Parse(fields[3]);
            Closures[(HandSide.Right, ActuationPoint.Palm)] = byte.Parse(fields[4]);
            Closures[(HandSide.Left, ActuationPoint.Thumb)] = byte.Parse(fields[5]);
            Closures[(HandSide.Left, ActuationPoint.Index)] = byte.Parse(fields[6]);
            Closures[(HandSide.Left, ActuationPoint.Middle)] = byte.Parse(fields[7]);
            Closures[(HandSide.Left, ActuationPoint.Palm)] = byte.Parse(fields[8]);
        }

        private string[] SerializeDefault()
        {
            string[] fields = new string[9];
            int i = 0;
            fields[i++] = "Tracking";
            fields[i++] = Closures[(HandSide.Right, ActuationPoint.Thumb)].ToString();
            fields[i++] = Closures[(HandSide.Right, ActuationPoint.Index)].ToString();
            fields[i++] = Closures[(HandSide.Right, ActuationPoint.Middle)].ToString();
            fields[i++] = Closures[(HandSide.Right, ActuationPoint.Palm)].ToString();
            fields[i++] = Closures[(HandSide.Left, ActuationPoint.Thumb)].ToString();
            fields[i++] = Closures[(HandSide.Left, ActuationPoint.Index)].ToString();
            fields[i++] = Closures[(HandSide.Left, ActuationPoint.Middle)].ToString();
            fields[i] = Closures[(HandSide.Left, ActuationPoint.Palm)].ToString();
            return fields;
        }

        private void DeserializeWeart(string[] fields)
        {
            // Right Hand
            Closures[(HandSide.Right, ActuationPoint.Index)] = byte.Parse(fields[2]);
            Closures[(HandSide.Right, ActuationPoint.Thumb)] = byte.Parse(fields[3]);
            Abductions[(HandSide.Right, ActuationPoint.Thumb)] = byte.Parse(fields[4]);
            Closures[(HandSide.Right, ActuationPoint.Middle)] = byte.Parse(fields[5]);

            // Left Hand
            Closures[(HandSide.Left, ActuationPoint.Index)] = byte.Parse(fields[6]);
            Closures[(HandSide.Left, ActuationPoint.Thumb)] = byte.Parse(fields[7]);
            Abductions[(HandSide.Left, ActuationPoint.Thumb)] = byte.Parse(fields[8]);
            Closures[(HandSide.Left, ActuationPoint.Middle)] = byte.Parse(fields[9]);
        }

        private string[] SerializeWeart()
        {
            string[] fields = new string[10];
            int i = 0;
            fields[i++] = "Tracking";
            fields[i++] = TrackingType.Serialize();

            // Right Hand
            fields[i++] = Closures[(HandSide.Right, ActuationPoint.Index)].ToString();
            fields[i++] = Closures[(HandSide.Right, ActuationPoint.Thumb)].ToString();
            fields[i++] = Abductions[(HandSide.Right, ActuationPoint.Thumb)].ToString();
            fields[i++] = Closures[(HandSide.Right, ActuationPoint.Middle)].ToString();

            // Left Hand
            fields[i++] = Closures[(HandSide.Left, ActuationPoint.Index)].ToString();
            fields[i++] = Closures[(HandSide.Left, ActuationPoint.Thumb)].ToString();
            fields[i++] = Abductions[(HandSide.Left, ActuationPoint.Thumb)].ToString();
            fields[i] = Closures[(HandSide.Left, ActuationPoint.Middle)].ToString();

            return fields;
        }
    }

    /// <summary>
    /// Message received from the middleware containing the closure data for G2 devices
    /// </summary>
    [WeArtMiddlewareMessageID("TRACKING_BENDING_G2")]
    public class TrackingMessageG2 : WeArtJsonMessage
    {
        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }
        public ThimbleData Index { get; set; }
        public ThimbleData Middle { get; set; }
        public ThimbleData Thumb { get; set; }
        public ThimbleData Annular { get; set; }
        public ThimbleData Pinky { get; set; }
        public ThimbleData Palm { get; set; }
        public Wrist Wrist { get; set; }

    }
    public class ThimbleData
    {
        public float Closure { get; set; }
        public float Abduction { get; set; }
    }

    public class Wrist
    {
        public Quaternion Quaternion { get; set; }
    }

    [WeArtMiddlewareMessageID("RAW_DATA_ON")]
    public class RawDataOnMessage : WeArtJsonMessage { }

    [WeArtMiddlewareMessageID("RAW_DATA_OFF")]
    public class RawDataOffMessage : WeArtJsonMessage { }

    [WeArtMiddlewareMessageID("RAW_DATA")]
    public class RawDataMessage : WeArtJsonMessage
    {
        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }
        public TrackingRawData Index { get; set; }
        public TrackingRawData Thumb { get; set; }
        public TrackingRawData Middle { get; set; }
        public TrackingRawData Palm { get; set; }
    }

    [WeArtMiddlewareMessageID("RAW_SENSOR_ON_MASK")]
    public class AnalogSensorsData : WeArtJsonMessage
    {
        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }

        public ActuationPoint ActuationPoint { get; set; }
        public AnalogSensorRawData Index { get; set; }
        public AnalogSensorRawData Thumb { get; set; }
        public AnalogSensorRawData Middle { get; set; }
    }

    [WeArtMiddlewareMessageID("MW_GET_STATUS")]
    public class GetMiddlewareStatusMessage : WeArtJsonMessage { }


    /// <summary>
    /// Defines the STATUS.
    /// </summary>
    public enum MiddlewareStatus
    {
        DISCONNECTED,
        IDLE,
        STARTING,
        RUNNING,
        STOPPING,
        UPLOADING_TEXTURES,
        CONNECTING_DEVICE,
        CALIBRATION,
    };

    /// <summary>
    /// Defines the TD connection type.
    /// </summary>
    public enum ConnectionType
    {
        NONE,
        BLE,
        WIFI,
        USB
    }

    /// <summary>
    /// Defines the selected type of devices at WeartApp
    /// </summary>
    public enum DeviceSelection
    {
        VirtualDevices,
        PhysicalDevices
    }

    [WeArtMiddlewareMessageID("MW_STATUS")]
    public class MiddlewareStatusMessage : WeArtJsonMessage
    {
        [JsonConverter(typeof(EnumConverter<MiddlewareStatus>))]
        public MiddlewareStatus Status { get; set; } = MiddlewareStatus.DISCONNECTED;

        public string Version { get; set; } = "";

        public int StatusCode { get; set; } = 0;

        public string ErrorDesc { get; set; } = "";

        public bool ActuationsEnabled { get; set; } = false;

        [JsonConverter(typeof(ListConverter<MiddlewareConnectedDevice>))]
        public List<MiddlewareConnectedDevice> ConnectedDevices { get; set; } = new List<MiddlewareConnectedDevice>();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    [WeArtMiddlewareMessageID("DEVICES_STATUS")]
    public class DevicesStatusMessage : WeArtJsonMessage
    {
        [JsonConverter(typeof(ListConverter<DeviceStatusData>))]
        public List<DeviceStatusData> Devices { get; set; } = new List<DeviceStatusData> { };

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    /// <summary>
    /// Status and informations about a single connected device
    /// </summary>
    public struct DeviceStatus
    {
        /// <summary>
        /// (BLE) Mac address of the device
        /// </summary>
        public string MacAddress { get; set; }

        /// <summary>
        /// Hand side to which the device is assigned
        /// </summary>
        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }

        /// <summary>
        /// Current battery charge level (percentage, 0-100)
        /// </summary>
        public int BatteryLevel { get; set; }

        /// <summary>
        /// Tells whether the device is charging or not
        /// </summary>
        public bool Charging { get; set; }

        /// <summary>
        /// Status of all the device thimbles
        /// </summary>
        public List<ThimbleStatus> Thimbles { get; set; }
    }

    public class DeviceStatusData : ITouchDiverData
    {
        public string MacAddress { get; set; }

        public string FirmwareVersion { get; set; }

        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }

        public int BatteryLevel { get; set; }

        public bool Charging { get; set; }

        [JsonConverter(typeof(ListConverter<ThimbleStatus>))]
        public List<ThimbleStatus> Thimbles { get; set; } = new List<ThimbleStatus> { };
    }

    public class ThimbleStatus
    {
        [JsonConverter(typeof(EnumConverter<ActuationPoint>))]
        public ActuationPoint Id { get; set; }

        public bool Connected { get; set; }

        public int StatusCode { get; set; }

        public string ErrorDesc { get; set; }
    }

    public class MiddlewareConnectedDevice
    {
        public string MacAddress { get; set; }

        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }
    }

    [WeArtMiddlewareMessageID("DEVICES_GET_STATUS")]
    public class GetDevicesStatusMessage : WeArtJsonMessage { }


    [WeArtMiddlewareMessageID("WA_STATUS")]
    public class WeartAppStatusMessage : WeArtJsonMessage
    {
        [JsonConverter(typeof(EnumConverter<MiddlewareStatus>))]
        public MiddlewareStatus Status { get; set; } = MiddlewareStatus.DISCONNECTED;
        public string Version { get; set; } = "";
        public int StatusCode { get; set; } = 0;
        public string ErrorDesc { get; set; } = "";
        public bool ActuationsEnabled { get; set; } = false;

        [JsonConverter(typeof(EnumConverter<ConnectionType>))]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.BLE;
        public bool AutoConnection { get; set; } = false;

        [JsonConverter(typeof(EnumConverter<DeviceSelection>))]
        public DeviceSelection DeviceSelection { get; set; } = DeviceSelection.VirtualDevices;

        public bool TrackingPlayback { get; set; } = false;
        public bool RawDataLog { get; set; } = false;
        public bool SensorOnMask { get; set; } = false;

        [JsonConverter(typeof(ListConverter<MiddlewareConnectedDevice>))]
        public List<MiddlewareConnectedDevice> ConnectedDevices { get; set; } = new List<MiddlewareConnectedDevice>();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public interface ITouchDiverData
    {
        string MacAddress { get; set; }

        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }
    }

    /// <summary>
    /// Message received from the middleware containing the TouchDiver Pro Status
    /// </summary>
    [WeArtMiddlewareMessageID("WEART_TD_PRO_STATUS")]
    public class TouchDiverProStatus : WeArtJsonMessage
    {
        [JsonConverter(typeof(ListConverter<TouchDiverProStatusData>))]
        public List<TouchDiverProStatusData> Devices { get; set; } = new List<TouchDiverProStatusData>();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class TouchDiverProStatusData : ITouchDiverData
    {
        public string MacAddress { get; set; }

        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }

        public MasterData Master { get; set; } = new MasterData();

        [JsonConverter(typeof(ListConverter<TouchDiverProThimbleStatus>))]
        public List<TouchDiverProThimbleStatus> Nodes { get; set; } = new List<TouchDiverProThimbleStatus>();
    }

    public class MasterData
    {
        public int BatteryLevel { get; set; }
        public bool Charging { get; set; }
        public bool ChargeCompleted { get; set; }
        public ConnectionData Connection { get; set; } = new ConnectionData();
        public bool ImuFault { get; set; }
        public bool AdcFault { get; set; }
        public bool ButtonPushed { get; set; }
    }

    public class ConnectionData
    {
        public bool BluetoothOn { get; set; }
        public bool BluetoothConnected { get; set; }
        public bool WifiOn { get; set; }
        public bool WifiConnected { get; set; }
        public bool UsbConnected { get; set; }
    }

    public class TouchDiverProThimbleStatus
    {
        [JsonConverter(typeof(EnumConverter<ActuationPoint>))]
        public ActuationPoint Id { get; set; }
        public bool Connected { get; set; }
        public bool ImuFault { get; set; }
        public bool AdcFault { get; set; }
        public bool TofFault { get; set; }
    }

    /// <summary>
    /// Message received from the middleware containing the TouchDiver Pro Raw Data 
    /// </summary>
    [WeArtMiddlewareMessageID("RAW_DATA_TD_PRO")]
    public class RawDataTouchDiverPro : WeArtJsonMessage
    {
        [JsonConverter(typeof(EnumConverter<HandSide>))]
        public HandSide HandSide { get; set; }
        public TrackingRawDataG2 Index { get; set; } = new TrackingRawDataG2();
        public TrackingRawDataG2 Middle { get; set; } = new TrackingRawDataG2();
        public TrackingRawDataG2 Thumb { get; set; } = new TrackingRawDataG2();
        public TrackingRawDataG2 Annular { get; set; } = new TrackingRawDataG2();
        public TrackingRawDataG2 Pinky { get; set; } = new TrackingRawDataG2();
        public TrackingRawDataG2 Palm { get; set; } = new TrackingRawDataG2();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}