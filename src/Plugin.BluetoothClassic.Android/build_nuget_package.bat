@echo off

if "%~1"=="" goto error

nuget pack Plugin.BluetoothClassic.Android.nuspec -Properties Configuration=Release -Version %1

goto exit

:error
echo:
echo Error: The {version} parameter not specfied.
echo:
echo Usage: 
echo     build_nuget_package.bat {vesion}
echo:
echo Example: 
echo     build_nuget_package.bat 1.1.0
:exit