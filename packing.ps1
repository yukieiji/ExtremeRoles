msbuild ExtremeRoles.sln -t:restore,build -p:Configuration=Release -p:RestorePackagesConfig=true
mkdir -Path workspace -Force

Write-Host "Build Complete!!"

Write-Host "Download BepInEx...."
Invoke-WebRequest "https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.667%2B6b500b3.zip" -OutFile workspace/bepinex.zip
Expand-Archive -Path workspace/bepinex.zip -DestinationPath workspace/bepinex -Force

Write-Host "Create Packing..."
mkdir -Path workspace/bepinex/BepInEx/config -Force
mkdir -Path workspace/bepinex/BepInEx/plugins -Force

Copy-Item -Path ExtremeRoles/Resources/Config/*.cfg -Destination workspace/bepinex/BepInEx/config -Force -Recurse

Copy-Item -Path workspace/bepinex -Destination workspace/ExtremeRoles -Force -Recurse
Copy-Item -Path workspace/bepinex -Destination workspace/ExtremeRolesWithSkins -Force -Recurse
mkdir -Path workspace/dll -Force

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/ExtremeRoles/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/ExtremeRolesWithSkins/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse
Copy-Item -Path ExtremeSkins/bin/Release/net6.0/ExtremeSkins.dll -Destination workspace/ExtremeRolesWithSkins/BepInEx/plugins/ExtremeSkins.dll -Force -Recurse

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/dll/ExtremeRoles.dll -Force -Recurse
Copy-Item -Path ExtremeSkins/bin/Release/net6.0/ExtremeSkins.dll -Destination workspace/dll/ExtremeSkins.dll -Force -Recurse
Copy-Item -Path ExtremeVoiceEngine/bin/Release/net6.0/ExtremeVoiceEngine.dll -Destination workspace/dll/ExtremeVoiceEngine.dll -Force -Recurse
