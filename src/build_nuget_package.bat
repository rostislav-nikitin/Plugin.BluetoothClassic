@echo off

if "%~1"=="" goto error

nuget pack -Properties Configuration=Release -Version %1

goto exit

:error

echo Error: versions parameter not specfied.
echo Use build_nuget_package.bat {vesion}.
echo Example: build_nuget_package.bat 1.1.0

:exit