pushd .
cd C:\Users\stranger\AppData\Local\Programs\Python\Python*
set PYTHON_PATH=%cd%
popd

%PYTHON_PATH%\Scripts\pip.exe install git+http://gitlab-ci-token:%CI_JOB_TOKEN%@tox.3divi.ru/3divi-devops/3divi-ctl-client.git@windows_fix

%PYTHON_PATH%\python.exe %PYTHON_PATH%\Scripts\3divi-ctl get --output ../depth_scanner/arm32 artifact://3d/depth_scanner/ubuntu-14.04.5-android-arm32:Fittar_v0.35.1_v2.1
%PYTHON_PATH%\python.exe %PYTHON_PATH%\Scripts\3divi-ctl get --output ../depth_scanner/arm64 artifact://3d/depth_scanner/ubuntu-14.04.5-android-arm64:Fittar_v0.35.1_v2.1

copy /y "..\depth_scanner\arm64\depth_scanner\deployment\NuitrackSDK\Nuitrack\lib\android-arm64\*.so" "Assets/Plugins/Android/libs/arm64-v8a"
copy /y "..\depth_scanner\arm32\depth_scanner\deployment\NuitrackSDK\Nuitrack\lib\android\*.so" "Assets/Plugins/Android/libs/armeabi-v7a"
rmdir /s /q "..\depth_scanner"