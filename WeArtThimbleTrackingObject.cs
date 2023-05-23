/**
*	WEART - Thimble Tracking Object componet
*	https://www.weart.it/
*/

using WeArt.Core;
using WeArt.Messages;

namespace WeArt.Components
{
    /// <summary>
    /// This component receives and exposes tracking data from the hardware
    /// </summary>
    public class WeArtThimbleTrackingObject
    {
        internal HandSide _handSide = HandSide.Left;
        internal ActuationPoint _actuationPoint = ActuationPoint.Thumb;

        private WeArtClient _client;

        /// <summary>
        /// The hand side of the thimble
        /// </summary>
        public HandSide HandSide
        {
            get => _handSide;
            set => _handSide = value;
        }

        /// <summary>
        /// The actuation point of the thimble
        /// </summary>
        public ActuationPoint ActuationPoint
        {
            get => _actuationPoint;
            set => _actuationPoint = value;
        }

        /// <summary>
        /// The closure measure received from the hardware
        /// </summary>
        public Closure Closure { get; private set; }

        /// <summary>
        /// The abduction measure received from the hardware (if any)
        /// </summary>
        public Abduction Abduction { get; private set; }

        public WeArtThimbleTrackingObject(WeArtClient client, HandSide handSide = HandSide.Right, ActuationPoint actuationPoint = ActuationPoint.Index)
        {
            _client = client;
            _client.OnConnectionStatusChanged -= OnConnectionChanged;
            _client.OnConnectionStatusChanged += OnConnectionChanged;
            _client.OnMessage -= OnMessageReceived;
            _client.OnMessage += OnMessageReceived;
            HandSide = handSide;
            ActuationPoint = actuationPoint;
        }

        internal void OnConnectionChanged(bool connected)
        {
            Closure = new Closure() { Value = 0f };
        }

        private void OnMessageReceived(WeArtClient.MessageType type, IWeArtMessage message)
        {
            if (type != WeArtClient.MessageType.MessageReceived)
                return;

            if (message is TrackingMessage trackingMessage)
            {
                Closure = trackingMessage.GetClosure(HandSide, ActuationPoint);
                Abduction = trackingMessage.GetAbduction(HandSide, ActuationPoint);
            }
        }
    }
}