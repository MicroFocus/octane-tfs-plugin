$invocation = (Get-Variable MyInvocation).Value
$directorypath = Split-Path $invocation.MyCommand.Path

# Get the ID and security principal of the current user account
$myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)

# Get the security principal for the Administrator role
$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator

# Check to see if we are currently running "as Administrator"
if ($myWindowsPrincipal.IsInRole($adminRole))
   {
   # We are running "as Administrator" - so change the title and background color to indicate this
   $Host.UI.RawUI.WindowTitle = $myInvocation.MyCommand.Definition + "(Elevated)"
   $Host.UI.RawUI.BackgroundColor = "DarkBlue"
   clear-host
   }
else
   {
   # We are not running "as Administrator" - so relaunch as administrator

   # Create a new process object that starts PowerShell
   $newProcess = new-object System.Diagnostics.ProcessStartInfo "PowerShell";

   # Specify the current script path and name as a parameter
   $newProcess.Arguments = $myInvocation.MyCommand.Definition;

   # Indicate that the process should be elevated
   $newProcess.Verb = "runas";

   # Start the new process
   [System.Diagnostics.Process]::Start($newProcess);

   # Exit from the current, unelevated, process
   exit
   }

# Run your code that needs to be elevated here
Write-Host ("Current dir " + $directorypath)

Write-Host ("Sopping service TFSJobAgent")
stop-service "TFSJobAgent"

$filesToCopy = New-Object System.Collections.ArrayList
$targetDir = "c:\Program Files\Microsoft Team Foundation Server 15.0\Application Tier\TFSJobAgent\Plugins\"
$sourceDir = $directorypath + "\OctaneTFSPlugin\bin\Debug\"
$filesToCopy.Add($sourceDir + "MicroFocus.Adm.Octane.Api.Core.*")
$filesToCopy.Add($sourceDir + "MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin.*")
$filesToCopy.Add($sourceDir + "MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.*")
$filesToCopy.Add($sourceDir + "Newtonsoft.Json.*")
$filesToCopy.Add($sourceDir + "Nancy.Hosting.*")
$filesToCopy.Add($sourceDir + "log4net.*")


ForEach ($item In $filesToCopy)
{
  Write-Host $item
}

$input= Read-Host ("Continue?")
if($input="y"){
  ForEach ($item In $filesToCopy)
  {
    try{
          Write-Host ("Copy : " + $item)
          Copy-Item $item $targetDir
          Write-Host ("Copied!")
        }catch{
          Write-Host ("Failed to copy: " + $item )
        }
  }

}

Write-Host ("Starting service TFSJobAgent")
start-service "TFSJobAgent"

Write-Host -NoNewLine "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
