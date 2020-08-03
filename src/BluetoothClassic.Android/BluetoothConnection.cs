namespace BluetoothClassic.Droid
{
    using Android.Bluetooth;
    using BluetoothClassic.Base;
    using Java.Util;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using AndroidBluetoothAdapter = Android.Bluetooth.BluetoothAdapter;

    /// <summary>
    /// The class that represents a <see cref="BluetoothConnection"/> type that implements a <see cref="IBluetoothConnection"/> interface.
    /// Usage: to create a connection for send/recive data over bluetooth protocol.
    /// </summary>
    public sealed class BluetoothConnection : IBluetoothConnection
    {
        private BluetoothSocket _socket;

        private const string CurrentDeviceAddress = "00001101-0000-1000-8000-00805F9B34FB";
        private readonly UUID CurrentDeviceAddressAsUUID = UUID.FromString(CurrentDeviceAddress);

        private readonly string _remoteDeviceAddress;

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// The <see cref="BluetoothConnection"/> type constructor.
        /// </summary>
        /// <param name="remoteDeviceAddress">The parameter that represents an address of the remote device to connect to.</param>
        public BluetoothConnection(string remoteDeviceAddress)
        {
            this._remoteDeviceAddress = remoteDeviceAddress;
        }

        /// <summary>
        /// The property that represents does current device connected to the remote device or not.
        /// </summary>
        public bool Connected => _socket?.IsConnected == true;

        /// <summary>
        /// The method that connectes current device to the remote device through bluetooth protocol.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/> instance.</returns>
        public async Task ConnectAsync()
        {
            try
            {
                if(Connected)
                {
                    throw new BluetoothConnectionException($"Alreay connected to the remote device with address: \"{_remoteDeviceAddress}\". Please, used the \"Disconnect\" method to disconnect. ");
                }

                this._cancellationTokenSource = new CancellationTokenSource();
                await CreateSocket();
            }
            catch (Exception exception)
            {
                throw new BluetoothConnectionException($"Can not connect to the remote device with address: \"{_remoteDeviceAddress}\".", exception);
            }

        }

        private async Task CreateSocket()
        {
            var remoteDevice = AndroidBluetoothAdapter.DefaultAdapter
                .GetRemoteDevice(this._remoteDeviceAddress);

            try
            {
                _socket = remoteDevice.CreateRfcommSocketToServiceRecord(CurrentDeviceAddressAsUUID);

                await _socket.ConnectAsync();

                Task.Run(async () =>
                {
                    while (true)
                    {
                        //Memory<byte> buffer = new Memory<byte>();
                        const int OffsetZero = 0;
                        const int BufferSizeDefault = 16;
                        byte[] buffer = new byte[BufferSizeDefault];
                        int recivedBytesCount = await _socket.InputStream.ReadAsync(buffer, OffsetZero, BufferSizeDefault, 
                            _cancellationTokenSource.Token);
                        RaiseRecived(new Memory<byte>(buffer, OffsetZero, recivedBytesCount));
                    }

                }, _cancellationTokenSource.Token);
            }
            catch(Java.IO.IOException exception)
            {
                throw new BluetoothConnectionException($"Can not create socket for the remote device with address: \"{_remoteDeviceAddress}\".", exception);
            }
            catch
            {
                throw;
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
        /// The method that disconnects the current device from the remote device.
        /// </summary>
        public void Disconnect()
        {
            CancelCancellationTokenSource();
            CloseSocket();
        }

        /// <summary>
        /// The method that sends data through the current bluetooth connection.
        /// </summary>
        /// <param name="buffer">The <see cref="Memory{byte}"/> with data to send.</param>
        /// <returns>Returns the <see cref="Task"/> instance.</returns>
        public async Task SendAsync(Memory<byte> buffer)
        {
            if(!Connected)
            {
                throw new BluetoothConnectionException($"You do not connected to the device with address: \"{_remoteDeviceAddress}\". Please, use the \"ConnectAsync\" method to connect.");
            }

            await _socket.OutputStream.WriteAsync(buffer, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// The method that dispose used non-managed resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();

        }

        private void CancelCancellationTokenSource()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private void CloseSocket()
        {
            if (Connected)
            {
                _socket.Close();
            }
        }

    }
}