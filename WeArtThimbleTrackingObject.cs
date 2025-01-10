/**
*	WEART - Thimble Tracking Object componet
*	https://www.weart.it/
*/

using System;
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
                return;
            }
            else if(message is TrackingMessageG2 trackingMessageG2)
            {
                if (trackingMessageG2.HandSide != _handSide) return;

                ThimbleData thimbleData = GetThimbleData(trackingMessageG2);

                Closure = new Closure { Value = thimbleData.Closure };
                Abduction = new Abduction { Value = thimbleData.Abduction };
                return;
            }
        }

        private ThimbleData GetThimbleData(TrackingMessageG2 trackingMessageG2)
        {
            switch (_actuationPoint)
            {
                case ActuationPoint.Thumb:
                    return trackingMessageG2.Thumb;
                case ActuationPoint.Index:
                    return trackingMessageG2.Index;
                case ActuationPoint.Middle:
                    return trackingMessageG2.Middle;
                case ActuationPoint.Annular:
                    return trackingMessageG2.Annular;
                case ActuationPoint.Pinky:
                    return trackingMessageG2.Pinky;
                case ActuationPoint.Palm:
                    return trackingMessageG2.Palm;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_actuationPoint));
            }
        }
    }
}