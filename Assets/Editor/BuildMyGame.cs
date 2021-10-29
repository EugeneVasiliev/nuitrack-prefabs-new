using System;
using System.Linq;
using UnityEditor;

using UnityEngine;
using System.IO;

namespace NuitrackSDKEditor
{
    public class BuildMyGame
    {
        public static void BuildAab()
        {
            SetProject();

            EditorUserBuildSettings.buildAppBundle = true;

            // Build player.
            BuildPipeline.BuildPlayer(GetScenes(), Environment.GetCommandLineArgs().Last(), BuildTarget.Android, BuildOptions.None);
        }

        //[MenuItem("MyTools/Linux Build")]
        public static void BuildLinux()
        {
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.allowDebugging = true;
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

        // This menu will be missing in the final unitypackage
        [MenuItem("Nuitrack/Developer/Create/NuitrackReadme")]
        public static void CreateAsset()
        {
            CreateAsset<Documentation.NuitrackTutorials>();
        }

        static void CreateAsset<T>() where T : ScriptableObject
        {
            T newAsset = ScriptableObject.CreateInstance<T>();

            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (assetPath.Equals("") == true)
            {
                assetPath = "Assets";
            }

            else if (Path.GetExtension(assetPath).Equals("") == false)
            {
                string filename = Path.GetFileName(assetPath);
                assetPath = assetPath.Replace(filename, "");
            }

            string assetFilename = "/Data" + typeof(T).ToString() + ".asset";
            string newAssetFullPath = AssetDatabase.GenerateUniqueAssetPath(assetPath + assetFilename);

            AssetDatabase.CreateAsset(newAsset, newAssetFullPath);

            // Save and Focus
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newAsset;
        }
    }
}