@echo off 
regsvr32 /s ole32.dll

mkdir .\tfs-server-dlls\current
SET installerProjectFile=.\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj
SET tfsVersionFile=.\OctaneManager\Tools\RunModeManager.cs
SET installPathRegexToFind="\"DefaultLocation\" = \"8:.*\""
SET tfsVersionRegexToFind="TfsVersionEnum.Tfs20.+;"

ECHO **********************************************************
ECHO *******************BUILD  2015****************************
ECHO **********************************************************

copy /Y .\tfs-server-dlls\2015 .\tfs-server-dlls\current
SET installPathReplacement="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 14.0\\Application Tier\\TFSJobAgent\\Plugins\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement% 
ECHO update install path to %installPathReplacement% 

SET tfsVersionReplacement="TfsVersionEnum.Tfs2015;"
powershell -file replaceInFile.ps1 %tfsVersionFile% %tfsVersionRegexToFind% %tfsVersionReplacement% 
ECHO update tfs version to %tfsVersionReplacement% 

"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2015


ECHO **********************************************************
ECHO *******************BUILD  2017****************************
ECHO **********************************************************
copy /Y .\tfs-server-dlls\2017 .\tfs-server-dlls\current

SET installPathReplacement="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 15.0\\Application Tier\\TFSJobAgent\\Plugins\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement%
ECHO update install path to %installPathReplacement% 

SET tfsVersionReplacement="TfsVersionEnum.Tfs2017;"
powershell -file replaceInFile.ps1 %tfsVersionFile% %tfsVersionRegexToFind% %tfsVersionReplacement% 
ECHO update tfs version to %tfsVersionReplacement% 

"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2017

ECHO ***********************DONE*******************************