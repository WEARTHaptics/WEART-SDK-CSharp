/**
*	WEART - Common TCP Client
*	https://www.weart.it/
*/

using System;
using System.Linq;
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

        private MiddlewareStatusUpdate _lastStatus = new MiddlewareStatusUpdate();

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

        // Calibration Events

        /// <summary>
        /// Called when the calibration procedure starts
        /// </summary>
        public event Action<HandSide> OnCalibrationStart;

        /// <summary>
        /// Called when the calibration procedure ends
        /// </summary>
        public event Action<HandSide> OnCalibrationFinish;

        /// <summary>
        /// Called when the calibration procedure is successful
        /// </summary>
        public event Action<HandSide> OnCalibrationResultSuccess;

        /// <summary>
        /// Called when the calibration procedure faild
        /// </summary>
        public event Action<HandSide> OnCalibrationResultFail;

        /// <summary>
        /// Called when a status update is received from the middleware
        /// </summary>
        public event Action<MiddlewareStatusUpdate> OnMiddlewareStatusUpdate;

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
        public Task Start(TrackingType trackingType = TrackingType.WEART_HAND)
        {
            _cancellation = new CancellationTokenSource();
            return Task.Run(async () =>
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
                        SendMessage(new StartFromClientMessage { TrackingType = trackingType });

                        await Task.Delay(1000);

                        SendMessage(new GetMiddlewareStatusMessage());
                        SendMessage(new GetDevicesStatusMessage());

                        // Message receiving loop
                        while (!_cancellation.IsCancellationRequested)
                        {
                            if (ReceiveMessages(out var messages))
                                foreach (var message in messages)
                                    OnMessageReceived(message);
                        }
                    }
                    catch (Exception e)
                    {
                        // Error handling and connection stop
                        OnError?.Invoke(ErrorType.ConnectionError, e);
                        StopConnection();
                    }
                }

                // Connection stop and reset status
                StopConnection();
                _mainThreadContext.Post(state => OnConnectionStatusChanged?.Invoke(false), this);
                
                _lastStatus = new MiddlewareStatusUpdate();
                OnMiddlewareStatusUpdate?.Invoke(_lastStatus);

            }, _cancellation.Token);
        }

        /// <summary>
        /// Stops the middleware and the established connection
        /// </summary>
        /// <returns>True if the middleware was stopped correctly and the connection closed, false otherwise</returns>
        public async Task<bool> Stop()
        {
            SendMessage(new StopFromClientMessage());
            await SendAndWaitForMessage(new GetMiddlewareStatusMessage(), (MiddlewareStatusMessage msg) => msg.Status == MiddlewareStatus.IDLE, 3000);
            StopConnection();
            return true;
        }

        /// <summary>
        /// Starts the calibration procedure
        /// </summary>
        public void StartCalibration()
        {
            SendMessage(new StartCalibrationMessage());
        }

        /// <summary>
        /// Stops the calibration procedure
        /// </summary>
        public void StopCalibration()
        {
            SendMessage(new StopCalibrationMessage());
        }

        /// <summary>
        /// Asks the middleware to start sending raw data events to the sdk
        /// </summary>
        public void StartRawData()
        {
            SendMessage(new RawDataOnMessage());
        }

        /// <summary>
        /// Tells the middleware to stop sending raw data events
        /// </summary>
        public void StopRawData()
        {
            SendMessage(new RawDataOffMessage());
        }

        /// <summary>
        /// Send the given message to the middleware and waits for a given type of response within the given timeout
        /// </summary>
        /// <typeparam name="T">Type of the message to wait for</typeparam>
        /// <param name="message">Message to send before waiting</param>
        /// <param name="timeoutMs">Time to wait for the given message type</param>
        /// <returns>The received response message</returns>
        /// <exception cref="TimeoutException">Thrown when the correct message is not received withih the given timeout</exception>
        public async Task<T> SendAndWaitForMessage<T>(IWeArtMessage message, int timeoutMs)
        {
            return await SendAndWaitForMessage<T>(message, (T msg) => true, timeoutMs);
        }

        /// <summary>
        /// Send the given message to the middleware and waits for a response (with a given condition applied) within the given timeout
        /// </summary>
        /// <typeparam name="T">Type of the message to wait for</typeparam>
        /// <param name="message">Message to send before waiting</param>
        /// <param name="predicate">The condition the received message must pass to be considered</param>
        /// <param name="timeoutMs">Time to wait for the given message type</param>
        /// <returns>The received response message</returns>
        /// <exception cref="TimeoutException">Thrown when the correct message is not received withih the given timeout</exception>
        public async Task<T> SendAndWaitForMessage<T>(IWeArtMessage message, Func<T, bool> predicate, int timeoutMs)
        {
            // Write Pack and wait for responses (of a certain type) with a given timeout
            CancellationTokenSource source = new CancellationTokenSource();

            T receivedMessage = default(T);
            Action<MessageType, IWeArtMessage> onMessageReceived = (MessageType type, IWeArtMessage msg) =>
            {
                if (type != MessageType.MessageReceived) return;
                if (msg is T castedMessage)
                {
                    if (predicate(castedMessage))
                    {
                        receivedMessage = castedMessage;
                        source.Cancel();
                    }
                }
            };

            OnMessage += onMessageReceived;

            SendMessage(message);

            // Wait to receive message
            try
            {
                await Task.Delay(timeoutMs, source.Token);
                throw new TimeoutException();
            }
            catch (OperationCanceledException)
            {
                // Canceled operation means we received the correct message
            }
            finally
            {
                OnMessage -= onMessageReceived;
            }

            return receivedMessage;
        }

        /// <summary>
        /// Waits for any message with the given type for the given timeout
        /// </summary>
        /// <typeparam name="T">Type of the message to wait</typeparam>
        /// <param name="timeoutMs">Timeout to wait for the correct message</param>
        /// <returns>The received message</returns>
        /// <exception cref="TimeoutException">Thrown when the correct message is not received withih the given timeout</exception>
        public async Task<T> WaitForMessage<T>(int timeoutMs)
        {
            return await WaitForMessage<T>((T msg) => true, timeoutMs);
        }

        /// <summary>
        /// Waits for any message with the given type and condition for the given timeout
        /// </summary>
        /// <typeparam name="T">Type of the message to wait</typeparam>
        /// <param name="predicate">Condition that the message must fullfill to be considered ok</param>
        /// <param name="timeoutMs">Timeout to wait for the correct message</param>
        /// <returns>The received message</returns>
        /// <exception cref="TimeoutException">Thrown when the correct message is not received withih the given timeout</exception>
        public async Task<T> WaitForMessage<T>(Func<T, bool> predicate, int timeoutMs)
        {
            // Write Pack and wait for responses (of a certain type) with a given timeout
            CancellationTokenSource source = new CancellationTokenSource();

            T receivedMessage = default(T);
            Action<MessageType, IWeArtMessage> onMessageReceived = (MessageType type, IWeArtMessage message) =>
            {
                if (type != MessageType.MessageReceived) return;
                if (message is T castedMessage)
                {
                    if (predicate(castedMessage))
                    {
                        receivedMessage = castedMessage;
                        source.Cancel();
                    }
                }
            };

            OnMessage += onMessageReceived;

            // Wait to receive message
            try
            {
                await Task.Delay(timeoutMs, source.Token);
                throw new TimeoutException();
            }
            catch (OperationCanceledException)
            {
                // Canceled operation means we received the correct message
            }
            finally
            {
                OnMessage -= onMessageReceived;
            }

            return receivedMessage;
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
                            if (_messageSerializer.Deserialize(split[i], out messages[i]))
                                ForwardMessage(split[i], messages[i]);
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
        /// Called internally to forward a message to all the related events
        /// </summary>
        /// <param name="messageText">String from which the message was deserialized</param>
        /// <param name="message">Received message</param>
        private void ForwardMessage(string messageText, IWeArtMessage message)
        {
            // Generic forward
            OnMessage?.Invoke(MessageType.MessageReceived, message);
            OnTextMessage?.Invoke(MessageType.MessageReceived, messageText);

            // Manage calibration messages
            if (message is TrackingCalibrationStatus calibrationStatus)
            {
                switch (calibrationStatus.Status)
                {
                    case CalibrationStatus.Calibrating:
                        OnCalibrationStart?.Invoke(calibrationStatus.HandSide);
                        break;
                    case CalibrationStatus.Running:
                        OnCalibrationFinish?.Invoke(calibrationStatus.HandSide);
                        break;
                }
            }
            else if (message is TrackingCalibrationResult calibrationResult)
            {
                if (calibrationResult.Success)
                    OnCalibrationResultSuccess?.Invoke(calibrationResult.HandSide);
                else
                    OnCalibrationResultFail?.Invoke(calibrationResult.HandSide);
            }
            else if (message is MiddlewareStatusMessage mwStatusMessage)
            {
                _lastStatus.Timestamp = mwStatusMessage.Timestamp;
                _lastStatus.Status = mwStatusMessage.Status;
                _lastStatus.Version = mwStatusMessage.Version;
                _lastStatus.StatusCode = mwStatusMessage.StatusCode;
                _lastStatus.ErrorDesc = mwStatusMessage.ErrorDesc;
                _lastStatus.ActuationsEnabled = mwStatusMessage.ActuationsEnabled;

                OnMiddlewareStatusUpdate?.Invoke(_lastStatus);
            }
            else if (message is DevicesStatusMessage devicesStatusMessage)
            {
                _lastStatus.Timestamp = devicesStatusMessage.Timestamp;
                _lastStatus.Devices = devicesStatusMessage.Devices.ToList(); // Clone list

                OnMiddlewareStatusUpdate?.Invoke(_lastStatus);
            }

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