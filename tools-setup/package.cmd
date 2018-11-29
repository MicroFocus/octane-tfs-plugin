REM Run package and generate setups
regsvr32 /s ole32.dll
call target2015.cmd
echo build msi
"%DEVENV_LOCATION%" ..\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2015
call target2017.cmd
"%DEVENV_LOCATION%" ..\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2017
