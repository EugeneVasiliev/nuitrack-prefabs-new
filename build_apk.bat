ECHO START
start cmd.exe /k "C:\Program Files\Unity2019.4\Editor\Unity.exe" -batchmode -quit -projectPath %cd% -logFile %cd%/build_log.txt -executeMethod BuildMyGame.BuildApk %cd%/NuitrackDemo.apk
ECHO FINISH