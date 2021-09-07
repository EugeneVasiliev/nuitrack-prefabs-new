using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace NuitrackSDKEditor
{
    [InitializeOnLoad]
    public class NuitrackMenu : MonoBehaviour
    {
        static string nuitrackScriptsPath = "Assets/NuitrackSDK/Nuitrack/Prefabs/NuitrackScripts.prefab";

        [MenuItem("Nuitrack/Add to scene and Configurate")]
        public static void AddNuitrackToScene()
        {
            UnityEngine.Object nuitrackScriptsPrefab = AssetDatabase.LoadAssetAtPath(nuitrackScriptsPath, typeof(GameObject));

            if (nuitrackScriptsPrefab == null)
                Debug.LogAssertion(string.Format("Prefab NuitrackScripts was not found at {0}", nuitrackScriptsPrefab));
            else
            {
                NuitrackManager nuitrackManager = FindObjectOfType<NuitrackManager>();

                if (nuitrackManager != null)
                {
                    EditorGUIUtility.PingObject(nuitrackManager);
                    Debug.LogWarning("NuitrackManager already exists on the scene.");
                }
                else
                {
                    UnityEngine.Object nuitrackScripts = PrefabUtility.InstantiatePrefab(nuitrackScriptsPrefab);
                    Undo.RegisterCreatedObjectUndo(nuitrackScripts, string.Format("Create object {0}", nuitrackScripts.name));
                    Selection.activeObject = nuitrackScripts;
                }
            }
        }

        [MenuItem("Nuitrack/Help/Open Tutorials Page", priority = 2)]
        public static void GoToDocsPage()
        {
            Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/");
        }

        [MenuItem("Nuitrack/Help/Open Troubleshooting Page", priority = 2)]
        public static void GoToTroubleshootingPage()
        {
            Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Troubleshooting.md#troubleshooting");
        }

        [MenuItem("Nuitrack/Help/Open Github Page", priority = 2)]
        public static void GoToGithubPage()
        {
            Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/");
        }

        [MenuItem("Nuitrack/Help/Manage Nuitrack License", priority = 2)]
        public static void GoToLicensePage()
        {
            Application.OpenURL("https://cognitive.3divi.com");
        }

        [MenuItem("Nuitrack/Open Nuitrack Activation Tool", priority = 1)]
        public static void OpenNuitrackApp()
        {
            string nuitrackHomePath = Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string workingDir = Path.Combine(nuitrackHomePath, "activation_tool");
            string path = Path.Combine(workingDir, "Nuitrack.exe");

            if (nuitrackHomePath != null)
                RunProgram(path, workingDir);
        }

        [MenuItem("Nuitrack/Open Nuitrack Test Sample", priority = 1)]
        public static void OpenNuitrackTestSample()
        {
            string nuitrackHomePath = Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string workingDir = Path.Combine(nuitrackHomePath, "bin");
            string path = Path.Combine(workingDir, "nuitrack_sample.exe");

            if (nuitrackHomePath != null)
                RunProgram(path, workingDir);
        }

        static void RunProgram(string appPath, string workingDirectory)
        {
            try
            {
                if (File.Exists(appPath))
                {
                    System.Diagnostics.Process app = new System.Diagnostics.Process();
                    app.StartInfo.FileName = appPath;
                    app.StartInfo.WorkingDirectory = workingDirectory;
                    app.Start();
                    app.WaitForExit();
                    app.Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Program not found", appPath + " not found!", "ОК");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to launch app: " + e.Message);
            }
        }
    }
}