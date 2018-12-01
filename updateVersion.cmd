@echo off 
SET version=1.3
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
set Arr[2]=.\OctaneTFSPluginConfiguratorUI\Properties\AssemblyInfo.cs 
set Arr[3]=.\TfsConsolePluginRunner\Properties\AssemblyInfo.cs
set "x=0" 
:SymLoop 

if defined Arr[%x%] ( 
   call set a=%%Arr[%x%]%% 
   echo Updating %a%
   powershell -file replaceInFile.ps1 %a% %assemblyVersionRegex% %assemblyVersionReplacement% 
   powershell -file replaceInFile.ps1 %a% %assemblyFileVersionRegex% %assemblyFileVersionReplacement%

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
SET setupRegex="\"ProductName\" = \"8:ALM Octane TFS CI Plugin .*\""
SET setupReplacement="\"ProductName\" = \"8:ALM Octane TFS CI Plugin "%version%"\""
ECHO setupFile %setupRegex% , %setupReplacement%
powershell -file replaceInFile.ps1 .\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj %setupRegex% %setupReplacement%

ECHO !!!! UPDATE Setup version and UPGRADE CODE manually
ECHO see https://www.c-sharpcorner.com/article/how-to-perform-custom-actions-and-upgrade-using-visual-studio-installer/

ECHO **********************DONE********************************
