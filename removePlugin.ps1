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

Write-Host ("Stopping service TFSJobAgent")
stop-service "TFSJobAgent"


$filesToRemove = New-Object System.Collections.ArrayList
$baseDir = "c:\Program Files\Microsoft Team Foundation Server 15.0\Application Tier\TFSJobAgent\Plugins\"
$filesToRemove.Add($baseDir + "MicroFocus.Adm.Octane.Api.Core.*")
$filesToRemove.Add($baseDir + "MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin.*")
$filesToRemove.Add($baseDir + "MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.*")
$filesToRemove.Add($baseDir + "Newtonsoft.Json.*")
$filesToRemove.Add($baseDir + "Nancy.Hosting.*")
$filesToRemove.Add($baseDir + "log4net.*")

ForEach ($item In $filesToRemove)
{
  Write-Host $item
}

$input= Read-Host ("Continue?")
if($input="y"){
  ForEach ($item In $filesToRemove)
  {
    try{
          Write-Host ("Deleting : " + $item)
          Remove-Item $item
        }catch{
          Write-Host ("Failed Deleting " + $item )
        }
  }

}
Write-Host ("Starting service TFSJobAgent")
stop-service "TFSJobAgent"

Write-Host -NoNewLine "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
