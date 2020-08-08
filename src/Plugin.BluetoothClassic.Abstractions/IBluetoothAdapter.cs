namespace Plugin.BluetoothClassic.Abstractions
{
    using System.Collections.Generic;

    public interface IBluetoothAdapter
    {
        /// <summary>
        /// The property that represents is bluetooth supported by the current device.
        /// </summary>
        bool BluetoothSupported { get; }

        /// <summary>
        /// The property that represents is bluetooth adapter enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// The method that enables a bluetooth adapter.
        /// </summary>
        void Enable();

        /// <summary>
        /// The method that disables a bluetooth adapter.
        /// </summary>
        void Disable();

        /// <summary>
        /// The method that starts bluetooth remote devices discovery.
        /// </summary>
        void StartDiscovery();

        /// <summary>
        /// The method that stops bluetooth remote device discovery.
        /// </summary>
        void StopDiscovery();

        /// <summary>
        /// The property that represents a bonded devices.
        /// </summary>
        IEnumerable<BluetoothDeviceModel> BondedDevices { get; }

        /// <summary>
        /// The method that creates a short time connection between a current bluetooth adapter and the remote bluetooth device.
        /// The <see cref="IBluetoothConnection"/> can be used for the short time connections, when required to transmit/recieve some small potion of data then close connection.
        /// </summary>
        /// <param name="bluetoothDeviceModel">The property that represents a model of the remote device to connect to.</param>
        /// <returns>Returns the <see cref="IBluetoothConnection"/> instance.</returns>
        IBluetoothConnection CreateConnection(BluetoothDeviceModel bluetoothDeviceModel);

        /// <summary>
        /// The method that creates a managed connection between a current bluetooth adapter and the remote bluetooth device.
        /// <see cref="IBluetoothManagedConnection"/> can be used for the long time connections. 
        /// Managed means that it contains the internal manager that control a connection state, reconnect it if required and do other tasks at the background.
        /// </summary>
        /// <param name="bluetoothDeviceModel">The property that represents a model of the remote device to connect to.</param>
        /// <returns>Returns the <see cref="IBluetoothManagedConnection"/> instance.</returns>
        IBluetoothManagedConnection CreateManagedConnection(BluetoothDeviceModel bluetoothDeviceModel);
    }
}
