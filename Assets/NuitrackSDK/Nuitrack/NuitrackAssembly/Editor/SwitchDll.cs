using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public class SwitchDll : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    static string pathAndroidIl2cpp = "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly/IL2CPP/nuitrack.net.dll";
    static string pathAndroidMono = "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly/nuitrack.net.dll";
    static string pathIOS = "Assets/NuitrackSDK/Nuitrack/NuitrackAssembly/IOS/nuitrack.net.dll";

    static SwitchDll()
    {
        Check();
    }

    [MenuItem("Nuitrack/Auto switch dll")]
    public static void Check()
    {
        PluginImporter pluginAndroidIl2cpp = (PluginImporter)PluginImporter.GetAtPath(pathAndroidIl2cpp);
        PluginImporter pluginAndroidMono = (PluginImporter)PluginImporter.GetAtPath(pathAndroidMono);
        PluginImporter pluginIOS = (PluginImporter)PluginImporter.GetAtPath(pathIOS);

        if (pluginAndroidIl2cpp == null)
        {
            Debug.LogError("Il2cpp Dll not found: " + pathAndroidIl2cpp);
            return;
        }

        if (pluginAndroidMono == null)
        {
            Debug.LogError("Mono Dll not found: " + pathAndroidMono);
            return;
        }

        if (pluginIOS == null)
        {
            Debug.LogError("IOS Dll not found: " + pathIOS);
            return;
        }

        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidIl2cpp, false, false);
            SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidMono, false, false);
            SwitchDll.SwitchCompatibleWithPlatform(pluginIOS, false, true);
        }
        else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            Debug.Log("Current Scripting Backend " + PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android));

            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP)
            {
                SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidIl2cpp, true, false);
                SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidMono, false, false);
                SwitchDll.SwitchCompatibleWithPlatform(pluginIOS, false, false);
            }
            else
            {
                SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidIl2cpp, false, false);
                SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidMono, true, false);
                SwitchDll.SwitchCompatibleWithPlatform(pluginIOS, false, false);
            }
        }
        else
        {
            SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidIl2cpp, false, false);
            SwitchDll.SwitchCompatibleWithPlatform(pluginAndroidMono, true, false);
            SwitchDll.SwitchCompatibleWithPlatform(pluginIOS, false, false);
        }
    }

    public static void SwitchCompatibleWithPlatform(PluginImporter plugin, bool value, bool iosValue)
    {
        if (value || iosValue)
            Debug.Log("Platform " + EditorUserBuildSettings.activeBuildTarget + ". Switch Nuitrack dll to " + plugin.assetPath);

        plugin.SetCompatibleWithAnyPlatform(false);
        plugin.SetCompatibleWithPlatform(BuildTarget.iOS, iosValue);
        plugin.SetCompatibleWithPlatform(BuildTarget.Android, value);
        plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, value);
        plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, value);
        plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, value);
        plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, value);
        plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, value);
        plugin.SetCompatibleWithEditor(value);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        Check();
    }
}
