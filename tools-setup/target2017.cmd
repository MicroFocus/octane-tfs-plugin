mkdir ..\tfs-server-dlls\current
copy /Y ..\tfs-server-dlls\2017 ..\tfs-server-dlls\current
echo %CD%
powershell -file setupinstaller.ps1 tfs2017