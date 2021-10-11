echo start
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -manualLicenseFile Unity_v2020.x.ulf
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -executeMethod BuildMyGame.BuildApk NuitrackDemo.apk -logFile log.txt
rem "C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath . -exportPackage "Assets/NuitrackSDK" "Assets/Plugins" NuitrackSDK.unitypackage
echo finish