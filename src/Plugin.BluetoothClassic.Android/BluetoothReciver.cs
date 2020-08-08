namespace Plugin.BluetoothClassic.Droid
{
    using Android.Bluetooth;
    using Plugin.BluetoothClassic.Abstractions;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BluetoothReciver : BluetoothDataTransferUnitBase
    {
        private const int ReciverBufferSizeDefault = BufferSizeDefault;
        public BluetoothReciver(
            ConcurrentValue<ConnectionState> connectionStateWrapper,
            ConcurrentValue<BluetoothSocket> bluetoothSocketWrapper, 
            CancellationToken cancellationToken = default,
            int bufferSize = ReciverBufferSizeDefault) : base(
                connectionStateWrapper, bluetoothSocketWrapper, cancellationToken, bufferSize)
        {
        }

        protected override void StartUnitThread()
        {
            Task.Run(async () =>
            {
                while (ConnectionState != ConnectionState.Disposing
                    && ConnectionState != ConnectionState.Disposed)
                {
                    if (ConnectionState == ConnectionState.Connected)
                    {
                        await ReciveAsync();
                    }
                }
            }, _cancellationToken);
        }

        private async Task ReciveAsync()
        {
            byte[] buffer = new byte[BufferSizeDefault];

            try
            {
                int recivedBytesCount = await BluetoothSocket.InputStream.ReadAsync(buffer, 
                    BufferOffsetDefault, _bufferSize, _cancellationToken);
                try
                {
                    RaiseRecivedEvent(new Memory<byte>(buffer, BufferOffsetZero, recivedBytesCount));
                }
                catch (Exception exception)
                {
                    LogWarning("ReciveAsync::RaiseRecived::Exception", exception);
                }
            }
            catch (Java.IO.IOException ioException)
            {
                HandleException("ReciveAsync::BluetoothSocket.InputStream.ReadAsync::Java.IO.IOException",
                    new BluetoothReciveException(ioException.Message, ioException));
            }
            catch (Exception exception)
            {
                HandleException("ReciveAsync::Exception", exception);
            }
        }

        /// <summary>
        /// The event that raises on a data recived.
        /// </summary>
        public event Recived OnRecived;

        private void RaiseRecivedEvent(Memory<byte> buffer)
        {
            OnRecived?.Invoke(this, new RecivedEventArgs(buffer));
        }

    }
}