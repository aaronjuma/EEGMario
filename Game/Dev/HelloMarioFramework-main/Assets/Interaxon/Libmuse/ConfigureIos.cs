#if UNITY_IOS && UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class ConfigureIOS
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            try
            {
                var projPath = PBXProject.GetPBXProjectPath(path);
                PBXProject proj = new PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));

                var target = proj.GetUnityFrameworkTargetGuid();
                proj.AddFrameworkToProject(target, "CoreBluetooth.framework", false);
                proj.AddFrameworkToProject(target, "ExternalAccessory.framework", false);

                proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(SRCROOT)/Frameworks/Plugins/iOS/Muse.framework/Headers");
                File.WriteAllText(projPath, proj.WriteToString());

                var plistPath = path + "/Info.plist";
                var plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));
                plist.root.SetString("NSBluetoothAlwaysUsageDescription", "This app needs access to Bluetooth to connect to Muse devices.");
                File.WriteAllText(plistPath, plist.WriteToString());
            }
            catch (Exception ex)
            {
                Debug.Log("Error configuring iOS: " + ex.Message);
            }
        }
    }
}

#endif
