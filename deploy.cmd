echo start
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -manualLicenseFile Unity_v2020.x.ulf
rem "C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -executeMethod BuildMyGame.BuildApk NuitrackDemo.apk -logFile log.txt
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -exportPackage "Assets/NuitrackSDK" "Assets/Plugins" NuitrackSDK.unitypackage -logFile log.txt
echo finish