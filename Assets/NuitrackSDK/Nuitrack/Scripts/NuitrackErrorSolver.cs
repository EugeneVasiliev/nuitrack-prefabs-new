using UnityEngine;
using System.IO;
using System.Security.Permissions;
using System.Security.AccessControl;

public class NuitrackErrorSolver : MonoBehaviour
{
    public static string CheckError(System.Exception ex, bool showInLog = true)
    {
        string errorMessage = string.Empty;
        string troubleshootingPageMessage = "<color=red><b>Also look Nuitrack Troubleshooting page:</b></color>github.com/3DiVi/nuitrack-sdk/blob/master/doc/Troubleshooting.md" +
            "\nIf all else fails and you decide to contact our technical support, do not forget to attach the Unity Editor Log File (in %LOCALAPPDATA%/Unity/Editor) and specify the Nuitrack version";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN 
        string nuitrackHomePath = System.Environment.GetEnvironmentVariable("NUITRACK_HOME");
        string incorrectInstallingMessage =
            "1.Is Nuitrack installed at all? (github.com/3DiVi/nuitrack-sdk/tree/master/Platforms) \n" +
            "2.Try restart PC \n" +
            "3.Check your Environment Variables in Windows settings. " +
            "There should be two variables \"NUITRACK_HOME\" with a path to \"Nuitrack\\nuitrack\\nutrack\" and a \"Path\" with a path to %NUITRACK_HOME%\\bin " +
            "Maybe the installation probably did not complete correctly, in this case, look Troubleshooting Page.";
        string accessDeniedMessage = "Check the read\\write permissions for the folder where Nuitrack Runtime is installed, as well as for all subfolders and files. " +
                    "Can you create text-file in " + nuitrackHomePath + "\\middleware folder?" + " Try allow Full controll permissions for Users. " +
                    "(More details: winaero.com/how-to-take-ownership-and-get-full-access-to-files-and-folders-in-windows-10/)";

#endif
#if UNITY_EDITOR_WIN
        if (ex.ToString().Contains("TBB"))
        {
            string unityTbbPath = UnityEditor.EditorApplication.applicationPath.Replace("Unity.exe", "") + "tbb.dll";
            string nuitrackTbbPath = nuitrackHomePath + "\\bin\\tbb.dll";
            errorMessage = "<color=red><b>You need to replace the file " + unityTbbPath + " with Nuitrack compatible file " + nuitrackTbbPath + " (Don't forget to close the editor first)</b></color>";
        }
        else
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (nuitrackHomePath == null)
            errorMessage = "<color=red><b>" + "Environment Variable [NUITRACK_HOME] not found" + "</b></color>" +
                "\n" + incorrectInstallingMessage;
        else
        {
            string nuitrackModulePath = nuitrackHomePath + "\\middleware\\NuitrackModule.dll";
            if (!(File.Exists(nuitrackModulePath)))
            {
                errorMessage = "<color=red><b>" + "File: " + nuitrackModulePath + " not exists or Unity doesn't have enough rights to access it." + "</b></color>" + " Nuitrack path is really: " +
                    nuitrackHomePath + " ?\n" + incorrectInstallingMessage + "\n" +
                    "4." + accessDeniedMessage;
            }
            else
            {
                try
                {
                    new FileIOPermission(FileIOPermissionAccess.Read, AccessControlActions.View, nuitrackModulePath);
                }
                catch (System.Exception)
                {
                    if (ex.ToString().Contains("Cannot load library module"))
                        errorMessage = accessDeniedMessage +
                            "Path: " + nuitrackHomePath;
                    else if (ex.ToString().Contains("Can't create DepthSensor"))
                        errorMessage = "Can't create DepthSensor module. Sensor connected? Is the connection stable? Are the wires okay? \nTry start " + nuitrackHomePath + "\\bin\\nuitrack_sample.exe";
                    else if (ex.ToString().Contains("System.DllNotFoundException: libnuitrack"))
                        errorMessage = "Perhaps installed Nuitrack Runtime version for x86 (nuitrack-windows-x86.exe), in this case, install x64 version (github.com/3DiVi/nuitrack-sdk/blob/master/Platforms/nuitrack-windows-x64.exe)";
                    else
                        errorMessage = "Perhaps the sensor is already being used in other program. \nIf not, try start " + nuitrackHomePath + "\\bin\\nuitrack_sample.exe";

                    errorMessage = "<color=red><b>" + errorMessage + "</b></color>";
                }
            }
        }

        if (showInLog) Debug.LogError(errorMessage);
#endif
        if (showInLog && Application.loadedLevelName != "allModulesScene") Debug.LogError("<color=red><b>It is recommended to test on allModulesScene</b></color");
        if (showInLog) Debug.LogError(troubleshootingPageMessage);
        if (showInLog) Debug.LogError(ex.ToString());

        return (errorMessage + "\n" + troubleshootingPageMessage);
    }
}
