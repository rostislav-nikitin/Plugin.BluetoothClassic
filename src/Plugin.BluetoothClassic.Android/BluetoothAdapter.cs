[assembly: Xamarin.Forms.Dependency(typeof(Plugin.BluetoothClassic.Droid.BluetoothAdapter))]
namespace Plugin.BluetoothClassic.Droid
{
    using Plugin.BluetoothClassic.Abstractions;
    using Java.Lang;
    using System.Collections.Generic;
    using System.Linq;

    using AndroidBluetoothAdapter = Android.Bluetooth.BluetoothAdapter;

    /// <summary>
    /// The class that pepresents a <see cref="BluetoothAdapter"/> type.
    /// It can be used to control default device bluetooth adapter and to create a new bluetooth connections instances.
    /// </summary>
    public class BluetoothAdapter : IBluetoothAdapter
    {
        private long DefaultWaitTimeoutInMsec = 100; 

        /// <summary>
        /// The constructor that creates <see cref="BluetoothAdapter"/> type instance.
        /// </summary>
        public BluetoothAdapter()
        {
        }

        /// <summary>
        /// The property that represents an availability of the bluetooth device.
        /// It returns true if the current device contains bluetooth adapter, otherwise false.
        /// </summary>
        public bool BluetoothSupported => AndroidBluetoothAdapter.DefaultAdapter != null;

        /// <summary>
        /// The property that represents a default bluetooth device adapter state. 
        /// It retrns true if the current bluetooth adapter is enabled, otherwise false.
        /// </summary>
        public bool Enabled => BluetoothSupported && AndroidBluetoothAdapter.DefaultAdapter.IsEnabled;

        /// <summary>
        /// The method that enables a default bluetooth adapter.
        /// </summary>
        public void Enable()
        {
            if (BluetoothSupported && !Enabled)
            {
                AndroidBluetoothAdapter.DefaultAdapter.Enable();
                while (!Enabled)
                {
                    Thread.Sleep(DefaultWaitTimeoutInMsec);
                }
            }
            
        }

        /// <summary>
        /// The method that disables a default bluetooth adapter.
        /// </summary>
        public void Disable()
        {
            if (BluetoothSupported && Enabled)
            {
                AndroidBluetoothAdapter.DefaultAdapter.Disable();
                while (Enabled)
                {
                    Thread.Sleep(DefaultWaitTimeoutInMsec);
                }
            }
        }

        /// <summary>
        /// The method that starts a bluetooth devices discovery process.
        /// </summary>
        public void StartDiscovery()
        {
            AndroidBluetoothAdapter.DefaultAdapter.StartDiscovery();
        }

        /// <summary>
        /// The method that stops a bluetooth devices discovery process.
        /// </summary>
        public void StopDiscovery()
        {
            AndroidBluetoothAdapter.DefaultAdapter.CancelDiscovery();
        }

        /// <summary>
        /// The property that represents a list of the bounded bluetooth devices.
        /// </summary>
        public IEnumerable<BluetoothDeviceModel> BondedDevices
        {
            get
            {
                return AndroidBluetoothAdapter.DefaultAdapter.BondedDevices
                    .Select(btd => new BluetoothDeviceModel(btd.Address, btd.Name));
            }
        }

        /// <summary>
        /// The method that creates a short time connection between a current bluetooth adapter and the remote bluetooth device.
        /// The <see cref="IBluetoothConnection"/> can be used for the short time connections, when required to transmit/recieve a some small potion of the data then close connection.
        /// </summary>
        /// <param name="bluetoothDeviceModel">The property that represents a model of the remote device to connect to.</param>
        /// <returns>Returns the <see cref="IBluetoothConnection"/> instance.</returns>
        public IBluetoothConnection CreateConnectionAsync(BluetoothDeviceModel bluetoothDeviceModel)
        {
            return new BluetoothConnection(bluetoothDeviceModel.Address);
        }

        /// <summary>
        /// The method that creates a managed connection between a current bluetooth adapter and the remote bluetooth device.
        /// <see cref="IBluetoothManagedConnection"/> can be used for the long time connections. 
        /// Managed means that it contains the internal manager that control a connection state, reconnect it if required and do other tasks at the background.
        /// </summary>
        /// <param name="bluetoothDeviceModel">The property that represents a model of the remote device to connect to.</param>
        /// <returns>Returns the <see cref="IBluetoothManagedConnection"/> instance.</returns>
        public IBluetoothManagedConnection CreateManagedConnection(BluetoothDeviceModel bluetoothDeviceModel)
        {
            return new BluetoothManagedConnection(bluetoothDeviceModel.Address);
        }
    }
}