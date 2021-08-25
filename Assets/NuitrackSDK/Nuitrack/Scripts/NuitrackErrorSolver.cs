using UnityEngine;
using System.IO;
using System.Security.Permissions;
using System.Security.AccessControl;

public class NuitrackErrorSolver : MonoBehaviour
{
    public static void CheckError(System.Exception ex)
    {
#if UNITY_EDITOR
        string nuitrackHomePath = System.Environment.GetEnvironmentVariable("NUITRACK_HOME");
        string incorrectInstallingMessage = "Check your Environment Variables in Windows settings. " +
                "There should be two variables \"NUITRACK_HOME\" with a path to Nuitrack and a \"Path\" c with a path to %NUITRACK_HOME%/bin " +
                "Maybe the installation probably did not complete correctly, then look Troubleshooting Page.";
        if (nuitrackHomePath == null)
            Debug.LogError("<color=red><b>Check Environment Variable [NUITRACK_HOME]</b></color>" +
                "\n So what's now? 1. Is Nuitrack installed at all? 2.Try restart PC 3. " + incorrectInstallingMessage);
        else
        {
            string nuitrackModulePath = nuitrackHomePath + "\\middleware\\NuitrackModule.dll";
            if (!(File.Exists(nuitrackModulePath)))
            {
                Debug.LogError("<color=red><b>File: " + nuitrackModulePath + " not exists. Check Path in Environment Variable [NUITRACK_HOME]</b></color>. Nuitrack path is really: " + nuitrackHomePath + " ?" +
                    "\n So what's now? 1.Try restart PC 2. " + incorrectInstallingMessage);
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
                        Debug.LogError("<color=red><b>Check the read\\write permissions for the folder where Nuitrack Runtime is installed, as well as for all subfolders and files. And try allow all permissions.</b></color> Path: " + nuitrackHomePath);
                    else if(ex.ToString().Contains("Can't create DepthSensor"))
                        Debug.LogError("<color=red><b>Can't create DepthSensor module. Sensor connected?</b></color>");
                    else
                        Debug.LogError("<color=red><b>Perhaps the sensor is already being used in some other program.</b></color>");
                }
            }
        }

        if (ex.ToString().Contains("TBB"))
        {
            string unityTbbPath = UnityEditor.EditorApplication.applicationPath.Replace("Unity.exe", "") + "tbb.dll";
            string nuitrackTbbPath = nuitrackHomePath + "\\bin\\tbb.dll";
            Debug.LogError("<color=red><b>You need to replace the file " + unityTbbPath + " with Nuitrack compatible file " + nuitrackTbbPath + " (Don't forget to close the editor first)</b></color>");
        }
        else
        {
            Debug.LogError(ex.ToString());
        }

        Debug.LogError("<color=red><b>Also look Nuitrack Troubleshooting page:</b></color> https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Troubleshooting.md#windows");
#endif
    }
}
