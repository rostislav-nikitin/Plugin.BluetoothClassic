namespace Plugin.BluetoothClassic.Droid
{
    using Android.Bluetooth;
    using Plugin.BluetoothClassic.Abstractions;
    using Java.Util;

    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Xamarin.Forms.Internals;
    using NativeBluetooth = Android.Bluetooth;

    /// <summary>
    /// The class that represents a <see cref="BluetoothManagedConnection"/> type that implements a <see cref="IBluetoothManagedConnection"/> interface.
    /// This type represents a managed connection between a current bluetooth adapter and the remote bluetooth device.
    /// It can be used for the long time connections. 
    /// Managed means that it contains the internal manager that control a connection state, reconnect it if required and do other tasks at the background.
    /// </summary>
    public sealed class BluetoothManagedConnection : IBluetoothManagedConnection
    {
        private const int TimoutInfinity = -1;
        private ReaderWriterLock _rwLock = new ReaderWriterLock();

        private readonly UUID SppRecordUUID = UUID.FromString(BluetoothConstants.SppRecordUUID);

        private readonly ConnectionType _connectionType;
        private readonly string _remoteDeviceAddress;
        private readonly ConcurrentValue<ConnectionState> _connectionStateWrapper;
        private readonly ConcurrentValue<BluetoothSocket> _socketWrapper;
        private readonly ConcurrentQueue<Memory<byte>> _transmitQueue;
        private CancellationTokenSource _managerCancellationTokenSource;
        private CancellationTokenSource _dataTransferUnitsCancellationTokenSource;
        private BluetoothReciver _reciver;
        private BluetoothTransmitter _transmitter;

        /// <summary>
        /// The <see cref="BluetoothManagedConnection"/> type constructor.
        /// </summary>
        /// <param name="remoteDeviceAddress">The parameter that represents an address of the remote device to connect to.</param>
        /// <param name="connectionType">The parameter that represents a bletooth connection type.</param>
        internal BluetoothManagedConnection(string remoteDeviceAddress, 
            ConnectionType connectionType = ConnectionType.Transiver)
        {
            _remoteDeviceAddress = remoteDeviceAddress;
            _connectionType = connectionType;

            _connectionStateWrapper = new ConcurrentValue<ConnectionState>(ConnectionState.Created);
            _socketWrapper = new ConcurrentValue<BluetoothSocket>();
            _transmitQueue = new ConcurrentQueue<Memory<byte>>();
        }

        /// <summary>
        /// The property that represents a current connection state.
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                return _connectionStateWrapper.Value;
            }
            private set
            {
                _connectionStateWrapper.Value = value;
                RaiseConnectionStateChangedEvent();
            }
        }

        #region Connect/Reconnect
        /// <summary>
        /// The methods that begins connection process. In not creates connection immediately, notify connection manager to create it as soon as possible.
        /// Use <see cref="Recived"/> event to listen to any state chagnes and particullary for the <see cref="ConnectionState.Connected"/> state.
        /// </summary>
        public void Connect()
        {
            _rwLock.AcquireWriterLock(TimoutInfinity);

            try
            {
                if (!(ConnectionState == ConnectionState.Created
                    || ConnectionState == ConnectionState.Disposed))
                {
                    throw new BluetoothConnectionException($"Can not connect because of current connection state is: \"{ConnectionState}\". Can connect only from the next states: \"{ConnectionState.Created}\", \"{ConnectionState.Disposed}\".");
                }

                ConnectionState = ConnectionState.Initializing;

                StartManager();
                StartDataTransferUnits();
            }
            catch
            {
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        private void StartManager()
        {
            TimeSpan TimeoutDefault = new TimeSpan(0, 0, 0, 0, 100);

            StartManagerCancellationTokenSource();

            Task.Run(async () =>
            {
                while (true)
                {
                    Thread.Sleep(TimeoutDefault);
                    ConnectionState state = ConnectionState;
                    try
                    {
                        if(ConnectionState == ConnectionState.Initializing)
                        {
                            await ConnectAsync();
                        }
                        if (ConnectionState == ConnectionState.ErrorHappend)
                        {
                            await ReconnectAsync();
                        }
                    }
                    catch (Exception exception)
                    {
                        HandleException("StartManager::Task::Exception", exception);
                    }
                }
            }, _managerCancellationTokenSource.Token);
        }

        private void StartManagerCancellationTokenSource()
        {
            _managerCancellationTokenSource = new CancellationTokenSource();
        }

        private async Task ConnectAsync()
        {
            ConnectionState = ConnectionState.Connecting;
            await CreateSocket();
            ConnectionState = ConnectionState.Connected;
        }

        private async Task ReconnectAsync()
        {
            ConnectionState = ConnectionState.Reconnecting;
            await RecreateSocket();
            ConnectionState = ConnectionState.Connected;
        }

        private async Task RecreateSocket()
        {
            CloseSocket();
            await CreateSocket();
        }

        private async Task CreateSocket()
        {
            var remoteDevice = GetRemoteDevice();

            var socket = await CreateSocketAndConnect(remoteDevice);

            _socketWrapper.Value = socket;
        }

        private BluetoothDevice GetRemoteDevice()
        {
            var result = NativeBluetooth.BluetoothAdapter.DefaultAdapter
                .GetRemoteDevice(_remoteDeviceAddress);

            if (result == null)
            {
                throw new BluetoothConnectionException($"Can not get remote device with address: \"{_remoteDeviceAddress}\"");
            }

            return result;
        }

        private async Task<BluetoothSocket> CreateSocketAndConnect(BluetoothDevice remoteDevice)
        {
            BluetoothSocket result = remoteDevice.CreateRfcommSocketToServiceRecord(
                SppRecordUUID);

            if (result == null)
            {
                throw new BluetoothConnectionException($"Can not get socket fot device with address: \"{_remoteDeviceAddress}\"");
            }

            await result.ConnectAsync();

            return result;
        }

        private void StartDataTransferUnits()
        {
            StartDataTransferUnitsCancellationTokenSource();
            CheckConnectionTypeAndStartReciver();
            CheckConnectionTypeAndStartTransmitter();
        }

        private void StartDataTransferUnitsCancellationTokenSource()
        {
            _dataTransferUnitsCancellationTokenSource = new CancellationTokenSource();
        }

        private void CheckConnectionTypeAndStartReciver()
        {
            if (_connectionType == ConnectionType.Reciver
                || _connectionType == ConnectionType.Transiver)
            {
                StartReciver();
            }
        }

        private void StartReciver()
        {
            _reciver = new BluetoothReciver(
                _connectionStateWrapper,
                _socketWrapper,
                _dataTransferUnitsCancellationTokenSource.Token);

            _reciver.OnRecived += _reciver_OnRecived;
            _reciver.OnError += _reciver_OnError;
        }

        private void _reciver_OnRecived(object sender, RecivedEventArgs recivedEventArgs)
        {
            RaiseRecivedEvent(recivedEventArgs.Buffer);
        }

        private void _reciver_OnError(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            HandlerDataTransferUnitError(sender, threadExceptionEventArgs);
        }

        private void CheckConnectionTypeAndStartTransmitter()
        {
            if (_connectionType == ConnectionType.Transmitter
                || _connectionType == ConnectionType.Transiver)
            {
                StartTransmitter();
            }
        }

        private void StartTransmitter()
        {
            _transmitter = new BluetoothTransmitter(
                _connectionStateWrapper,
                _socketWrapper, 
                _transmitQueue, 
                _dataTransferUnitsCancellationTokenSource.Token);

            _transmitter.OnTransmitted += _transmitter_OnTransmitted;
            _transmitter.OnError += _transmitter_OnError;
        }

        private void _transmitter_OnTransmitted(object sender, TransmittedEventArgs recivedEventArgs)
        {
            RaiseTransmittedEvent(recivedEventArgs.Buffer);
        }

        private void _transmitter_OnError(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            HandlerDataTransferUnitError(sender, threadExceptionEventArgs);
        }

        private void HandlerDataTransferUnitError(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            if (threadExceptionEventArgs.Exception is BluetoothDataTransferUnitException
                && ConnectionState != ConnectionState.ErrorHappend)
            {
                HandleException("HandlerDataTransferUnitError", threadExceptionEventArgs.Exception, false);
            }
        }

        #endregion

        #region Transmit

        /// <summary>
        /// The method that do not transmits a data immediately but adds it into the transmit queue to be transmitted on a connection will be in the <see cref="ConnectionState.Connected"/> state.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte{int}"/> with data to transmit.</param>
        public void Transmit(Memory<byte> buffer)
        {
            _transmitQueue.Enqueue(buffer);
        }


        /// <summary>
        /// The method that do not transmits a data immediately but adds it into the transmit queue to be transmitted on a connection will be in the <see cref="ConnectionState.Connected"/> state.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte{int}"/> with a data to transmit.</param>
        /// <param name="offset">The parameter that represents a transmit buffer offset.</param>
        /// <param name="count">The parameter that represents a count of the bytes to transmit.</param>
        public void Transmit(byte[] buffer, int offset, int count)
        {
            _transmitQueue.Enqueue(new Memory<byte>(buffer, offset, count));
        }

        #endregion

        #region Dispose/Disconnect
        /// <summary>
        /// The method that dispose non-managed resources.
        /// </summary>
        public void Dispose()
        {
            _rwLock.AcquireWriterLock(TimoutInfinity);

            try
            {
                if (ConnectionState != ConnectionState.Disposed
                    && ConnectionState != ConnectionState.Disposing)
                {
                    ConnectionState = ConnectionState.Disposing;
                    StopDataTransferUnits();
                    Disconnect();
                    CancelManagerCancellationTokenSource();
                    ResetEventHandlers();
                    ConnectionState = ConnectionState.Disposed;
                }
            }
            catch (Exception exception)
            {
                LogWarning("Dispose::Exception", exception);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }

        }

        private void StopDataTransferUnits()
        {
            CancelDataTransferUnitsCancellationTokenSource();
            CheckConnectionTypeAndStopReciver();
            CheckConnectionTypeAndStopTransmitter();
        }

        private void CancelDataTransferUnitsCancellationTokenSource()
        {
            if (_dataTransferUnitsCancellationTokenSource != null
                && !_dataTransferUnitsCancellationTokenSource.IsCancellationRequested)
            {
                _dataTransferUnitsCancellationTokenSource.Cancel();
            }
        }

        private void CheckConnectionTypeAndStopReciver()
        {
            if ((_connectionType == ConnectionType.Reciver
                || _connectionType == ConnectionType.Transiver)
                && _reciver != null)
            {
                _reciver.OnError -= _reciver_OnError;
                _reciver.OnRecived -= _reciver_OnRecived;
            }
        }

        private void CheckConnectionTypeAndStopTransmitter()
        {
            if ((_connectionType == ConnectionType.Transmitter
                || _connectionType == ConnectionType.Transiver)
                && _transmitter != null)
            {
                _transmitter.OnError -= _transmitter_OnError;
                _transmitter.OnTransmitted -= _transmitter_OnTransmitted;
            }
        }

        /// <summary>
        /// The method that disconnects the current device from the remote device.
        /// </summary>
        private void Disconnect()
        {
            ConnectionState = ConnectionState.Disconnecting;
            CloseSocket();
            ConnectionState = ConnectionState.Disconnected;
        }

        private void CloseSocket()
        {
            try
            {
                _socketWrapper.Value?.Close();
                _socketWrapper.Value = null;
            }
            catch (Exception exception)
            {
                LogWarning("CloseSocket::Exception", exception);
            }
        }

        private void CancelManagerCancellationTokenSource()
        {
            if (_managerCancellationTokenSource != null
                && !_managerCancellationTokenSource.IsCancellationRequested)
            {
                _managerCancellationTokenSource.Cancel();
            }
        }

        private void ResetEventHandlers()
        {
            OnTransmitted = null;
            OnRecived = null;
            OnError = null;
        }

        #endregion

        #region Events

        /// <summary>
        /// The event that raises on a connection state changed.
        /// </summary>
        public event StateChanged OnStateChanged;
        private void RaiseConnectionStateChangedEvent()
        {
            OnStateChanged?.Invoke(this, new StateChangedEventArgs(ConnectionState));
        }

        /// <summary>
        /// The event that raises on a data transmitted.
        /// </summary>
        public event Transmitted OnTransmitted;
        private void RaiseTransmittedEvent(Memory<byte> buffer)
        {
            OnTransmitted?.Invoke(this, new TransmittedEventArgs(buffer));
        }

        /// <summary>
        /// The event that raises on a data recived.
        /// </summary>
        public event Recived OnRecived;
        private void RaiseRecivedEvent(Memory<byte> buffer)
        {
            OnRecived?.Invoke(this, new RecivedEventArgs(buffer));
        }

        /// <summary>
        /// The event tht raises on a connection and data transfer errors.
        /// </summary>
        public event Error OnError;
        internal void RaiseErrorEvent(Exception exception)
        {
            OnError?.Invoke(this, new ThreadExceptionEventArgs(exception));
        }


        #endregion

        #region Handlers/Log

        private void HandleException(string category, Exception exception, bool logging = true)
        {
            ConnectionState = ConnectionState.ErrorHappend;
            if (logging)
            {
                LogWarning(category, exception);
            }
            RaiseErrorEvent(exception);
        }

        private void LogWarning(string category, Exception exception)
        {
            string message = exception.Message;
            LogWarning(category, message);
        }

        private void LogWarning(string category, string message)
        {
            Log.Warning(category, message);
        }

        #endregion
    }
}