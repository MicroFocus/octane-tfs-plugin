mkdir ..\tfs-server-dlls\current
copy /Y ..\tfs-server-dlls\2015 ..\tfs-server-dlls\current
powershell -file setupinstaller.ps1 tfs2015