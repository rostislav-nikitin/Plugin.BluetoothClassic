![picture](https://github.com/rostislav-nikitin/BluetoothClassic.Xamarin/blob/master/documentation/logo/logo.png?raw=true)
# BluetoothClassic for Xamarin
This is a package that supports sending/receiving data through bluetooth classic protocol under the:
* Xamarin.Forms
* Xamarin.Android
<!-- * Xamarin.iOS (not implemented yet)
* Xamarin.UWP (not implemented yet) -->

## How to install it?
Install a next nuget package into your Xamarin.Forms project:
* `PM> Install-Package BluetoothClassic.Base -Version 1.0`

Install a next nuget package into your Xamarin.Forms/Xamarin.Android projects:
* `PM> Install-Package BluetoothClassic.Base -Version 1.0`
* `PM> Install-Package BluetoothClassic.Android -Version 1.0`


## How to use it?
1. Use `DependencyService.Resolve<IBluetoothAdapter>` to get instance of the phone default bluetooth adapter
2. Use `IBluetoothAdapter` in a next way:
  * `IBluetoothAdapter.Enabled` to check is adapter enabled
  * `IBluetoothAdapter.Enable()/IBluetoothAdapter.Disable()` to enable it or disable
  * `IEnumerable<BluetoothDeviceModel> IBluetoothAdapter.BondedDevices` to show list of the bonded remote devices 
  * `IBluetoothConnection CreateConnection(BluetoothDeviceModel bluetoothDeviceModel)` to create a new connection to the bonded remote device
3. Use `IBluetoothConnection: IDisposable` in a next way:
  * `Task IBluetoothConnection.ConnectAsync()` to connect to the remote device
  * `void IDisposable.Dispose()` to disconnect from the remote device and free unmanaged resources
  * `bool IBluetoothConnection.Connected` to check is still connected to the remote device (for now work not so stable)
  * `Task SendAsync(Memory<byte> buffer)` to send byte buffer to the connected remote device
  * `event Recived OnRecived` to subscribe/unsubscribe for/from reciving a data from the connected remote device

## Examples
The examples folder contains working example(s) that demonstrates how to use this package in practice.
