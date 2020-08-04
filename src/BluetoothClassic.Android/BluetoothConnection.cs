namespace BluetoothClassic.Droid
{
    using Android.Bluetooth;
    using Android.OS;
    using Android.Speech;
    using Android.Util;
    using BluetoothClassic.Base;
    using Java.Util;

    using System;
    using System.Runtime.CompilerServices;
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
            Disconnecting,
            Disconnected,
            Connecting,
            Connected,
            ErrorHappend,
            Disposed
        }

        private ConnectionState _connectionState;

        private const string CurrentDeviceAddress = "00001101-0000-1000-8000-00805F9B34FB";
        private readonly UUID CurrentDeviceAddressAsUUID = UUID.FromString(CurrentDeviceAddress);

        private readonly string _remoteDeviceAddress;

        private BluetoothSocket _socket;

        private CancellationTokenSource _managerCancellationTokenSource;
        private CancellationTokenSource _socketCancellationTokenSource;


        /// <summary>
        /// The <see cref="BluetoothConnection"/> type constructor.
        /// </summary>
        /// <param name="remoteDeviceAddress">The parameter that represents an address of the remote device to connect to.</param>
        public BluetoothConnection(string remoteDeviceAddress)
        {
            this._remoteDeviceAddress = remoteDeviceAddress;
            SetInitializing();
        }

        private bool Disconnected => _connectionState == ConnectionState.Disconnected;
        private bool Connecting => _connectionState == ConnectionState.Connecting;
        private bool Connected => _connectionState == ConnectionState.Connected;

        private bool Initializing => _connectionState == ConnectionState.Initializing;

        private bool Disposed => _connectionState == ConnectionState.Disposed;

        private bool Disconnecting => _connectionState == ConnectionState.Disconnecting;

        private bool ErrorHappend => _connectionState == ConnectionState.ErrorHappend;

        


        private void SetInitializing()
        {
            _connectionState = ConnectionState.Initializing;
        }

        private void SetConnected()
        {
            _connectionState = ConnectionState.Connected;
        }
        private void SetDisconnected()
        {
            _connectionState = ConnectionState.Disconnected;
        }

        private void SetErrorHappend()
        {
            _connectionState = ConnectionState.ErrorHappend;
        }


        private void SetDisconnecting()
        {
            _connectionState = ConnectionState.Disconnecting;
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
                        StartManager();
                        await ConnectToSocketAndListenAsync();
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
            _managerCancellationTokenSource = new CancellationTokenSource();

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
                            string message = exception.Message;
                            Log.Warn("StartManager::Task::Exception", message);
                        }
                    }
                }
            }, _managerCancellationTokenSource.Token);
        }

        private async Task ConnectToSocketAndListenAsync()
        {
            try
            {
                await CreateSocket();
                SetConnected();
                StartListener();
            }
            catch (Exception exception)
            {
                SetErrorHappend();
                string message = exception.Message;
                Log.Warn("ConnectInternalAsync::Exception", message);
                // throw new BluetoothConnectionException($"Can not connect to the remote device with address: \"{_remoteDeviceAddress}\".", exception);
            }
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
            _socketCancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (Connected)
                {
                    try
                    {
                        byte[] buffer = new byte[BufferSizeDefault];

                        int recivedBytesCount = await _socket.InputStream.ReadAsync(buffer, BufferOffsetZero, BufferSizeDefault,
                            _socketCancellationTokenSource.Token);

                        try
                        {
                            RaiseRecived(new Memory<byte>(buffer, BufferOffsetZero, recivedBytesCount));
                        }
                        catch(Exception exception)
                        {
                            Log.Warn("StartListener::Task::RaiseRecived::Exception", exception.Message);
                        }
                    }
                    catch(InsufficientMemoryException insufficientMemoryException)
                    {
                        SetErrorHappend();
                        Log.Warn("StartListener::Task::InsufficientMemoryException", insufficientMemoryException.Message);
                        RaiseError(insufficientMemoryException);
                        //throw;
                    }
                    catch(OutOfMemoryException outOfMemoryException)
                    {
                        SetErrorHappend();
                        Log.Warn("StartListener::Task::OutOfMemoryException", outOfMemoryException.Message);
                        RaiseError(outOfMemoryException);
                        //throw;
                    }
                    catch (Java.IO.IOException ioException)
                    {
                        SetErrorHappend();
                        string message = ioException.Message;
                        Log.Warn("StartListener::Task::BluetoothSocket.InputStream.ReadAsync::IOException::Error", message);
                        //await ReconnectAsync();
                    }
                    catch(Exception exception)
                    {
                        SetErrorHappend();
                        Log.Warn("StartListener::Task::Exception", exception.Message);
                        RaiseError(exception);
                        //throw;
                    }
                }
            }, _socketCancellationTokenSource.Token);
        }

        /// <summary>
        /// The method that sends data through the current bluetooth connection.
        /// </summary>
        /// <param name="buffer">The <see cref="Memory{byte}"/> with data to send.</param>
        /// <returns>Returns the <see cref="Task"/> instance.</returns>
        public async Task SendAsync(Memory<byte> buffer)
        {
            byte[] bufferAsByteArray = buffer.ToArray();
            try
            {
                await _socket?.OutputStream.WriteAsync(bufferAsByteArray,
                    BufferOffsetZero, buffer.Length, _socketCancellationTokenSource.Token);
            }
            catch (Java.IO.IOException ioException)
            {
                SetErrorHappend();
                string message = ioException.Message;
                Log.Warn("SendAsync::IOException", message);
                //await ReconnectAsync();
            }
        }

        private async Task ReconnectAsync()
        {
            Disconnect();
            await ConnectToSocketAndListenAsync();
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
                    _managerCancellationTokenSource.Cancel();
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

        private void CancelSocketCancellationTokenSource()
        {
            if (_socketCancellationTokenSource != null 
                && !_socketCancellationTokenSource.IsCancellationRequested)
            {
                _socketCancellationTokenSource.Cancel();
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
        /// The event that will be raised on a new data portion recived throguh the current bluetooth connection.
        /// </summary>
        public event Recived OnRecived;

        private void RaiseRecived(Memory<byte> buffer)
        {
            OnRecived?.Invoke(this, new RecivedEventArgs(buffer));
        }

        /// <summary>
        /// The event that will be raised on a send/recive threads errors.
        /// </summary>
        public event Error OnError;

        private void RaiseError(Exception exception)
        {
            OnError?.Invoke(this, new ThreadExceptionEventArgs(exception));
        }
    }
}