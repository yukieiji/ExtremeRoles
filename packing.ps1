msbuild ExtremeRoles.sln -t:restore,build -p:Configuration=Release -p:RestorePackagesConfig=true
mkdir -Path workspace -Force

Invoke-WebRequest "https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.667%2B6b500b3.zip" -OutFile workspace/bepinex.zip
Expand-Archive -Path workspace/bepinex.zip -DestinationPath workspace/bepinex -Force

mkdir -Path workspace/bepinex/BepInEx/config -Force
mkdir -Path workspace/bepinex/BepInEx/plugins -Force

Copy-Item -Path ExtremeRoles/Resources/Config -Destination workspace/bepinex/BepInEx/config -Force -Recurse

Copy-Item -Path workspace/bepinex -Destination workspace/ExtremeRoles -Force -Recurse
Copy-Item -Path workspace/bepinex -Destination workspace/ExtremeRolesWithSkins -Force -Recurse

mkdir -Path release -Force

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/ExtremeRoles/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse
Compress-Archive -DestinationPath release/ExtremeRoles.zip -Path workspace/ExtremeRoles

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination workspace/ExtremeRolesWithSkins/BepInEx/plugins/ExtremeRoles.dll -Force -Recurse
Copy-Item -Path ExtremeSkins/bin/Release/net6.0/ExtremeSkins.dll -Destination workspace/ExtremeRolesWithSkins/BepInEx/plugins/ExtremeSkins.dll -Force -Recurse
Compress-Archive -DestinationPath release/ExtremeRolesWithSkin.zip -Path workspace/ExtremeRolesWithSkins

Copy-Item -Path ExtremeRoles/bin/Release/net6.0/ExtremeRoles.dll -Destination release/ExtremeRoles.dll -Force -Recurse
Copy-Item -Path ExtremeSkins/bin/Release/net6.0/ExtremeSkins.dll -Destination release/ExtremeSkins.dll -Force -Recurse
Copy-Item -Path ExtremeVoiceEngine/bin/Release/net6.0/ExtremeVoiceEngine.dll -Destination release/ExtremeVoiceEngine.dll -Force -Recurse