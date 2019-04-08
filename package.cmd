@echo off 
regsvr32 /s ole32.dll

SET installerProjectFile=.\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj
SET installPathRegexToFind="\"DefaultLocation\" = \"8:.*\""
SET installProductNameRegex="\"ProductName\" = \"8:TFS .* Plugin for ALM Octane"
SET tfsVersionRegexToFind="TfsVersionEnum.Tfs20.+;"

ECHO **********************************************************
ECHO *******************BUILD  2015****************************
ECHO **********************************************************

REM update installation path in setup project
copy /Y .\tfs-server-dlls\2015 .\tfs-server-dlls\current
SET installPathReplacement="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 14.0\\Application Tier\\TFSJobAgent\\Plugins\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement% 
ECHO update install path to %installPathReplacement% 

REM update setup title with correct version of tfs
SET installProductNameReplacement="\"ProductName\" = \"8:TFS 2015 Plugin for ALM Octane"
powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement%
ECHO update product name in installer to %installProductNameReplacement% 

REM build setup files
"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2015


ECHO **********************************************************
ECHO *******************BUILD  2017****************************
ECHO **********************************************************
copy /Y .\tfs-server-dlls\2017 .\tfs-server-dlls\current

REM update installation path in setup project
SET installPathReplacement="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 15.0\\Application Tier\\TFSJobAgent\\Plugins\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement%
ECHO update install path to %installPathReplacement% 

REM update setup title with correct version of tfs
SET installProductNameReplacement="\"ProductName\" = \"8:TFS 2017 Plugin for ALM Octane"
powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement%
ECHO update product name in installer to %installProductNameReplacement% 

REM build setup files
"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2017


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

REM build setup files
"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2018


ECHO **********************************************************
ECHO *******************BUILD  2019****************************
ECHO **********************************************************
copy /Y .\tfs-server-dlls\2019 .\tfs-server-dlls\current

REM update installation path in setup project
SET installPathReplacement="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Azure DevOps Server 2019\\Application Tier\\TFSJobAgent\\Plugins\""
powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement%
ECHO update install path to %installPathReplacement% 

REM update setup title with correct version of tfs
SET installProductNameReplacement="\"ProductName\" = \"8:TFS 2019 Plugin for ALM Octane"
powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement%
ECHO update product name in installer to %installProductNameReplacement% 

REM build setup files
"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2019

ECHO ***********************DONE*******************************
