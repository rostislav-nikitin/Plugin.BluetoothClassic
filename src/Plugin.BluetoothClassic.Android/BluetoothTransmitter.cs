namespace Plugin.BluetoothClassic.Droid
{
    using Android.Bluetooth;
    using Plugin.BluetoothClassic.Abstractions;

    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BluetoothTransmitter : BluetoothDataTransferUnitBase
    {
        private const int TransmitterBufferSizeDefault = BufferSizeDefault;

        private readonly ConcurrentQueue<Memory<byte>> _trnasmitQueue;

        public BluetoothTransmitter(
            ConcurrentValue<ConnectionState> connectionStateWrapper,
            ConcurrentValue<BluetoothSocket> bluetoothSocketWrapper, 
            ConcurrentQueue<Memory<byte>> trnasmitQueue,
            CancellationToken cancellationToken = default,
            int bufferSize = TransmitterBufferSizeDefault) : base(
                connectionStateWrapper, bluetoothSocketWrapper, cancellationToken, bufferSize)
        {
            _trnasmitQueue = trnasmitQueue;
        }

        /// <summary>
        /// The event that will be raised on a buffer sent throguh the current bluetooth connection.
        /// </summary>

        public event Transmitted OnTransmitted;

        protected override void StartUnitThread()
        {
            TimeSpan TimeoutDefault = new TimeSpan(0, 0, 0, 0, 1);

            Task.Run(async () =>
            {
                while (ConnectionState != ConnectionState.Disposing
                    && ConnectionState != ConnectionState.Disposed)
                {
                    Thread.Sleep(TimeoutDefault);
                    if (ConnectionState == ConnectionState.Connected)
                    {
                        await TransmitAsync();
                    }
                }
            }, _cancellationToken);
        }

        private async Task TransmitAsync()
        {
            Memory<byte> buffer;
            if (_trnasmitQueue.TryPeek(out buffer))
            {
                byte[] bufferAsByteArray = buffer.ToArray();

                try
                {
                    await BluetoothSocket?.OutputStream.WriteAsync(bufferAsByteArray,
                        BufferOffsetZero, buffer.Length, _cancellationToken);

                    _trnasmitQueue.TryDequeue(out buffer);

                    try
                    {
                        RaiseTransmittedEnvet(buffer);
                    }
                    catch (Exception exception)
                    {
                        LogWarning("TransmitAsync::RaiseTransmittedEvent::Exception", exception);
                    }
                }
                catch (Java.IO.IOException ioException)
                {
                    HandleException("TransmitAsync::BluetoothSocket?.OutputStream.WriteAsync::Java.IO.IOException",
                        new BluetoothTransmitException(ioException.Message, ioException, buffer));
                }
                catch (Exception exception)
                {
                    HandleException("TransmitAsync::Exception", exception);
                }
            }
        }

        private void RaiseTransmittedEnvet(Memory<byte> buffer)
        {
            OnTransmitted?.Invoke(this, new TransmittedEventArgs(buffer));
        }
    }
}