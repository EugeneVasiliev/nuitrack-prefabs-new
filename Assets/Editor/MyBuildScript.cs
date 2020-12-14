using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MyBuildScript
{
    [MenuItem("Demo/Test build")]
    public static void Test() {
        SetEnvironmentVariable("CI_COMMIT_MESSAGE","Test");
        SetEnvironmentVariable("CI_JOB_ID","0");
        SetEnvironmentVariable("CI_COMMIT_TAG","0.0.1");
        SetEnvironmentVariable("BUILD_TARGET",BuildTarget.StandaloneWindows64.ToString());
        SetEnvironmentVariable("BUILD_PREFIX","demo");
        SetEnvironmentVariable("OUTPUT_DIR","build");
        MyBuildProcess();
    }

    public static void MyBuildProcess() {
        var settings = UpdateSettings();
        var buildPostfix = settings.commitRef.Replace("/","_");
        var buildPrefix = string.Format("{0}_{1}", GetEnvironmentVariable("BUILD_PREFIX"),buildPostfix);        
        var buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget),GetEnvironmentVariable("BUILD_TARGET"));                
        string[] scenes = new string[] { "Assets/Scenes/Main.unity" };
        string path = null;
        switch(buildTarget) {
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneWindows:
                path = Path.Combine(GetEnvironmentVariable("OUTPUT_DIR"),buildPrefix,"Demo.exe");
                break;
            default:
                //TODO error handling
                return;
        }        
        var report = BuildPipeline.BuildPlayer(scenes,path,buildTarget,BuildOptions.None);
        if(Application.isBatchMode) {
            EditorApplication.Exit(report.summary.totalErrors > 0 ? 1 : 0);
        }
    }

    private static MySettings UpdateSettings() {
        //Custom settings
        var settings = AssetDatabase.LoadAssetAtPath<MySettings>("Assets/Resources/MySettings.asset");        
        settings.commitRef      = GetEnvironmentVariable("CI_COMMIT_REF_NAME");        
        settings.jobId          = GetEnvironmentVariable("CI_JOB_ID");
        settings.pipelineId     = GetEnvironmentVariable("CI_PIPELINE_ID");
        settings.commitHash     = GetEnvironmentVariable("CI_COMMIT_SHA");
        settings.commitMessage  = GetEnvironmentVariable("CI_COMMIT_MESSAGE");
        settings.backendBaseURL = GetEnvironmentVariable("BACKEND_BASE_URL");        
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        //Player settings
        var tag = GetEnvironmentVariable("CI_COMMIT_TAG");
        PlayerSettings.bundleVersion = string.IsNullOrWhiteSpace(tag) ? "0.0.1" : tag;
        return settings;
    }

    private static string GetEnvironmentVariable(string name) {
        return Environment.GetEnvironmentVariable(name,EnvironmentVariableTarget.Process);
    }

    private static void SetEnvironmentVariable(string name,string value) {
        Environment.SetEnvironmentVariable(name,value,EnvironmentVariableTarget.Process);
    }
}
