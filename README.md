![picture](https://github.com/rostislav-nikitin/BluetoothClassic.Xamarin/blob/master/documentation/images/logo_128x128.png?raw=true)
# Plugin.BluetoothClassic for Xamarin
This is a plug-in that supports transmitting/receiving data with use of the SPP (Serial Port Profile) through a bluetooth classic protocol in the next types of the Xamarin projects:
* Xamarin.Forms
* Xamarin.Android

<!-- * Xamarin.iOS (not implemented yet)
* Xamarin.UWP (not implemented yet) -->

## How to install it?
One of they ways is to use a NuGet package manager:
* `PM> Install-Package Plugin.BluetoothClassic -Version 1.0.5`

If you building Xamarin.Forms application then you need to install this package into the both: Xamarin.Forms/Xamarin.Android projects.

More information about this NuGet package is accessbile by this link: https://www.nuget.org/packages/Plugin.BluetoothClassic/

## How to use it?
1. Use `DependencyService.Resolve<IBluetoothAdapter>` to get instance of the device default bluetooth adapter
2. Use `IBluetoothAdapter` in a next way:
  * Properties:
    * `bool Enabled` to check is bluetooth adapter enabled
    * `IEnumerable<BluetoothDeviceModel> BondedDevices` to show list of the bonded remote devices 
  * Methods:
    * `void Enable()` to enable a default bluetooth adapter
    * `void Disable()` to disable a default bluetooth adapter
    * `IBluetoothConnection CreateConnection(BluetoothDeviceModel bluetoothDeviceModel)` to create a new connection to the bonded remote device
3. Use `IBluetoothConnection: IDisposable` in a next way:
  * Properties:
    * `ConnectionState IBluetoothConnection.ConnectionState` to check a connection state
  * Methods:
    * `void Connect()` to connect to the remote device
    * `void IDisposable.Dispose()` to disconnect from the remote device and free unmanaged resources
    * `void Transmit(Memory<byte> buffer)` to add a buffer to the transmit queue (buffers are transmitting to the remote device sequentially while remote device connected otherwise data will be stored until it will be reconnected)
  * Events:
    * `event StateChanged OnStateChanged` to subscribe/unsubscribe on any connection state changes
    * `event Transmitted OnTransmitted` to subscribe/unsubscribe on a data transmitted to the connected remote device
    * `event Recived OnRecived` to subscribe/unsubscribe for/from reciving a data from the connected remote device
    * `event Error OnError` to subscribe/unsubscribe on any connection errors
    
Don't forget to add the next lines to the your {AplicationName}.Android/Properties/AndroidManifest.xml file:
```XML
<manifest xmlns:android="http://schemas.android.com/apk/res/android" 
          android:versionCode="1" android:versionName="1.0" 
          package="com.companyname.plugin.bluetoothclassic">
    ...
    <uses-feature android:name="android.hardware.bluetooth" />
    <uses-permission android:name="android.permission.BLUETOOTH" />
    <uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    ...
</manifest>
```

## Examples
The examples folder contains working example(s) that demonstrates how to use this package in practice.
