
/**
*	WEART - Analog sensor raw data
*	https://www.weart.it/
*/

using WeArt.Core;
using WeArt.Messages;

namespace WeArt.Components
{

    /// <summary>
    /// This component receives and exposes anlog raw sensors data from the thimbles
    /// </summary>
    public class WeArtAnalogSensorRawDataObject
    {
        public delegate void dAnalogSensorRawDataEvent(AnalogSensorRawData analogRawData);

        public event dAnalogSensorRawDataEvent DataReceived;

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

        public AnalogSensorRawData LastSample { get; private set; }

        public WeArtAnalogSensorRawDataObject(WeArtClient client, HandSide handSide = HandSide.Right, ActuationPoint actuationPoint = ActuationPoint.Index)
        {
            _client = client;
            _client.OnConnectionStatusChanged -= OnConnectionChanged;
            _client.OnConnectionStatusChanged += OnConnectionChanged;
            _client.OnMessage -= OnMessageReceived;
            _client.OnMessage += OnMessageReceived;
            HandSide = handSide;
            ActuationPoint = actuationPoint;
            LastSample = new AnalogSensorRawData { };
        }

        internal void OnConnectionChanged(bool connected)
        {
            LastSample = new AnalogSensorRawData { };
        }

        private void OnMessageReceived(WeArtClient.MessageType type, IWeArtMessage message)
        {
            if (type != WeArtClient.MessageType.MessageReceived)
                return;

            if (message is AnalogSensorsData anlogoRawData)
            {
                if (anlogoRawData.HandSide != _handSide)
                    return;

                AnalogSensorRawData newSample;
                switch (ActuationPoint)
                {
                    case ActuationPoint.Thumb: newSample = anlogoRawData.Thumb; break;
                    case ActuationPoint.Index: newSample = anlogoRawData.Index; break;
                    case ActuationPoint.Middle: newSample = anlogoRawData.Middle; break;
                    default: return;
                }
                newSample.Timestamp = anlogoRawData.Timestamp;
                LastSample = newSample;

                DataReceived?.Invoke(newSample);
            }
        }
    }
}
