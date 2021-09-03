using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System;

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
                Process app = new Process();
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
            UnityEngine.Debug.LogError("Unable to launch app: " + e.Message);
        }
    }
}