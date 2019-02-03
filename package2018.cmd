@echo off 
regsvr32 /s ole32.dll

REM Set shared parameters
SET installerProjectFile=.\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj
SET tfsVersionFile=.\OctaneManager\Tools\RunModeManager.cs
SET installPathRegexToFind="\"DefaultLocation\" = \"8:.*\""
SET installProductNameRegex="\"ProductName\" = \"8:TFS .* Plugin for ALM Octane"
SET tfsVersionRegexToFind="TfsVersionEnum.Tfs20.+;"

ECHO **********************************************************
ECHO *******************BUILD  2018****************************
ECHO **********************************************************
copy /Y .\tfs-server-dlls\2018 .\tfs-server-dlls\current

REM update installation path in setup project
SET installPathReplacement="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 2018\\Application Tier\\TFSJobAgent\\Plugins\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement%
ECHO update install path to %installPathReplacement% 

REM update setup title with correct version of tfs
SET installProductNameReplacement="\"ProductName\" = \"8:TFS 2018 Plugin for ALM Octane"
powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement%
ECHO update product name in installer to %installProductNameReplacement% 

REM update tfs version in RunModeManager.cs
SET tfsVersionReplacement="TfsVersionEnum.Tfs2018;"
powershell -file replaceInFile.ps1 %tfsVersionFile% %tfsVersionRegexToFind% %tfsVersionReplacement% 
ECHO update tfs version to %tfsVersionReplacement% 

REM build setup files
"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2018


ECHO ***********************DONE*******************************