@echo off 
regsvr32 /s ole32.dll

SET installerProjectFile=.\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj
SET installPathRegexToFind="\"DefaultLocation\" = \"8:.*\""
SET installProductNameRegex="\"ProductName\" = \"8:.*\""
SET tfsVersionRegexToFind="TfsVersionEnum.Tfs20.+;"

ECHO **********************************************************
ECHO *******************BUILD  2019****************************
ECHO **********************************************************
copy /Y .\tfs-server-dlls\2019 .\tfs-server-dlls\current

REM update installation path in setup project
SET installPathReplacement="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Azure DevOps Server 2019\\Application Tier\\TFSJobAgent\\Plugins\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement%
ECHO update install path to %installPathReplacement% 

REM update setup title with correct version of tfs
SET installProductNameReplacement="\"ProductName\" = \"8:ALM Octane CI Plugin v1.4 for Azure DevOps Server 2019\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement%
ECHO update product name in installer to %installProductNameReplacement% 

REM build setup files
"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2019

ECHO ***********************DONE*******************************
