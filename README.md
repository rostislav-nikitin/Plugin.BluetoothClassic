![picture](https://github.com/rostislav-nikitin/BluetoothClassic.Xamarin/blob/master/documentation/images/logo_128x128.png?raw=true)
# BluetoothClassic for Xamarin
This is a package that supports sending/receiving data through bluetooth classic protocol under the:
* Xamarin.Forms
* Xamarin.Android
<!-- * Xamarin.iOS (not implemented yet)
* Xamarin.UWP (not implemented yet) -->

## How to install it?
Install a next nuget package into your Xamarin.Forms project:
* `PM> Install-Package BluetoothClassic.Base -Version 1.1.0`

Install a next nuget packages into your Xamarin.Forms/Xamarin.Android projects:
* `PM> Install-Package BluetoothClassic.Base -Version 1.1.0`
* `PM> Install-Package BluetoothClassic.Android -Version 1.1.0`

## How to use it?
1. Use `DependencyService.Resolve<IBluetoothAdapter>` to get instance of the phone default bluetooth adapter
2. Use `IBluetoothAdapter` in a next way:
  * Properties:
    * `bool Enabled` to check is bluetooth adapter enabled
    * `IEnumerable<BluetoothDeviceModel> BondedDevices` to show list of the bonded remote devices 
  * Methods:
    * `void Enable()` to enable a bluetooth adapter
    * `void Disable()` to disable a bluetooth adapter
    * `IBluetoothConnection CreateConnection(BluetoothDeviceModel bluetoothDeviceModel)` to create a new connection to the bonded remote device
3. Use `IBluetoothConnection: IDisposable` in a next way:
  * Properties:
    * `ConnectionState IBluetoothConnection.ConnectionState` to check connection state
  * Methods:
    * `void Connect()` to connect to the remote device
    * `void IDisposable.Dispose()` to disconnect from the remote device and free unmanaged resources
    * `void Transmit(Memory<byte> buffer)` to add a buffer to the transmit queue (buffers are transmitting to the connected remote device sequentially on remote device connected)
  * Events:
    * `event StateChanged OnStateChanged` to subscribe/unsubscribe on any connection state changes
    * `event Transmitted OnTransmitted` to subscribe/unsubscribe on a data transmitted to the connected remote device
    * `event Recived OnRecived` to subscribe/unsubscribe for/from reciving a data from the connected remote device
    * `event Error OnError` to subscribe/unsubscribe on any connection errors

## Examples
The examples folder contains working example(s) that demonstrates how to use this package in practice.
