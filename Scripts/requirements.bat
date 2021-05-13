pushd .
cd C:\Users\stranger\AppData\Local\Programs\Python\Python*
set PYTHON_PATH=%cd%
popd

set DEPTH_TAG=Fittar_develop

%PYTHON_PATH%\Scripts\pip.exe install git+http://gitlab-ci-token:%CI_JOB_TOKEN%@tox.3divi.ru/3divi-devops/3divi-ctl-client.git@windows_fix

%PYTHON_PATH%\python.exe %PYTHON_PATH%\Scripts\3divi-ctl get --output ../depth_scanner/arm32 artifact://3d/depth_scanner/ubuntu-14.04.5-android-arm32:%DEPTH_TAG%
%PYTHON_PATH%\python.exe %PYTHON_PATH%\Scripts\3divi-ctl get --output ../depth_scanner/arm64 artifact://3d/depth_scanner/ubuntu-14.04.5-android-arm64:%DEPTH_TAG%
%PYTHON_PATH%\python.exe %PYTHON_PATH%\Scripts\3divi-ctl get --output ../depth_scanner/lin64 artifact://3d/depth_scanner/ai_ubuntu-14.04.5-linux-x64:%DEPTH_TAG%
%PYTHON_PATH%\python.exe %PYTHON_PATH%\Scripts\3divi-ctl get --output ../depth_scanner/ios64 artifact://3d/depth_scanner/macos-ios-arm64:%DEPTH_TAG%

copy /y "..\depth_scanner\arm64\depth_scanner\deployment\NuitrackSDK\Nuitrack\lib\android-arm64\*.jar"     "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly"

copy /y "..\depth_scanner\arm64\depth_scanner\build_android64\Wrappers\CSharp\Nuitrack\nuitrack.net.dll"   "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly/IL2CPP"
copy /y "..\depth_scanner\ios64\depth_scanner\build\Wrappers\CSharp\Nuitrack\nuitrack.net.dll"             "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly/iOS" 
copy /y "..\depth_scanner\lin64\depth_scanner\deployment\NuitrackSDK\Nuitrack\lib\csharp\nuitrack.net.dll" "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly"

copy /y "..\depth_scanner\arm64\depth_scanner\deployment\NuitrackSDK\Nuitrack\lib\android-arm64\*.so"      "Assets/Plugins/Android/libs/arm64-v8a"
copy /y "..\depth_scanner\arm32\depth_scanner\deployment\NuitrackSDK\Nuitrack\lib\android\*.so"            "Assets/Plugins/Android/libs/armeabi-v7a"

del /s "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly/nuitrackhelper.jar"
rmdir /s /q "..\depth_scanner"