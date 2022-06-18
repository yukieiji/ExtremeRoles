@echo off
echo "Killing AmongUs......"
taskkill /im "Among Us.exe"

echo "Waiting 10 seconds"
powershell sleep 10

echo "Remove Old BepInEx......"
rd /s /q %2\BepInEx\core\
rd /s /q %2\BepInEx\unhollowed\
rd /s /q %2\BepInEx\unity-libs\
rd /s /q %2\mono\
del /q %2\changelog.txt
del /q %2\doorstop_config.ini
del /q %2\winhttp.dll

echo "BepInEx Installing...."
xcopy /q /s /e /c /-y %1 %2

echo "Update Complete!! Please Restart Game"
pause