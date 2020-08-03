[assembly: Xamarin.Forms.Dependency(typeof(BluetoothClassic.Droid.BluetoothAdapter))]
namespace BluetoothClassic.Droid
{
    using BluetoothClassic.Base;
    using Java.Lang;
    using System.Collections.Generic;
    using System.Linq;

    using AndroidBluetoothAdapter = Android.Bluetooth.BluetoothAdapter;

    /// <summary>
    /// The class that pepresents a <see cref="BluetoothAdapter"/> type.
    /// Usage: to control default device bluetooth adapter and to create a new <see cref="IBluetoothConnection"/> instances.
    /// </summary>
    public class BluetoothAdapter : IBluetoothAdapter
    {
        private long DefaultWaitTimeoutInMsec = 100; 

        /// <summary>
        /// The <see cref="BluetoothAdapter"/> type constructor.
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
        /// The method that created a new bluetooth connection object.
        /// </summary>
        /// <param name="bluetoothDeviceModel">The parameters that represents a bluetooth device to connect to.</param>
        /// <returns>The new instance that implements a <see cref="IBluetoothConnection"/>.</returns>
        public IBluetoothConnection CreateConnection(BluetoothDeviceModel bluetoothDeviceModel)
        {
            return new BluetoothConnection(bluetoothDeviceModel.Address);
        }

    }
}