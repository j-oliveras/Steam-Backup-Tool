@echo off
echo Please Make sure that Steam Backup Tool has exited before continuing
TIMEOUT /T 30

cd rsc
7za x "update.7z" -o"..\" -aoa

del "update.7z" 

pause

cd ..\
start "" "steamBackup.exe"