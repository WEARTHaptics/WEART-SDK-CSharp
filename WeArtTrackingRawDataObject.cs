/**
*	WEART - Thimble Tracking Object componet
*	https://www.weart.it/
*/

using WeArt.Core;
using WeArt.Messages;

namespace WeArt.Components
{
    /// <summary>
    /// This component receives and exposes raw sensors data from the hardware
    /// </summary>
    public class WeArtTrackingRawDataObject
    {
        public delegate void dRawSensorDataEvent(TrackingRawData sensorsData);

        public event dRawSensorDataEvent DataReceived;

        internal HandSide _handSide = HandSide.Left;
        internal ActuationPoint _actuationPoint = ActuationPoint.Thumb;

        private WeArtClient _client;

        /// <summary>
        /// The hand side of the thimble/sensor
        /// </summary>
        public HandSide HandSide
        {
            get => _handSide;
            set => _handSide = value;
        }

        /// <summary>
        /// The actuation point of the thimble/sensor
        /// </summary>
        public ActuationPoint ActuationPoint
        {
            get => _actuationPoint;
            set => _actuationPoint = value;
        }

        public TrackingRawData LastSample { get; private set; }

        public WeArtTrackingRawDataObject(WeArtClient client, HandSide handSide = HandSide.Right, ActuationPoint actuationPoint = ActuationPoint.Index)
        {
            _client = client;
            _client.OnConnectionStatusChanged -= OnConnectionChanged;
            _client.OnConnectionStatusChanged += OnConnectionChanged;
            _client.OnMessage -= OnMessageReceived;
            _client.OnMessage += OnMessageReceived;
            HandSide = handSide;
            ActuationPoint = actuationPoint;
            LastSample = new TrackingRawData { };
        }

        internal void OnConnectionChanged(bool connected)
        {
            LastSample = new TrackingRawData { };
        }

        private void OnMessageReceived(WeArtClient.MessageType type, IWeArtMessage message)
        {
            if (type != WeArtClient.MessageType.MessageReceived)
                return;

            if (message is RawDataMessage rawDataMessage)
            {
                if (rawDataMessage.HandSide != _handSide)
                    return;

                TrackingRawData newSample;
                switch (ActuationPoint)
                {
                    case ActuationPoint.Thumb: newSample = rawDataMessage.Thumb; break;
                    case ActuationPoint.Index: newSample = rawDataMessage.Index; break;
                    case ActuationPoint.Middle: newSample = rawDataMessage.Middle; break;
                    case ActuationPoint.Palm: newSample = rawDataMessage.Palm; break;
                    default: return;
                }
                newSample.Timestamp = rawDataMessage.Timestamp;
                LastSample = newSample;

                DataReceived?.Invoke(newSample);
            }
        }
    }
}