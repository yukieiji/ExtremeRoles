Write-Host "Init Env..."

pip install -r requirements.txt
mkdir -Path ExtremeRoles/Resources -Force
mkdir -Path ExtremeSkins/Resources/Asset -Force

Write-Host "Start Build!!"

msbuild ExtremeRoles.sln -t:restore,rebuild -p:Configuration=Release -p:RestorePackagesConfig=true
mkdir -Path workspace -Force

Write-Host "Build Complete!!"


Write-Host "Download BepInEx...."
Invoke-WebRequest "https://builds.bepinex.dev/projects/bepinex_be/671/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.671%2B9caf61d.zip" -OutFile workspace/bepinex_x86.zip
Invoke-WebRequest "https://builds.bepinex.dev/projects/bepinex_be/671/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.671%2B9caf61d.zip" -OutFile workspace/bepinex_x64.zip
Expand-Archive -Path workspace/bepinex_x86.zip -DestinationPath workspace/bepinex_x86 -Force
Expand-Archive -Path workspace/bepinex_x64.zip -DestinationPath workspace/bepinex_x64 -Force


Write-Host "Create Packing..."
mkdir -Path workspace/bepinex_x86/BepInEx/config -Force
mkdir -Path workspace/bepinex_x86/BepInEx/plugins -Force


Write-Host "Create default package"
New-Item workspace/bepinex_x86/steam_appid.txt
Set-Content workspace/bepinex_x86/steam_appid.txt '945360'

Copy-Item -Path ExtremeRoles/Resources/Config/*.cfg -Destination workspace/bepinex_x86/BepInEx/config -Force -Recurse

Copy-Item -Path workspace/bepinex_x86 -Destination workspace/ExtremeRoles -Force -Recurse
Copy-Item -Path workspace/bepinex_x86 -Destination workspace/ExtremeRolesWithSkins -Force -Recurse
mkdir -Path workspace/dll -Force

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/ExtremeRoles/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/ExtremeRolesWithSkins/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse
Copy-Item -Path ExtremeSkins/bin/Release/net6.0/ExtremeSkins.dll -Destination workspace/ExtremeRolesWithSkins/BepInEx/plugins/ExtremeSkins.dll -Force -Recurse


Write-Host "Create MSStore package"
Copy-Item -Path ExtremeRoles/Resources/Config/*.cfg -Destination workspace/bepinex_x64/BepInEx/config -Force -Recurse

Copy-Item -Path workspace/bepinex_x64 -Destination workspace/MSStore_ExtremeRoles -Force -Recurse
Copy-Item -Path workspace/bepinex_x64 -Destination workspace/MSStore_ExtremeRolesWithSkins -Force -Recurse
mkdir -Path workspace/dll -Force

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/MSStore_ExtremeRoles/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/MSStore_ExtremeRolesWithSkins/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse
Copy-Item -Path ExtremeSkins/bin/Release/net6.0/ExtremeSkins.dll -Destination workspace/MSStore_ExtremeRolesWithSkins/BepInEx/plugins/ExtremeSkins.dll -Force -Recurse


Write-Host "Copy DLL"
Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/dll/ExtremeRoles.dll -Force -Recurse
Copy-Item -Path ExtremeSkins/bin/Release/net6.0/ExtremeSkins.dll -Destination workspace/dll/ExtremeSkins.dll -Force -Recurse
Copy-Item -Path ExtremeVoiceEngine/bin/Release/net6.0/ExtremeVoiceEngine.dll -Destination workspace/dll/ExtremeVoiceEngine.dll -Force -Recurse
Copy-Item -Path ExtremeRoles.Test/bin/Release/net6.0/ExtremeRoles.Test.dll -Destination workspace/dll/ExtremeRoles.Test.dll -Force -Recurse

