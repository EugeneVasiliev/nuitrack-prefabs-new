@echo off
echo start
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -executeMethod BuildMyGame.BuildApk NuitrackDemo.apk
echo finish