/**
*	WEART - Common utility 
*	https://www.weart.it/
*/

using System;
using System.Collections.Generic;
using System.Linq;

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

    public enum TrackingType
    {
        DEFAULT,
        WEART_HAND,
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
        public const string WEART_SDK_TYPE = "SdkLLCSH";
        public const string WEART_SDK_VERSION = "1.0.0";

        public const float defaultTemperature = 0.5f;
        public const float minTemperature = 0f;
        public const float maxTemperature = 1f;

        public const float defaultForce = 0f;
        public const float minForce = 0f;
        public const float maxForce = 1f;

        public const float defaultAbduction = 0.5f;
        public const float minAbduction = 0f;
        public const float maxAbduction = 1f;

        public const float defaultClosure = 0f;
        public const float minClosure = 0f;
        public const float maxClosure = 1f;

        public const int defaultTextureIndex = (int)TextureType.ClickNormal;
        public const int minTextureIndex = (int)TextureType.ClickNormal;
        public const int maxTextureIndex = (int)TextureType.DoubleSidedTape;
        public const int nullTextureIndex = 255;
        
        public const float defaultTextureVelocity_X = 0.5f;
        public const float defaultTextureVelocity_Y = 0f;
        public const float defaultTextureVelocity_Z = 0f;
        public const float minTextureVelocity = 0f;
        public const float maxTextureVelocity = 1f;

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