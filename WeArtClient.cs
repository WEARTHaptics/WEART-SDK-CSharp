using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WeArt.Messages;

namespace WeArt.Core
{
    /// <summary>
    /// Network client used to send and receive message to/from the hardware middleware.
    /// This class is meant to be used internally but can also be used by developers who want
    /// to communicate with the hardware in a direct way.
    /// </summary>
    public class WeArtClient
    {

        /// <summary>
        /// Possible message types
        /// </summary>
        public enum MessageType
        {
            MessageSent, MessageReceived
        }

        /// <summary>
        /// Possible error types
        /// </summary>
        public enum ErrorType
        {
            ConnectionError, SendMessageError, ReceiveMessageError
        }


        // Threading
        private CancellationTokenSource _cancellation;
        private readonly SynchronizationContext _mainThreadContext = SynchronizationContext.Current;

        // Networking
        private string _ipAddress;
        private int _port;
        private Socket _socket;

        // Messaging
        private static readonly char _messagesSeparator = '~';
        private readonly WeArtMessageSerializer _messageSerializer = new WeArtMessageCustomSerializer();
        private readonly byte[] _messageReceivedBuffer = new byte[1024];
        private string _trailingText = string.Empty;


        /// <summary>
        /// Called when the connection has been established (true) and when it is closed (false)
        /// </summary>
        public event Action<bool> OnConnectionStatusChanged;

        /// <summary>
        /// Called when a <see cref="IWeArtMessage"/> is sent or received
        /// </summary>
        public event Action<MessageType, IWeArtMessage> OnMessage;

        /// <summary>
        /// Called when a <see cref="IWeArtMessage"/> is serialized or deserialized
        /// </summary>
        public event Action<MessageType, string> OnTextMessage;

        /// <summary>
        /// Called when an error occurs
        /// </summary>
        public event Action<ErrorType, Exception> OnError;

        /// <summary>
        /// True if a connection to the middleware has been established
        /// </summary>
        public bool IsConnected => _socket != null && _socket.Connected;


        /// <summary>
        /// The IP address of the middleware network endpoint
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value;
        }

        /// <summary>
        /// The port of the middleware network endpoint
        /// </summary>
        public int Port
        {
            get => _port;
            set => _port = value;
        }

        /// <summary>
        /// Establishes a connection to the middleware and send the <see cref="StartFromClientMessage"/>
        /// </summary>
        /// <returns>The task running during the entire life of the network connection</returns>
        public Task Start()
        {
            _cancellation = new CancellationTokenSource();
            return Task.Run(() =>
            {
                // Connection loop
                while (!_cancellation.IsCancellationRequested)
                {
                    try
                    {
                        // Create the socket
                        IPAddress ipAddr = IPAddress.Parse(_ipAddress);
                        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, _port);
                        _socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        // Connect to it
                        _socket.Connect(localEndPoint);
                        _mainThreadContext.Post(state => OnConnectionStatusChanged?.Invoke(true), this);

                        // Send the request to start
                        SendMessage(new StartFromClientMessage());
                    }
                    catch (Exception e)
                    {
                        // Error handling and connection stop
                        OnError?.Invoke(ErrorType.ConnectionError, e);
                        StopConnection();
                    }

                    // Message receiving loop
                    while (!_cancellation.IsCancellationRequested)
                    {
                        if (ReceiveMessages(out var messages))
                            foreach (var message in messages)
                                OnMessageReceived(message);
                    }
                }

                // Connection stop
                StopConnection();
                _mainThreadContext.Post(state => OnConnectionStatusChanged?.Invoke(false), this);

            }, _cancellation.Token);
        }

        /// <summary>
        /// Stops the middleware and the established connection
        /// </summary>
        public void Stop()
        {
            SendMessage(new StopFromClientMessage());
            StopConnection();
        }

        /// <summary>
        /// Sends a message to the middleware
        /// </summary>
        /// <param name="message">The message</param>
        public void SendMessage(IWeArtMessage message)
        {
            if (IsConnected)
            {
                try
                {
                    _messageSerializer.Serialize(message, out string text);
                    text += _messagesSeparator;
                    _messageSerializer.Serialize(text, out byte[] bytes);
                    _socket.Send(bytes);
                    OnMessage?.Invoke(MessageType.MessageSent, message);
                    OnTextMessage?.Invoke(MessageType.MessageSent, text);
                }
                catch (Exception e)
                {
                    OnError?.Invoke(ErrorType.SendMessageError, e);
                }
            }
        }

        /// <summary>
        /// Called internally to parse eventual incoming messages
        /// </summary>
        /// <param name="messages">Correctly parsed messages</param>
        /// <returns>True if at least one message has been received</returns>
        private bool ReceiveMessages(out IWeArtMessage[] messages)
        {
            if (IsConnected)
            {
                try
                {
                    int numBytes = _socket.Receive(_messageReceivedBuffer);
                    if (numBytes > 0)
                    {
                        _messageSerializer.Deserialize(_messageReceivedBuffer, numBytes, out string bufferText);

                        bufferText = _trailingText + bufferText;
                        int lastSeparatorIndex = bufferText.LastIndexOf(_messagesSeparator);
                        string text = bufferText.Substring(0, lastSeparatorIndex);
                        _trailingText = bufferText.Substring(lastSeparatorIndex + 1);

                        if (string.IsNullOrEmpty(text))
                        {
                            messages = null;
                            return false;
                        }

                        string[] split = text.Split(_messagesSeparator);
                        messages = new IWeArtMessage[split.Length];
                        for (int i = 0; i < messages.Length; i++)
                        {
                            _messageSerializer.Deserialize(split[i], out messages[i]);
                            OnMessage?.Invoke(MessageType.MessageReceived, messages[i]);
                            OnTextMessage?.Invoke(MessageType.MessageReceived, split[i]);
                        }
                        return true;
                    }
                }
                catch (Exception e)
                {
                    if (IsConnected)
                        OnError?.Invoke(ErrorType.ReceiveMessageError, e);
                }
            }

            messages = null;
            return false;
        }

        /// <summary>
        /// Called internally to stop the connection if the middleware requests it
        /// </summary>
        /// <param name="msg">A received message</param>
        private void OnMessageReceived(IWeArtMessage msg)
        {
            if (msg is ExitMessage ||
                msg is DisconnectMessage)
            {
                Stop();
            }
        }

        /// <summary>
        /// Called internally to stop the connection task and the socket
        /// </summary>
        private void StopConnection()
        {
            _cancellation?.Cancel();
            _socket?.Close();
        }
    }
}