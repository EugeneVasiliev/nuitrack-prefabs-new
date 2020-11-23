using System;
using System.Linq;
using UnityEditor;

public class BuildMyGame
{
    static string androidSDKPath = "C:\\NVPACK\\dk1.8.0_77";
    static string androidNDKPath = "C:\\android-ndk-r19c";
    static string androidJDKPath = "C:\\Program Files\\Java\\jdk1.8.0_181";

    //[MenuItem("MyTools/Android Build With Postprocess")]
    public static void BuildAab()
    {
        SetProject();

        EditorUserBuildSettings.buildAppBundle = true;

        // Build player.
        BuildPipeline.BuildPlayer(GetScenes(), Environment.GetCommandLineArgs().Last(), BuildTarget.Android, BuildOptions.None);
    }

    public static void BuildApk()
    {
        SetProject();

        EditorUserBuildSettings.buildAppBundle = false;

        // Build player.
        BuildPipeline.BuildPlayer(GetScenes(), Environment.GetCommandLineArgs().Last(), BuildTarget.Android, BuildOptions.None);
    }

    static void SetProject()
    {
        //Ключ
        PlayerSettings.Android.keystoreName = "C:\\KeyStore\\my-release-key.keystore";
        PlayerSettings.Android.keystorePass = "q2w3e4r";
        PlayerSettings.Android.keyaliasName = "main_key";
        PlayerSettings.Android.keyaliasPass = "q2w3e4r";

        EditorPrefs.SetString("AndroidSdkRoot", androidSDKPath);
        EditorPrefs.SetString("AndroidNdkRoot", androidNDKPath);
        EditorPrefs.SetString("JdkPath", androidJDKPath);
        //EditorPrefs.SetString("AndroidNdkRootR16b", androidNDKPath);

        EditorPrefs.SetString("AndroidSdkRoot", "C:\\AndroidSDK");
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
    }

    static string[] GetScenes()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }
}