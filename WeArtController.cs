/**
*	WEART - Controller and client wrapper
*	https://www.weart.it/
*/

using System;
using WeArt.Core;
using WeArt.Utils;
using ClientError = WeArt.Core.WeArtClient.ErrorType;

namespace WeArt.Components
{
    /// <summary>
    /// This component wraps and exposes the network client that communicates with the hardware middleware.
    /// Use the <see cref="Instance"/> singleton property to retrieve the instance.
    /// </summary>
    public class WeArtController
    {
        
        internal int _clientPort = 13031;

        internal bool _debugMessages = false;

        private WeArtClient _weArtClient;

        public WeArtController()
        {

        }

        /// <summary>
        /// The network client that communicates with the hardware middleware.
        /// </summary>
        public WeArtClient Client
        {
            get
            {
                if (_weArtClient == null)
                {
                    _weArtClient = new WeArtClient
                    {
                        IpAddress = WeArtNetwork.LocalIPAddress,
                        Port = _clientPort,
                    };
                    _weArtClient.OnConnectionStatusChanged += OnConnectionChanged;
                    _weArtClient.OnTextMessage += OnMessage;
                    _weArtClient.OnError += OnError;
                }
                return _weArtClient;
            }
        }

        private void OnConnectionChanged(bool connected)
        {
            if (connected)
                WeArtLog.Log($"Connected to {Client.IpAddress}.");
            else
                WeArtLog.Log($"Disconnected from {Client.IpAddress}.");
        }

        private void OnMessage(WeArtClient.MessageType type, string message)
        {
            if (!_debugMessages)
                return;

            if (type == WeArtClient.MessageType.MessageSent)
                WeArtLog.Log($"To Middleware: \"{message}\"");

            else if (type == WeArtClient.MessageType.MessageReceived)
                WeArtLog.Log($"From Middleware: \"{message}\"");
        }

        private void OnError(ClientError error, Exception exception)
        {
            string errorMessage;
            switch (error)
            {
                case ClientError.ConnectionError:
                    errorMessage = $"Cannot connect to {Client.IpAddress}";
                    break;
                case ClientError.SendMessageError:
                    errorMessage = $"Error on send message";
                    break;
                case ClientError.ReceiveMessageError:
                    errorMessage = $"Error on message received";
                    break;
                default:
                    throw new NotImplementedException();
            }
            WeArtLog.Log($"{errorMessage}\n{exception.StackTrace}");
        }
    }
}