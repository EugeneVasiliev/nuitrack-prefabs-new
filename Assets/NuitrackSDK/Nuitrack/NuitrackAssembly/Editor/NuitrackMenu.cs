using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

[InitializeOnLoad]
public class NuitrackMenu : MonoBehaviour
{
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
        Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/");
    }

    [MenuItem("Nuitrack/Help/Manage Nuitrack License", priority = 2)]
    public static void GoToLicensePage()
    {
        Application.OpenURL("https://cognitive.3divi.com");
    }

    [MenuItem("Nuitrack/Open Nuitrack Activation Tool", priority = 1)]
    public static void OpenNuitrackApp()
    {
        string nuitrackHomePath = System.Environment.GetEnvironmentVariable("NUITRACK_HOME");
        string path = Path.Combine(nuitrackHomePath, "activation_tool", "Nuitrack.exe");
        if (nuitrackHomePath != null && File.Exists(path))
        {
            Process nuitrackApp = Process.Start(path);
            nuitrackApp.WaitForExit();
            nuitrackApp.Close();
        }
        else
        {
            EditorUtility.DisplayDialog("Nuitrack.exe not found", path + " not found!", "ОК");
        }
    }
}