@echo off

if "%~1"=="" goto error
if "%~2"=="" goto error

nuget push Plugin.BluetoothClassic.%1.nupkg  %2 -Source https://api.nuget.org/v3/index.json

goto exit

:error
echo:
echo Error: The {version} or(and) {key} parameter(s) not specfied.
echo:
echo Usage:
echo     upload_nuget_package.bat {vesion} {key}
echo:
echo Example:
echo     build_nuget_package.bat 1.1.0 otrfgww3sqhqqj4ueufzbtyafyaezgle3gnkp3tiomde7i

:exit

