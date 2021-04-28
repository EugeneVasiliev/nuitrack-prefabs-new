using System;
using System.Linq;
using UnityEditor;

public class BuildMyGame
{

    //[MenuItem("MyTools/Android Build With Postprocess")]
    public static void BuildAab()
    {
        SetProject();

        EditorUserBuildSettings.buildAppBundle = true;

        // Build player.
        BuildPipeline.BuildPlayer(GetScenes(), Environment.GetCommandLineArgs().Last(), BuildTarget.Android, BuildOptions.None);
    }

    public static void BuildLinux()
    {
        // Build player.
        BuildPipeline.BuildPlayer(GetScenes(), Environment.GetCommandLineArgs().Last(), BuildTarget.StandaloneLinux64, BuildOptions.None);
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

        EditorPrefs.SetString("AndroidSdkRoot", "C:\\AndroidSDK");
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
    }

    static string[] GetScenes()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }
}