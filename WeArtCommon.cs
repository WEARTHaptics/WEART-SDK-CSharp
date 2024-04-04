/**
*	WEART - Common utility 
*	https://www.weart.it/
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using WeArt.Messages;

namespace WeArt.Core
{
    /// <summary>
    /// The hand side, left or right
    /// </summary>
    public enum HandSide
    {
        Left = 0,
        Right = 1
    };

    /// <summary>
    /// The multi-selectable version of <see cref="HandSide"/>
    /// </summary>
    [Flags]
    public enum HandSideFlags
    {
        None = 0,
        Left = 1 << HandSide.Left,
        Right = 1 << HandSide.Right
    };

    /// <summary>
    /// The point of application of the haptic feeling
    /// </summary>
    public enum ActuationPoint
    {
        Thumb = 0,
        Index = 1,
        Middle = 2,
        Palm = 3,
    };

    /// <summary>
    /// Type of tracking method and messages used/sent by the middleware
    /// </summary>
    public enum TrackingType
    {
        /// <summary>
        /// Default tracking type, only closures (for right/left thumb/index/middle/palm)
        /// </summary>
        [Description("")]
        DEFAULT = 0,

        /// <summary>
        /// Closure values for fingers, and thumb abduction value
        /// </summary>
        [Description("TrackType1")]
        WEART_HAND,
    }

    /// <summary>
    /// Contains extension and util methods for the TrackingType enum.
    /// </summary>
    public static class TrackingTypeExtension
    {
        /// <summary>
        /// Serializes the trackingtype enum by outputting its description
        /// </summary>
        /// <param name="type">Enum value to serialize</param>
        /// <returns>enum value serialized to string</returns>
        public static string Serialize(this TrackingType type)
        {
            return type.GetDescription();
        }

        /// <summary>
        /// Deserializes the trackingtype enum value from a given string, based on the type description
        /// </summary>
        /// <param name="str">String to deserialize into TrackingType</param>
        /// <returns>the deserialized TrackingType value</returns>
        public static TrackingType Deserialize(string str)
        {
            return Enum.GetValues(typeof(TrackingType))
                .Cast<TrackingType>()
                .FirstOrDefault(t => t.GetDescription() == str);
        }

        /// <summary>
        /// Get the value of the Description attribute for the given Tracking Type value
        /// </summary>
        /// <param name="value">Tracking Type value from which to get the description</param>
        /// <returns>tracking type value description, or empty string otherwise</returns>
        public static string GetDescription(this TrackingType value)
        {
            Type genericEnumType = value.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(value.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Any()))
                {
                    return ((DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return "";
        }
    }

    /// <summary>
    /// The multi-selectable version of <see cref="ActuationPoint"/>
    /// </summary>
    [Flags]
    public enum ActuationPointFlags
    {
        None = 0,
        Thumb = 1 << ActuationPoint.Thumb,
        Index = 1 << ActuationPoint.Index,
        Middle = 1 << ActuationPoint.Middle,
        Palm = 1 << ActuationPoint.Palm,
    };

    /// <summary>
    /// Texture type to Haptic feel
    /// </summary>
    public enum TextureType : int
    {
        ClickNormal = 0, ClickSoft = 1, DoubleClick = 2,
        AluminiumFineMeshSlow = 3, AluminiumFineMeshFast = 4,
        PlasticMeshSlow = 5, ProfiledAluminiumMeshMedium = 6, ProfiledAluminiumMeshFast = 7,
        RhombAluminiumMeshMedium = 8,
        TextileMeshMedium = 9,
        CrushedRock = 10,
        VenetianGranite = 11,
        SilverOak = 12,
        LaminatedWood = 13,
        ProfiledRubberSlow = 14,
        VelcroHooks = 15,
        VelcroLoops = 16,
        PlasticFoil = 17,
        Leather = 18,
        Cotton = 19,
        Aluminium = 20,
        DoubleSidedTape = 21
    }

    /// <summary>
    /// Enum Hand Closing State
    /// </summary>
    public enum HandClosingState
    {
        Open = 0,
        Closing = 1,
        Closed = 2
    }

    /// <summary>
    /// State of Hand Grasping
    /// </summary>
    public enum GraspingState
    {
        Grabbed = 0,
        Released = 1
    }

    /// <summary>
    /// Status of the current calibration procedure
    /// </summary>
    public enum CalibrationStatus
    {
        IDLE = 0,
        Calibrating = 1,
        Running = 2,
    };

    public struct Accelerometer
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public struct Gyroscope
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public struct TimeOfFlight
    {
        public int Distance { get; set; }
    }

    public class TrackingRawData
    {
        [JsonIgnore]
        public DateTime Timestamp { get; set; }

        public Accelerometer Accelerometer { get; set; }
        public Gyroscope Gyroscope { get; set; }
        public TimeOfFlight TimeOfFlight { get; set; }
    }

    public class AnalogSensorRawData
    {
        [JsonIgnore]
        public DateTime Timestamp { get; set; }

        public float NtcTemperatureRaw { get; set; }

        public float NtcTemperatureConverted { get; set; }
        public float ForceSensingRaw { get; set; }
        public float ForceSensingConverted { get; set; }
    }

    /// <summary>
    /// Data structure containing the updated status received from the middleware
    /// </summary>
    public class MiddlewareStatusUpdate
    {
        /// <summary>
        /// Timestamp of the last update
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Current Middleware status
        /// </summary>
        public MiddlewareStatus Status { get; set; } = MiddlewareStatus.DISCONNECTED;

        /// <summary>
        /// Current middleware version
        /// </summary>
        public string Version { get; set; } = "";

        /// <summary>
        /// Last status code received (0 = OK)
        /// </summary>
        public int StatusCode { get; set; } = 0;

        /// <summary>
        /// Description of the last status code received
        /// </summary>
        public string ErrorDesc { get; set; } = "";

        /// <summary>
        /// Tells whether the middleware will forward actuations to the devices or not
        /// </summary>
        public bool ActuationsEnabled { get; set; } = false;

        /// <summary>
        /// Status of the devices (TouchDIVERs) connected to the middleware
        /// </summary>
        public List<DeviceStatus> Devices { get; set; } = new List<DeviceStatus>();
    }

    public static class WeArtUtility
    {
        /// <summary>
        /// Normalized value from compute dinamic grasp between GraspForce and 1
        /// </summary>
        public static float NormalizedGraspForceValue(float value)
        {
            return Math.Clamp(value, WeArtConstants.graspForce, 1.0f);
        }
    }

    /// <summary>
    /// Constants shared by the WeArt components
    /// </summary>
    public static class WeArtConstants
    {
        public const string ipLocalHost = "127.0.0.1";

        public const string WEART_SDK_TYPE = "SdkLLCSH";
        public const string WEART_SDK_VERSION = "1.1.1";

        public const float defaultTemperature = 0.5f;
        public const float minTemperature = 0f;
        public const float maxTemperature = 1f;

        public const float defaultForce = 0f;
        public const float minForce = 0f;
        public const float maxForce = 1f;

        public const float defaultAbduction = 0.442f;
        public const float minAbduction = 0f;
        public const float maxAbduction = 1f;

        public const float defaultClosure = 0f;
        public const float minClosure = 0f;
        public const float maxClosure = 1f;

        public const int defaultTextureIndex = (int)TextureType.ClickNormal;
        public const int minTextureIndex = (int)TextureType.ClickNormal;
        public const int maxTextureIndex = (int)TextureType.DoubleSidedTape;
        public const int nullTextureIndex = 255;

        public const float defaultTextureVelocity = 0f;
        public const float minTextureVelocity = 0f;
        public const float maxTextureVelocity = 0.5f;

        public const float defaultCollisionMultiplier = 20.0f;
        public const float minCollisionMultiplier = 0f;
        public const float maxCollisionMultiplier = 100f;

        public const float defaultVolumeTexture = 100.0f;
        public const float minVolumeTexture = 0.0f;
        public const float maxVolumeTexture = 100.0f;

        public const float thresholdThumbClosure = 0.5f;
        public const float thresholdIndexClosure = 0.5f;
        public const float thresholdMiddleClosure = 0.5f;

        public const float graspForce = 0.3f;
        public const float dinamicForceSensibility = 10.0f;


        public static readonly IReadOnlyList<HandSide> HandSides = Enum.GetValues(typeof(HandSide))
            .Cast<HandSide>()
            .ToList();

        public static readonly IReadOnlyList<ActuationPoint> ActuationPoints = Enum.GetValues(typeof(ActuationPoint))
            .Cast<ActuationPoint>()
            .ToList();
    }

}