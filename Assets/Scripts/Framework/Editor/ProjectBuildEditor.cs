using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
public class ProjectBuildEditor : Editor
{
    static string targetPath;
    [PostProcessBuildAttribute(88)]
    public static void onPostProcessBuild(BuildTarget target, string targetPath)
    {
        string unityEditorAssetPath = Application.dataPath;

        if (target != BuildTarget.iOS)
        {
            return;
        }
        EditorSetting(targetPath);
    }

    private static void EditorSetting(string targetPath)
    {
        string projPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(targetPath);
        UnityEditor.iOS.Xcode.PBXProject proj = new UnityEditor.iOS.Xcode.PBXProject();
        proj.ReadFromFile(projPath);
        string targetGuid = proj.TargetGuidByName("Unity-iPhone");
        proj.AddFileToBuild(targetGuid, proj.AddFile("usr/lib/libc++.tbd", "libc++.tbd", PBXSourceTree.Sdk));
        proj.AddFileToBuild(targetGuid, proj.AddFile("usr/lib/libz.tbd", "libz.tbd", PBXSourceTree.Sdk));
        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        proj.WriteToFile(projPath);
        EditorPlistFile(targetPath);
    }

    private static void EditorPlistFile(string path)
    {
        PlistDocument plist = new PlistDocument();
        string plistpath = System.IO.Path.Combine(path, "info.plist");
        plist.ReadFromString(System.IO.File.ReadAllText(plistpath));
        PlistElementDict rootDict = plist.root;
        rootDict.SetString("NSBluetoothPeripheralUsageDescription", "需要蓝牙链接机器人");
        rootDict.SetString("NSLocationWhenInUseUsageDescription", "\"鲸鱼机器人\"需要您的同意，才能在使用期间访问位置 ，以便于为不同区域用户提供更精准的售后服务");
        rootDict.SetString("NSLocationAlwaysUsageDescription", "\"鲸鱼机器人\"需要您的同意，才能在使用期间访问位置 ，以便于为不同区域用户提供更精准的售后服务");
        rootDict.SetString("NSMicrophoneUsageDescription", "需要麦克风收听声音");
        rootDict.SetString("CFBundleDevelopmentRegion", "zh_CN");
        System.IO.File.WriteAllText(plistpath, plist.WriteToString());
    }

    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    static string GetBuildPath(BuildTarget buildType)
    {
        string projectPath = System.IO.Path.GetFullPath(".").ToForwardSlash().ToSingleForwardSlash();
        string dirPath = projectPath;
        switch (buildType)
        {
            case BuildTarget.iOS:
                dirPath = projectPath + "/../build/iPhone/";
                break;
            case BuildTarget.Android:
                dirPath = projectPath + "/../build/android/";
                break;
            case BuildTarget.StandaloneWindows:
                dirPath = projectPath + "/../build/windows/";
                break;
        }
        PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by10 | AspectRatio.Aspect16by9, true);
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        return dirPath;
    }

    static void BuildSetting(BuildTargetGroup targetGroup, List<string> defineSymbols, System.Action callback, bool force = false, bool isgoogleplay = false)
    {
        AssetBundleCreatorEditor.BuildAll(() =>
        {
            AssetDatabase.Refresh();
            CSObjectWrapEditor.Generator.GenAll();
            AssetDatabase.Refresh();
            AssetBundleCreatorEditor.CopyToStreamAsset();
            AssetBundleCreatorEditor.DelResourcesPrefab(); //删除Resources下面的 prefab
            PlayerSettings.keyaliasPass = "a21211321";
            PlayerSettings.keystorePass = "a21211321";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));
            PlayerSettings.bundleVersion = GameSettingUtil.Version;
            PlayerSettings.Android.bundleVersionCode = int.Parse(GameSettingUtil.Versioncode);
            PlayerSettings.iOS.buildNumber = GameSettingUtil.Versioncode;
            PlayerSettings.runInBackground = true;
            PlayerSettings.applicationIdentifier = GameSettingUtil.Idf;
            if (isgoogleplay)
            {
                PlayerSettings.Android.useAPKExpansionFiles = true;
            }
            else
            {
                PlayerSettings.Android.useAPKExpansionFiles = false;
            }
            if (callback != null)
            {
                callback();
            }
        }, force);
    }

    [MenuItem("XYFramework/出包/Win/1.编译exe")]
    public static void BuildForExe()
    {
        List<string> defineSymbols = new List<string>();
        // defineSymbols.Add("HOTFIX_ENABLE"); //支持lua 热修复
        defineSymbols.Add("Debug");//Debug 环境 打印log
        defineSymbols.Add("OutLog");//可以到处当前的日志为文件
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
        BuildSetting(targetGroup, defineSymbols, () =>
        {
            string path = GetBuildPath(BuildTarget.StandaloneWindows) + "app.exe";
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.StandaloneWindows, BuildOptions.CompressWithLz4);
        }, false);

    }

    [MenuItem("XYFramework/出包/Android/1.测试版android")]
    public static void BuildForAndroid()
    {
        List<string> defineSymbols = new List<string>();
        // defineSymbols.Add("HOTFIX_ENABLE"); //支持lua 热修复
        defineSymbols.Add("Debug");//Debug 环境 打印log
        defineSymbols.Add("OutLog");//可以到处当前的日志为文件
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        BuildSetting(targetGroup, defineSymbols, () =>
        {
            string path = GetBuildPath(BuildTarget.Android) + "app.apk";
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.CompressWithLz4);
        }, false);

    }

    [MenuItem("XYFramework/出包/Android/2.正式版Android")]
    public static void BuildForReleaseAndroid()
    {
        List<string> defineSymbols = new List<string>();
        // defineSymbols.Add("HOTFIX_ENABLE"); //支持lua 热修复
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        BuildSetting(targetGroup, defineSymbols, () =>
        {
            string path = GetBuildPath(BuildTarget.Android) + "app " + GameSettingUtil.Version + "_" + GameSettingUtil.Versioncode + ".apk";
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.CompressWithLz4);
        }, true);

    }
    [MenuItem("XYFramework/出包/Android/2.1正式版Android(googleplay)")]
    public static void BuildForReleaseAndroid_Googleplay()
    {
        List<string> defineSymbols = new List<string>();
        // defineSymbols.Add("HOTFIX_ENABLE"); //支持lua 热修复
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        BuildSetting(targetGroup, defineSymbols, () =>
        {
            string filename = "app_googleplay " + GameSettingUtil.Version + "_" + GameSettingUtil.Versioncode;
            string path = GetBuildPath(BuildTarget.Android) + filename + ".apk";
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.CompressWithLz4);
            string nowobbpath = GetBuildPath(BuildTarget.Android) + filename + ".main.obb";
            if (File.Exists(nowobbpath))
            {
                File.Copy(nowobbpath, GetBuildPath(BuildTarget.Android) + "main." + GameSettingUtil.Versioncode + "." + GameSettingUtil.Idf + ".obb");
                File.Delete(nowobbpath);
            }
        }, true, true);

    }

    [MenuItem("XYFramework/出包/Android/3.使用当前资源直接打包APK文件(资源必须提前打包完成)")]
    public static void BuildForApk()
    {
        List<string> defineSymbols = new List<string>();
        // defineSymbols.Add("HOTFIX_ENABLE"); //支持lua 热修复
        defineSymbols.Add("Debug");//Debug 环境 打印log
        defineSymbols.Add("OutLog");//可以到处当前的日志为文件
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        CSObjectWrapEditor.Generator.GenAll();
        AssetDatabase.Refresh();
        AssetBundleCreatorEditor.CopyToStreamAsset();
        AssetBundleCreatorEditor.DelResourcesPrefab(); //删除Resources下面的 prefab
        PlayerSettings.keyaliasPass = "a21211321";
        PlayerSettings.keystorePass = "a21211321";
        PlayerSettings.bundleVersion = GameSettingUtil.Version;
        PlayerSettings.iOS.buildNumber = GameSettingUtil.Versioncode;
        PlayerSettings.runInBackground = true;
        PlayerSettings.applicationIdentifier = GameSettingUtil.Idf;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));
        string path = GetBuildPath(BuildTarget.Android) + "app.apk";
        BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.CompressWithLz4);
    }

    [MenuItem("XYFramework/出包/IOS/1.导出测试版IOS工程")]
    public static void BuildForIOS()
    {
        List<string> defineSymbols = new List<string>();
        // defineSymbols.Add("HOTFIX_ENABLE"); //支持lua 热修复
        defineSymbols.Add("Debug");//Debug 环境 打印log
        defineSymbols.Add("OutLog");//可以到处当前的日志为文件
        BuildTargetGroup targetGroup = BuildTargetGroup.iOS;
        BuildSetting(targetGroup, defineSymbols, () =>
        {
            string path = GetBuildPath(BuildTarget.iOS);
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iOS, BuildOptions.CompressWithLz4);
        }, false);

    }

    [MenuItem("XYFramework/出包/IOS/2.导出正式版IOS工程")]
    public static void BuildForRealIOS()
    {
        List<string> defineSymbols = new List<string>();
        // defineSymbols.Add("HOTFIX_ENABLE"); //支持lua 热修复
        // defineSymbols.Add("Debug");//Debug 环境 打印log
        // defineSymbols.Add("OutLog");//可以到处当前的日志为文件
        BuildTargetGroup targetGroup = BuildTargetGroup.iOS;
        BuildSetting(targetGroup, defineSymbols, () =>
        {
            string path = GetBuildPath(BuildTarget.iOS);
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iOS, BuildOptions.CompressWithLz4);
        }, true);

    }

}
