namespace Plugin.BluetoothClassic.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The static class that represents extensions for the <see cref="IBluetoothConnection"/> instances.
    /// </summary>
    public static class RetryBluetoothConnection
    {
        private const bool Success = true;
        private const bool Failed = false;

        /// <summary>
        /// The extension method that retries to connect to the remote device a specified amount of times.
        /// </summary>
        /// <param name="bluetoothConnection">The parameter that represents a current <see cref="IBluetoothConnection"/> instance.</param>
        /// <param name="retriesCount">The parameter that represents a retries count.</param>
        /// <returns>Returns the <see cref="Task{bool}"/> instance with true if get connected otherwise false.</returns>
        public static async Task<bool> RetryConnectAsync(this IBluetoothConnection bluetoothConnection,
            int retriesCount = 3)
        {
            while (retriesCount > 0)
            {
                try
                {
                    await bluetoothConnection.ConnectAsync();
                    return Success;
                }
                catch (BluetoothConnectionException)
                {
                    retriesCount--;
                }
                catch
                {
                    throw;
                }
            }

            return Failed;
        }

        /// <summary>
        /// The extension method that retries to transmit a data specified amount of times.
        /// </summary>
        /// <param name="bluetoothConnection">The parameter that represents a current <see cref="IBluetoothConnection"/> instance.</param>
        /// <param name="buffer">The parameter that represents a <see cref="Memory{byte}" /> buffer to transmit.</param>
        /// <param name="retriesCount">The parameter that represents a retries count.</param>
        /// <returns>Returns the <see cref="Task{bool}"/> instance with true if get transmitted otherwise false.</returns>
        public static async Task<bool> RetryTransmitAsync(this IBluetoothConnection bluetoothConnection,
            Memory<byte> buffer,
            CancellationToken cancellationToken = default,
            int retriesCount = 3)
        {
            while (retriesCount > 0)
            {
                try
                {
                    await bluetoothConnection.TransmitAsync(buffer, cancellationToken);
                    return Success;
                }
                catch(BluetoothDataTransferUnitException)
                {
                    retriesCount--;
                }
                catch
                {
                    throw;
                }
            }

            return Failed;
        }

        /// <summary>
        /// The extension method that retries to transmit a data specified amount of times.
        /// </summary>
        /// <param name="bluetoothConnection">The parameter that represents a current <see cref="IBluetoothConnection"/> instance.</param>
        /// <param name="buffer">The parameter that represents a <see cref="buffer[]"/> to transmit.</param>
        /// <param name="offset">The parameter that represents a buffer offset.</param>
        /// <param name="count">The parameter that represents a cound of bytes to trnasmit.</param>
        /// <param name="retriesCount">The parameter that represents a retries count.</param>
        /// <returns>Returns the <see cref="Task{bool}"/> instance with true if get transmitted otherwise false.</returns>
        public static async Task<bool> RetryTransmitAsync(this IBluetoothConnection bluetoothConnection,
            byte[] buffer,
            int offset,
            int count,
            int retriesCount = 3)
        {
            while (retriesCount > 0)
            {
                try
                {
                    await bluetoothConnection.TransmitAsync(buffer, offset, count);
                    return Success;
                }
                catch(BluetoothDataTransferUnitException)
                {
                    retriesCount--;
                }
                catch
                {
                    throw;
                }
            }

            return Failed;
        }

        /// <summary>
        /// The extension method that retries to transmit a data specified amount of times.
        /// </summary>
        /// <param name="bluetoothConnection">The parameter that represents a current <see cref="IBluetoothConnection"/> instance.</param>
        /// <param name="buffer">The parameter that represents a <see cref="buffer[]"/> to transmit.</param>
        /// <param name="offset">The parameter that represents a buffer offset.</param>
        /// <param name="count">The parameter that represents a cound of bytes to trnasmit.</param>
        /// <param name="cancellationToken">The parameter that represents a <see cref="CancellationToken"/> instance.</param>
        /// <param name="retriesCount">The parameter that represents a retries count.</param>
        /// <returns>Returns the <see cref="Task{bool}"/> instance with true if get transmitted otherwise false.</returns>
        public static async Task<bool> RetryTransmitAsync(this IBluetoothConnection bluetoothConnection, 
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken,
            int retriesCount = 3)
        {
            while (retriesCount > 0)
            {
                try
                {
                    await bluetoothConnection.TransmitAsync(buffer, offset, count, cancellationToken);
                    return Success;
                }
                catch (BluetoothDataTransferUnitException)
                {
                    retriesCount--;
                }
                catch
                {
                    throw;
                }
            }

            return Failed;
        }

        /// <summary>
        /// The extension method that retries to recive a data specified amount of times.
        /// </summary>
        /// <param name="bluetoothConnection">The parameter that represents a current <see cref="IBluetoothConnection"/> instance.</param>
        /// <param name="buffer">The parameter that represents a <see cref="Memory{byte}" /> buffer to recive.</param>
        /// <param name="retriesCount">The parameter that represents a retries count.</param>
        /// <returns>Returns the <see cref="Task{(bool Succeeded, int Count)}"/> instance.</returns>
        public static async Task<(bool Succeeded, int Count)> RetryReciveAsync(this IBluetoothConnection bluetoothConnection,
            Memory<byte> buffer,
            CancellationToken cancellationToken = default,
            int retriesCount = 3)
        {
            while (retriesCount > 0)
            {
                try
                {
                    int result = await bluetoothConnection.ReciveAsync(buffer, cancellationToken);
                    return (Success, result);
                }
                catch (BluetoothDataTransferUnitException)
                {
                    retriesCount--;
                }
                catch
                {
                    throw;
                }
            }

            return (Failed, default);
        }

        /// <summary>
        /// The extension method that retries to recive a data specified amount of times.
        /// </summary>
        /// <param name="bluetoothConnection">The parameter that represents a current <see cref="IBluetoothConnection"/> instance.</param>
        /// <param name="buffer">The parameter that represents a <see cref="buffer[]"/> to recive.</param>
        /// <param name="offset">The parameter that represents a recive buffer offset.</param>
        /// <param name="count">The parameter that represents a cound of bytes to recive.</param>
        /// <param name="retriesCount">The parameter that represents a retries count.</param>
        /// <returns>Returns the <see cref="Task{(bool Succeeded, int Count)}"/> instance.</returns>
        public static async Task<(bool Succeeded, int Count)> RetryReciveAsync(this IBluetoothConnection bluetoothConnection,
            byte[] buffer,
            int offset,
            int count,
            int retriesCount = 3)
        {
            while (retriesCount > 0)
            {
                try
                {
                    int result = await bluetoothConnection.ReciveAsync(buffer, offset, count);
                    return (Success, result);
                }
                catch (BluetoothDataTransferUnitException)
                {
                    retriesCount--;
                }
                catch
                {
                    throw;
                }
            }

            return (Failed, default);
        }

        /// <summary>
        /// The extension method that retries to recive a data specified amount of times.
        /// </summary>
        /// <param name="bluetoothConnection">The parameter that represents a current <see cref="IBluetoothConnection"/> instance.</param>
        /// <param name="buffer">The parameter that represents a <see cref="buffer[]"/> to recive.</param>
        /// <param name="offset">The parameter that represents a recive buffer offset.</param>
        /// <param name="count">The parameter that represents a cound of bytes to trnasmit.</param>
        /// <param name="cancellationToken">The parameter that represents a <see cref="CancellationToken"/> instance.</param>
        /// <param name="retriesCount">The parameter that represents a retries count.</param>
        /// <returns>Returns the <see cref="Task{(bool Succeeded, int Count)}"/> instance.</returns>
        public static async Task<(bool Succeeded, int Count)> RetryReciveAsync(this IBluetoothConnection bluetoothConnection,
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken,
            int retriesCount = 3)
        {
            while (retriesCount > 0)
            {
                try
                {
                    int result = await bluetoothConnection.ReciveAsync(buffer, offset, count, cancellationToken);
                    return (Success, result);
                }
                catch (BluetoothDataTransferUnitException)
                {
                    retriesCount--;
                }
                catch
                {
                    throw;
                }

                return (Failed, default);
            }

            return default;
        }
    }
}
