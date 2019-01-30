@echo off 
regsvr32 /s ole32.dll

REM Set shared parameters
SET installerProjectFile=.\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj
SET tfsVersionFile=.\OctaneManager\Tools\RunModeManager.cs
SET installPathRegexToFind="\"DefaultLocation\" = \"8:.*\""
SET installProductNameRegex="\"ProductName\" = \"8:TFS .* Plugin for ALM Octane"
SET tfsVersionRegexToFind="TfsVersionEnum.Tfs20.+;"

REM Set runTfs2015
set runTfs2015=false
if "%1"=="2015" set runTfs2015=true
if "%1"=="" set runTfs2015=true
ECHO runTfs2015=%runTfs2015%

REM Set runTfs2017
set runTfs2017=false
if "%1"=="2017" set runTfs2017=true
if "%1"=="" set runTfs2017=true
ECHO runTfs2017=%runTfs2017%

REM Set runTfs2018
set runTfs2018=false
if "%1"=="2018" set runTfs2018=true
if "%1"=="" set runTfs2018=true
ECHO runTfs2018=%runTfs2018%



REM runTfs2015
if "%runTfs2015%" == "true" (
	ECHO **********************************************************
	ECHO *******************BUILD  2015****************************
	ECHO **********************************************************

	REM update installation path in setup project
	copy /Y .\tfs-server-dlls\2015 .\tfs-server-dlls\current
	SET installPathReplacement2015="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 14.0\\Application Tier\\TFSJobAgent\\Plugins\""
	powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement2015% 
	ECHO update install path to %installPathReplacement2015% 

	REM update setup title with correct version of tfs
	SET installProductNameReplacement2015="\"ProductName\" = \"8:TFS 2015 Plugin for ALM Octane"
	powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement2015%
	ECHO update product name in installer to %installProductNameReplacement2015% 

	REM update tfs version in RunModeManager.cs
	SET tfsVersionReplacement2015="TfsVersionEnum.Tfs2015;"
	powershell -file replaceInFile.ps1 %tfsVersionFile% %tfsVersionRegexToFind% %tfsVersionReplacement2015% 
	ECHO update tfs version to %tfsVersionReplacement2015% 

	REM build setup files
	"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2015
)

REM runTfs2017
if "%runTfs2017%" == "true" (
	ECHO **********************************************************
	ECHO *******************BUILD  2017****************************
	ECHO **********************************************************
	copy /Y .\tfs-server-dlls\2017 .\tfs-server-dlls\current

	REM update installation path in setup project
	SET installPathReplacement2017="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 15.0\\Application Tier\\TFSJobAgent\\Plugins\""
	powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement2017%
	ECHO update install path to %installPathReplacement2017% 

	REM update setup title with correct version of tfs
	SET installProductNameReplacement2017="\"ProductName\" = \"8:TFS 2017 Plugin for ALM Octane"
	powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement2017%
	ECHO update product name in installer to %installProductNameReplacement2017% 

	REM update tfs version in RunModeManager.cs
	SET tfsVersionReplacement2017="TfsVersionEnum.Tfs2017;"
	powershell -file replaceInFile.ps1 %tfsVersionFile% %tfsVersionRegexToFind% %tfsVersionReplacement2017% 
	ECHO update tfs version to %tfsVersionReplacement2017% 

	REM build setup files
	"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2017
)

REM runTfs2018
if "%runTfs2017%" == "true" (
	ECHO **********************************************************
	ECHO *******************BUILD  2018****************************
	ECHO **********************************************************
	copy /Y .\tfs-server-dlls\2018 .\tfs-server-dlls\current

	REM update installation path in setup project
	SET installPathReplacement2018="\"DefaultLocation\" = \"8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 2018\\Application Tier\\TFSJobAgent\\Plugins\""
	powershell -file replaceInFile.ps1 %installerProjectFile% %installPathRegexToFind% %installPathReplacement2018%
	ECHO update install path to %installPathReplacement2018% 

	REM update setup title with correct version of tfs
	SET installProductNameReplacement2018="\"ProductName\" = \"8:TFS 2018 Plugin for ALM Octane"
	powershell -file replaceInFile.ps1 %installerProjectFile% %installProductNameRegex% %installProductNameReplacement2018%
	ECHO update product name in installer to %installProductNameReplacement2018% 

	REM update tfs version in RunModeManager.cs
	SET tfsVersionReplacement2018="TfsVersionEnum.Tfs2018;"
	powershell -file replaceInFile.ps1 %tfsVersionFile% %tfsVersionRegexToFind% %tfsVersionReplacement2018% 
	ECHO update tfs version to %tfsVersionReplacement2018% 

	REM build setup files
	"%DEVENV_LOCATION%" .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj /build Package2018
)

ECHO ***********************DONE*******************************