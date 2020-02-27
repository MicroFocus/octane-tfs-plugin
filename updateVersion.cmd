@echo off 
SET version=1.4
ECHO Version is %version%



ECHO **********************************************************
ECHO *************UPDATE ASSEMBLY FILES************************
ECHO **********************************************************
SET assemblyVersion=%version%.0.0
ECHO Assembly Version is %assemblyVersion%

SET assemblyVersionRegex="\[assembly: AssemblyVersion(.*)\]"
SET assemblyVersionReplacement="[assembly: AssemblyVersion(\"%assemblyVersion%\")]"
ECHO assemblyVersion     %assemblyVersionRegex%     , %assemblyVersionReplacement%

SET assemblyFileVersionRegex="\[assembly: AssemblyFileVersion(.*)\]"
SET assemblyFileVersionReplacement="[assembly: AssemblyFileVersion(\"%assemblyVersion%\")]"
ECHO assemblyFileVersion %assemblyFileVersionRegex% , %assemblyFileVersionReplacement%


set Arr[0]=.\OctaneManager\Properties\AssemblyInfo.cs 
set Arr[1]=.\OctaneTFSPlugin\Properties\AssemblyInfo.cs 
set Arr[2]=.\ConfigurationLauncher\Properties\AssemblyInfo.cs 
set Arr[3]=.\TfsConsolePluginRunner\Properties\AssemblyInfo.cs
set "x=0" 
:SymLoop 

if defined Arr[%x%] ( 
   call set a=%%Arr[%x%]%% 
   echo Updating %a%
   powershell -file replaceInFile.ps1 %a% %assemblyVersionRegex% %assemblyVersionReplacement% 
   powershell -file replaceInFile.ps1 %a% %assemblyFileVersionRegex% %assemblyFileVersionReplacement%
   echo Updating %a% is finished

   set /a "x+=1"
   GOTO :SymLoop 
)
echo "Number of updated assembly files is %x%

ECHO **********************************************************
ECHO ****************UPDATE YML FILE***************************
ECHO **********************************************************
SET ymlVersionRegex="version: .*{build}"
SET ymlVersionReplacement="version: "%version%".{build}"
ECHO assemblyFileVersion %ymlVersionRegex% , %ymlVersionReplacement%
powershell -file replaceInFile.ps1 ./appveyor.yml %ymlVersionRegex% %ymlVersionReplacement%



ECHO **********************************************************
ECHO ****************UPDATE SETUP FILE*************************
ECHO **********************************************************
rem see https://www.c-sharpcorner.com/article/how-to-perform-custom-actions-and-upgrade-using-visual-studio-installer
rem update product name
SET setupFilePath=.\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj
SET productNameRegex="\"ProductName\" = \"8:ALM Octane TFS CI Plugin .*\""
SET productNameReplacement="\"ProductName\" = \"8:ALM Octane TFS CI Plugin "%version%"\""
ECHO update ProductName %productNameRegex% , %productNameReplacement%
powershell -file replaceInFile.ps1 %setupFilePath% %productNameRegex% %productNameReplacement%


rem update packageCode
POWERSHELL [guid]::NewGuid().ToString().ToUpper() > uuid.tmp
set /p uuid=<uuid.tmp
SET packageCodeRegex="\"PackageCode\" = \"8:{.*}\""
SET packageCodeReplacement="\"PackageCode\" = \"8:{"%uuid%}"\""
ECHO update PackageCode %packageCodeRegex% , %packageCodeReplacement%
powershell -file replaceInFile.ps1 %setupFilePath% %packageCodeRegex% %packageCodeReplacement%

rem update ProductCode
POWERSHELL [guid]::NewGuid().ToString().ToUpper() > uuid.tmp
set /p uuid=<uuid.tmp
SET productCodeRegex="\"ProductCode\" = \"8:{.*}\""
SET productCodeReplacement="\"ProductCode\" = \"8:{"%uuid%}"\""
ECHO update ProductCode %productCodeRegex% , %productCodeReplacement%
powershell -file replaceInFile.ps1 %setupFilePath% %productCodeRegex% %productCodeReplacement%

rem update product version
SET productVersionRegex="\"ProductVersion\" = \"8:.*\""
SET productVersionReplacement="\"ProductVersion\" = \"8:%version%.0"\""
ECHO update ProductVersion %productVersionRegex% , %productVersionReplacement%
powershell -file replaceInFile.ps1 %setupFilePath% %productVersionRegex% %productVersionReplacement%

rem remove temp uuid file
del uuid.tmp


ECHO **********************DONE********************************
