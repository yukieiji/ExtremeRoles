chcp 65001

taskkill /im "Among Us.exe"

powershell sleep 10

rd /s /q %2\BepInEx\core\
rd /s /q %2\BepInEx\unhollowed\
rd /s /q %2\BepInEx\unity-libs\
rd /s /q %2\mono\
del /q %2\changelog.txt
del /q %2\doorstop_config.ini
del /q %2\winhttp.dll

xcopy /q /s /e /c /-y %1 %2