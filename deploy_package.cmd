@echo off
echo start
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -exportPackage "Assets/NuitrackSDK" "Assets/Plugins" NuitrackSDK.unitypackage
echo finish