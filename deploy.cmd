echo start
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -createManualActivationFile
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -executeMethod BuildMyGame.BuildApk NuitrackDemo.apk -logFile log.txt -force-free
rem "C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath . -exportPackage "Assets/NuitrackSDK" "Assets/Plugins" NuitrackSDK.unitypackage
echo finish