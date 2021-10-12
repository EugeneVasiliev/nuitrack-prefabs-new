@echo off
echo start
mkdir build
"C:\Editors\2020.3.1f1\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -executeMethod BuildMyGame.BuildLinux "build\Nui_Test.x86_64"
powershell Compress-Archive build linux_build.zip

echo finish