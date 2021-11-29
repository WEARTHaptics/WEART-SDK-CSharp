using WeArt.Core;
using static WeArt.Messages.WeArtMessageCustomSerializer;

namespace WeArt.Messages
{
    /// <summary>
    /// Interface for all messages sent or received on communicating with the middleware
    /// </summary>
    public interface IWeArtMessage { }


    /// <summary>
    /// Message that requests the middleware to start and to turn on the hardware
    /// </summary>
    [WeArtMiddlewareMessageID("StartFromClient")]
    public class StartFromClientMessage : IWeArtMessage { }


    /// <summary>
    /// Message that requests the middleware to stop and to turn off the hardware
    /// </summary>
    [WeArtMiddlewareMessageID("StopFromClient")]
    public class StopFromClientMessage : IWeArtMessage { }


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
    public class TrackingMessage : IWeArtMessage
    {
        public byte RightThumbClosure;
        public byte RightIndexClosure;
        public byte RightMiddleClosure;
        public byte RightPalmClosure;
        public byte LeftThumbClosure;
        public byte LeftIndexClosure;
        public byte LeftMiddleClosure;
        public byte LeftPalmClosure;

        public Closure GetClosure(HandSide handSide, ActuationPoint actuationPoint)
        {
            byte byteValue = 0x00;
            switch (handSide)
            {
                case HandSide.Left:
                    switch (actuationPoint)
                    {
                        case ActuationPoint.Thumb:  byteValue = LeftThumbClosure;  break;
                        case ActuationPoint.Index:  byteValue = LeftIndexClosure;  break;
                        case ActuationPoint.Middle: byteValue = LeftMiddleClosure; break;
                        case ActuationPoint.Palm:   byteValue = LeftPalmClosure;   break;
                    }
                    break;
                case HandSide.Right:
                    switch (actuationPoint)
                    {
                        case ActuationPoint.Thumb:  byteValue = RightThumbClosure;  break;
                        case ActuationPoint.Index:  byteValue = RightIndexClosure;  break;
                        case ActuationPoint.Middle: byteValue = RightMiddleClosure; break;
                        case ActuationPoint.Palm:   byteValue = RightPalmClosure;   break;
                    }
                    break;
            }

            return new Closure()
            {
                Value = byteValue / (float)byte.MaxValue
            };
        }
    }
}