namespace Plugin.BluetoothClassic.Droid
{
    using Android.Bluetooth;
    using Android.Util;
    using Java.Util;
    using Plugin.BluetoothClassic.Abstractions;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    ///<summary>
    /// The class that represents a type to create a short time connections between a current bluetooth adapter and the remote bluetooth device.
    /// It can be used for the short time connections, when required to transmit/recieve a some small portion of the data and then close a connection.
    ///</summary>
    public class BluetoothConnection : IBluetoothConnection
    {
        private readonly UUID SppRecodrUUID = UUID.FromString(BluetoothConstants.SppRecordUUID);

        private readonly string _bluetoothDeviceAddress;
        private BluetoothSocket _socket;

        /// <summary>
        /// The constructor that creates a new instance of the <see cref="BluetoothConnection"/> type.
        /// </summary>
        /// <param name="bluetoothDeviceAddress">The property that represents an address of the remote device to connect to.</param>
        internal BluetoothConnection(string bluetoothDeviceAddress)
        {
            _bluetoothDeviceAddress = bluetoothDeviceAddress;
        }

        /// <summary>
        /// The method that connects to the remote bluetooth device asynchronously.
        /// </summary>
        /// <returns>Returns a <see cref="Task"/> instance.</returns>
        public async Task ConnectAsync()
        {
            if (string.IsNullOrWhiteSpace(_bluetoothDeviceAddress))
            {
                throw new BluetoothConnectionException(
                    $"Can not connect to the remote bluetooth device. Address is not a null or empty.");
            }

            var device = GetRemoteDevice();

            await CreateSocketAndConnectAsync(device);

        }
        private BluetoothDevice GetRemoteDevice()
        {
            var result = Android.Bluetooth.BluetoothAdapter.DefaultAdapter
                .GetRemoteDevice(_bluetoothDeviceAddress);

            if (result == null)
            {
                throw new BluetoothConnectionException(
                    $"Can not get remote bluetooth device with address: \"{_bluetoothDeviceAddress}\".");
            }

            return result;
        }

        private async Task CreateSocketAndConnectAsync(BluetoothDevice device)
        {
            var socket = device.CreateRfcommSocketToServiceRecord(SppRecodrUUID);

            if (socket == null)
            {
                throw new BluetoothConnectionException(
                    $"Can not connect to the remote bluetooth device with address: \"{_bluetoothDeviceAddress}\". Can not create RFCOMM socket.");
            }

            try
            {
                await socket.ConnectAsync();
                _socket = socket;
            }
            catch
            {
                throw new BluetoothConnectionException(
                    $"Can not connect to the remote bluetooth device with address: \"{_bluetoothDeviceAddress}\". Can not connect to the RFCOMM socket.");
            }
        }

        private void ValidateSocket()
        {
            if (_socket == null)
            {
                throw new BluetoothConnectionException("Can not transmit/recive data because connection is not opened. Plase, use \"Task ConnectAsync()\" method before.");
            }
        }

        /// <summary>
        /// The method that transmit data to the remote bluetooth device asynchronously.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="Memory{byte}"/> buffer to transmit.</param>
        /// <param name="cancellationToken">The parameter that represents a <see cref="CancellationToken"/> instance.</param>
        /// <returns>Returns a <see cref="Task"/> instance.</returns>
        public async Task TransmitAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            ValidateSocket();
            try
            {
                await _socket.OutputStream.WriteAsync(buffer, cancellationToken);
            }
            catch(Exception exception)
            {
                throw new BluetoothTransmitException(
                    $"Can not transmit data to the device with address: \"{_bluetoothDeviceAddress}\"",
                    exception);
            }
        }

        /// <summary>
        /// The method that transmit data to the remote bluetooth device asynchronously.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte[]"/> buffer to transmit.</param>
        /// <param name="offset">The parameter that represents a transmit buffer offset.</param>
        /// <param name="count">The parameter that represents a count of bytes to transmit.</param>
        /// <returns>Returns a <see cref="Task"/> instance.</returns>
        public async Task TransmitAsync(byte[] buffer, int offset, int count)
        {
            ValidateSocket();
            try
            {
                await _socket.OutputStream.WriteAsync(buffer, offset, count);
            }
            catch (Exception exception)
            {
                throw new BluetoothTransmitException(
                    $"Can not transmit data to the device with address: \"{_bluetoothDeviceAddress}\"",
                    exception);
            }
        }

