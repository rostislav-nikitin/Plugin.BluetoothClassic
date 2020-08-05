namespace BluetoothClassic.Droid
{
    using Android.Bluetooth;
    using Android.Util;
    using BluetoothClassic.Base;
    using Java.Util;

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using AndroidBluetoothAdapter = Android.Bluetooth.BluetoothAdapter;

    /// <summary>
    /// The class that represents a <see cref="BluetoothConnection"/> type that implements a <see cref="IBluetoothConnection"/> interface.
    /// Usage: to create a connection for send/recive data over bluetooth protocol.
    /// </summary>
    public sealed class BluetoothConnection : IBluetoothConnection
    {
        //private volatile static object _syncRoot = new object();
        private ReaderWriterLock rwLock = new ReaderWriterLock();

        private enum ConnectionState
        {
            Initializing,
            Connecting,
            Connected,
            ErrorHappend,
            Disconnecting,
            Disconnected,
            Disposed
        }

        private ConnectionState _connectionState;

        private const string CurrentDeviceAddress = "00001101-0000-1000-8000-00805F9B34FB";
        private readonly UUID CurrentDeviceAddressAsUUID = UUID.FromString(CurrentDeviceAddress);

        private readonly string _remoteDeviceAddress;

        private BluetoothSocket _socket;

        private CancellationTokenSource _managerCancellationTokenSource;
        private CancellationTokenSource _socketCancellationTokenSource;

        private readonly ConcurrentQueue<Memory<byte>> _sendQueue;

        /// <summary>
        /// The <see cref="BluetoothConnection"/> type constructor.
        /// </summary>
        /// <param name="remoteDeviceAddress">The parameter that represents an address of the remote device to connect to.</param>
        public BluetoothConnection(string remoteDeviceAddress)
        {
            _remoteDeviceAddress = remoteDeviceAddress;
            _sendQueue = new ConcurrentQueue<Memory<byte>>();

            SetInitializing();
        }

        private bool Connected => _connectionState == ConnectionState.Connected;

        private bool Initializing => _connectionState == ConnectionState.Initializing;

        private bool ErrorHappend => _connectionState == ConnectionState.ErrorHappend;

        private bool Disposed => _connectionState == ConnectionState.Disposed;


        private void SetInitializing()
        {
            _connectionState = ConnectionState.Initializing;
        }

        private void SetConnecting()
        {
            _connectionState = ConnectionState.Connecting;
        }

        private void SetConnected()
        {
            _connectionState = ConnectionState.Connected;
        }

        private void SetErrorHappend()
        {
            _connectionState = ConnectionState.ErrorHappend;
        }

        private void SetDisconnecting()
        {
            _connectionState = ConnectionState.Disconnecting;
        }

        private void SetDisconnected()
        {
            _connectionState = ConnectionState.Disconnected;
        }

        private void SetDisposed()
        {
            _connectionState = ConnectionState.Disposed;
        }

        /// <summary>
        /// The method that connectes current device to the remote device through bluetooth protocol.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/> instance.</returns>
        public async Task ConnectAsync()
        {
            if (Initializing || Disposed)
            {
                rwLock.AcquireWriterLock(TimeSpan.Zero);
                try
                {
                    if (Initializing || Disposed)
                    {
                        SetConnecting();
                        StartManager();
                        await ConnectToSocketAndStartTransiverAsync();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
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

                    if (ErrorHappend)
                    {
                        try
                        {
                            await ReconnectAsync();
                        }
                        catch(Exception exception)
                        {
                            HandleException("StartManager::Task::Exception", exception);
                        }
                    }
                }
            }, _managerCancellationTokenSource.Token);
        }

        private async Task ConnectToSocketAndStartTransiverAsync()
        {
            try
            {
                await CreateSocket();
                SetConnected();
                StartTransiver();
            }
            catch (Exception exception)
            {
                HandleException("ConnectInternalAsync::Exception", exception);
            }
        }

        private void StartTransiver()
        {
            StartSocketCancellationTokenSource();
            StartListener();
            StartSender();
        }

        private async Task CreateSocket()
        {
            var remoteDevice = AndroidBluetoothAdapter.DefaultAdapter
                .GetRemoteDevice(this._remoteDeviceAddress);

            if(remoteDevice == null)
            {
                throw new BluetoothConnectionException($"Can not get remote device with address: \"{_remoteDeviceAddress}\"");
            }

            _socket = remoteDevice.CreateRfcommSocketToServiceRecord(CurrentDeviceAddressAsUUID);

            if (_socket == null)
            {
                throw new BluetoothConnectionException($"Can not get socket fot device with address: \"{_remoteDeviceAddress}\"");
            }

            await _socket.ConnectAsync();
        }

        const int BufferOffsetZero = 0;
        const int BufferSizeDefault = 32;

        private void StartListener()
        {
            Task.Run(async () =>
            {
                while (Connected)
                {
                    await ReciveAsync();
                }
            }, _socketCancellationTokenSource.Token);
        }

        private async Task ReciveAsync()
        {
            byte[] buffer = new byte[BufferSizeDefault];

            try
            {
                int recivedBytesCount = await _socket.InputStream.ReadAsync(buffer, BufferOffsetZero, BufferSizeDefault,
                    _socketCancellationTokenSource.Token);
                try
                {
                    RaiseRecivedEvent(new Memory<byte>(buffer, BufferOffsetZero, recivedBytesCount));
                }
                catch (Exception exception)
                {
                    Log.Warn("ReciveAsync::Task::RaiseRecived::Exception", exception.Message);
                }
            }
            catch (Java.IO.IOException ioException)
            {
                HandleException("ReciveAsync::Task::BluetoothSocket.InputStream.ReadAsync::Java.IO.IOException::Error",
                    new BluetoothReciveException(ioException.Message, ioException));
            }
            catch (Exception exception)
            {
                HandleException("ReciveAsync::Task::Exception", exception);
            }
        }

        private void StartSender()
        {
            TimeSpan TimeoutDefault = new TimeSpan(0, 0, 0, 0, 1);

            Task.Run(async () =>
            {
                while(Connected)
                {
                    Thread.Sleep(TimeoutDefault);
                    await SendAsync();
                }
            }, _socketCancellationTokenSource.Token);
        }


        /// <summary>
        /// The method that sends data through the current bluetooth connection.
        /// </summary>
        /// <param name="buffer">The <see cref="Memory{byte}"/> with data to send.</param>
        public void Send(Memory<byte> buffer)
        {
            _sendQueue.Enqueue(buffer);
        }

        private async Task SendAsync()
        {
            Memory<byte> buffer;
            if (_sendQueue.TryPeek(out buffer))
            {
                byte[] bufferAsByteArray = buffer.ToArray();

                try
                {
                    await _socket?.OutputStream.WriteAsync(bufferAsByteArray,
                        BufferOffsetZero, buffer.Length, _socketCancellationTokenSource.Token);

                    _sendQueue.TryDequeue(out buffer);

                    try
                    {
                        RaiseSentEnvet(buffer);
                    }
                    catch(Exception exception)
                    {
                        string message = exception.Message;
                        Log.Warn("SendAsync::Exception", message);
                    }
                }
                catch (Java.IO.IOException ioException)
                {
                    HandleException("SendAsync::Java.IO.IOException",
                        new BluetoothSendException(ioException.Message, ioException, buffer));
                }
                catch (Exception exception)
                {
                    HandleException("SendAsync::Exception", exception);
                }
            }
        }

        private async Task ReconnectAsync()
        {
            Disconnect();
            await ConnectToSocketAndStartTransiverAsync();
        }

        /// <summary>
        /// The method that dispose used non-managed resources.
        /// </summary>
        public void Dispose()
        {
            if (!Disposed)
            {
                rwLock.AcquireWriterLock(TimeSpan.Zero);

                try
                {
                    Disconnect();
                    CancelManagerCancellationTokenSource();
                    ResetEventHandlers();
                    SetDisposed();
                }
                catch (Exception exception)
                {
                    string message = exception.Message;
                    Log.Warn("Dispose::Exception", message);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }
        }

        private void ResetEventHandlers()
        {
            OnSent = null;
            OnRecived = null;
            OnError = null;
        }

        /// <summary>
        /// The method that disconnects the current device from the remote device.
        /// </summary>
        private void Disconnect()
        {
            SetDisconnecting();
            CancelSocketCancellationTokenSource();
            CloseSocket();
            SetDisconnected();
        }

        private void StartSocketCancellationTokenSource()
        {
            _socketCancellationTokenSource = new CancellationTokenSource();
        }

        private void CancelSocketCancellationTokenSource()
        {
            if (_socketCancellationTokenSource != null 
                && !_socketCancellationTokenSource.IsCancellationRequested)
            {
                _socketCancellationTokenSource.Cancel();
            }
        }

        private void StartManagerCancellationTokenSource()
        {
            _managerCancellationTokenSource = new CancellationTokenSource();
        }

        private void CancelManagerCancellationTokenSource()
        {
            if (_managerCancellationTokenSource != null
                && !_managerCancellationTokenSource.IsCancellationRequested)
            {
                _managerCancellationTokenSource.Cancel();
            }
        }

        private void CloseSocket()
        {
            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                    _socket = null;
                }
                catch(Exception exception)
                {
                    Log.Warn("CloseSocket::Exception", exception.Message);
                }
            }
        }

        /// <summary>
        /// The event that will be raised on a buffer sent throguh the current bluetooth connection.
        /// </summary>

        public event Sent OnSent;

        private void RaiseSentEnvet(Memory<byte> buffer)
        {
            OnSent?.Invoke(this, new SentEventArgs(buffer));
        }

        /// <summary>
        /// The event that will be raised on a buffer recived throguh the current bluetooth connection.
        /// </summary>
        public event Recived OnRecived;

        private void RaiseRecivedEvent(Memory<byte> buffer)
        {
            OnRecived?.Invoke(this, new RecivedEventArgs(buffer));
        }

        /// <summary>
        /// The event that will be raised on a transmitter threads errors.
        /// </summary>
        public event Error OnError;

        private void RaiseErrorEvent(Exception exception)
        {
            OnError?.Invoke(this, new ThreadExceptionEventArgs(exception));
        }

        private void HandleException(string tag, Exception exception)
        {
            SetErrorHappend();
            string message = exception.Message;
            Log.Warn(tag, message);
            RaiseErrorEvent(exception);
        }
    }
}