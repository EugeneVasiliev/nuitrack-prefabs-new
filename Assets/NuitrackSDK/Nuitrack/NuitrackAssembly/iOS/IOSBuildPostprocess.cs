#if UNITY_EDITOR
#if UNITY_IOS

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class IOSBuildPostprocess
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild( BuildTarget buildTarget, string path)
    {
        if(buildTarget == BuildTarget.iOS)
        {
            // Update configuration
            string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string target = pbxProject.TargetGuidByName("Unity-iPhone");

            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            pbxProject.AddBuildProperty(target, "OTHER_LDFLAGS", "-Xlinker -export_dynamic");

            pbxProject.AddFrameworkToProject(target,"AVFoundation.framework", false);
            pbxProject.AddFrameworkToProject(target,"VideoToolbox.framework", false);
            pbxProject.AddFrameworkToProject(target,"MetalPerformanceShaders.framework", false);
            pbxProject.AddFrameworkToProject(target,"CoreMedia.framework", false);
            pbxProject.AddFrameworkToProject(target,"ExternalAccessory.framework", false);
            pbxProject.AddFrameworkToProject(target,"CoreMotion.framework", false);
            pbxProject.AddFrameworkToProject(target,"CoreImage.framework", false);
            pbxProject.AddFrameworkToProject(target,"Accelerate.framework", false);
            pbxProject.AddFrameworkToProject(target,"ImageIO.framework", false);
            pbxProject.AddFrameworkToProject(target,"UIKit.framework", false);
            pbxProject.AddFrameworkToProject(target,"libz.tbd", false);

            pbxProject.WriteToFile (projectPath);

            // Update plist
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;

            PlistElementArray bgModes = rootDict.CreateArray("UISupportedExternalAccessoryProtocols");
            bgModes.AddString("io.structure.depth");
            bgModes.AddString("io.structure.infrared");
            bgModes.AddString("io.structure.control");

            File.WriteAllText(plistPath, plist.WriteToString());

        }
    }
}

#endif
#endif