        /// <summary>
        /// The method that transmit data to the remote bluetooth device asynchronously.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte[]"/> buffer to transmit.</param>
        /// <param name="offset">The parameter that represents a transmit buffer offset.</param>
        /// <param name="count">The parameter that represents a count of bytes to transmit.</param>
        /// <param name="cancellationToken">The parameter that represents a <see cref="CancellationToken"/> instance.</param>
        /// <returns>Returns a <see cref="Task"/> instance.</returns>
        public async Task TransmitAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            ValidateSocket();
            try
            {
                await _socket.OutputStream.WriteAsync(buffer, offset, count, cancellationToken);
            }
            catch (Exception exception)
            {
                throw new BluetoothTransmitException(
                    $"Can not transmit data to the device with address: \"{_bluetoothDeviceAddress}\"",
                    exception);
            }
        }

        /// <summary>
        /// The property that represents is any data available to recive.
        /// </summary>
        public bool DataAvailable
        {
            get
            {
                ValidateSocket();
                try
                {
                    return _socket.InputStream.IsDataAvailable();
                }
                catch (Exception exception)
                {
                    throw new BluetoothReciveException(
                        $"Can not recive is data available for the device with address: \"{_bluetoothDeviceAddress}\"",
                        exception);
                }
            }
        }

        /// <summary>
        /// The method that recive data from the remote bluetooth device asynchronously.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="Memory{byte}"/> buffer to recive.</param>
        /// <param name="cancellationToken">The parameter that represents a <see cref="CancellationToken"/> instance.</param>
        /// <returns>Returns a <see cref="Task{int}"/> instance with a count of the recieved bytes.</returns>
        public async Task<int> ReciveAsync(Memory<byte> buffer, 
            CancellationToken cancellationToken = default)
        {
            ValidateSocket();
            try
            {
                return await _socket.InputStream.ReadAsync(buffer, cancellationToken);
            }
            catch (Exception exception)
            {
                throw new BluetoothReciveException(
                    $"Can not recive data from the device with address: \"{_bluetoothDeviceAddress}\"",
                    exception);
            }
        }

        /// <summary>
        /// The method that recive data from the remote bluetooth device asynchronously.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte[]"/> buffer to recive.</param>
        /// <param name="offset">The parameter that represents a recive buffer offset.</param>
        /// <param name="count">The parameter that represents a count of bytes to recive.</param>
        /// <returns>Returns a <see cref="Task{int}"/> instance with a count of the recieved bytes.</returns>
        public async Task<int> ReciveAsync(byte[] buffer, int offset, int count)
        {
            ValidateSocket();
            try
            {
                return await _socket.InputStream.ReadAsync(buffer, offset, count);
            }
            catch (Exception exception)
            {
                throw new BluetoothReciveException(
                    $"Can not recive data from the device with address: \"{_bluetoothDeviceAddress}\"",
                    exception);
            }
        }

        /// <summary>
        /// The method that recive data from the remote bluetooth device asynchronously.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte[]"/> buffer to recive.</param>
        /// <param name="offset">The parameter that represents a recive buffer offset.</param>
        /// <param name="count">The parameter that represents a count of bytes to recive.</param>
        /// <param name="cancellationToken">The parameter that represents a <see cref="CancellationToken"/> instance.</param>
        /// <returns>Returns a <see cref="Task{int}"/> instance witch a count of the recived bytes.</returns>
        public async Task<int> ReciveAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateSocket();
            try
            {
                return await _socket.InputStream.ReadAsync(buffer, offset, count, cancellationToken);
            }
            catch (Exception exception)
            {
                throw new BluetoothReciveException(
                    $"Can not recive data from the device with address: \"{_bluetoothDeviceAddress}\"",
                    exception);
            }
        }

        /// <summary>
        /// The mehtod that dispose unmanaged resources used by the <see cref="BluetoothConnection"/> instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
            }
            catch(Exception exception)
            {
                Log.Warn("Dispose::Exception", exception.Message);
            }
        }
    }
}