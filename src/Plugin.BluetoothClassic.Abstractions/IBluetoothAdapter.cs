using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.BluetoothClassic.Abstractions
{
    public interface IBluetoothAdapter
    {
        bool BluetoothSupported { get; }
        bool Enabled { get; }
        void Enable();
        void Disable();

        void StartDiscovery();
        void StopDiscovery();
        IEnumerable<BluetoothDeviceModel> BondedDevices { get; }

        IBluetoothConnection CreateConnection(BluetoothDeviceModel bluetoothDeviceModel);
    }
}
