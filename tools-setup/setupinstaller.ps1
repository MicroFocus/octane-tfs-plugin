$tabs="`t`t`t`t`t`t"
$stringToReplace= '.*DefaultLocation.*'
$tfs2015Location=$tabs + '"DefaultLocation" = "8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 14.0\\Application Tier\\TFSJobAgent\\Plugins"'
$tfs2017Location=$tabs + '"DefaultLocation" = "8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 15.0\\Application Tier\\TFSJobAgent\\Plugins"'
$tfs2018Location=$tabs + '"DefaultLocation" = "8:[ProgramFiles64Folder]\\Microsoft Team Foundation Server 2018\\Application Tier\\TFSJobAgent\\Plugins"'

switch($args[0])
{
    'tfs2015'
    {
        $tfsLocation = $tfs2015Location
    }
    'tfs2017'
    {
        $tfsLocation = $tfs2017Location
    }
    'tfs2018'
    {
        $tfsLocation = $tfs2018Location
    }
    default
    {
        Write-Error -Message 'no type specified'
        exit
    }
}
$installerProjectFile='..\OctaneTfsPluginSetup\OctaneTfsPluginSetup.vdproj'

$newFile = Get-Content $installerProjectFile | Foreach-Object {$_ -replace $stringToReplace,$tfsLocation }

Set-Content -Path $installerProjectFile -Value $newFile
