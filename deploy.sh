echo start building
"C:\Program Files\Unity2019.4\Editor\Unity.exe" -batchmode -quit -projectPath . -executeMethod BuildMyGame.BuildAab NuitrackDemo.aab
"C:\Program Files\Unity2019.4\Editor\Unity.exe" -batchmode -quit -projectPath . -executeMethod BuildMyGame.BuildApk NuitrackDemo.apk
"C:\Program Files\Unity2019.4\Editor\Unity.exe" -batchmode -quit -projectPath . -exportPackage "Assets/NuitrackSDK" "Assets/Plugins" NuitrackSDK.unitypackage
echo finish building