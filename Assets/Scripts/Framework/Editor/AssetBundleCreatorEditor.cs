using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 打包bundle工具
/// </summary>
public class AssetBundleCreatorEditor : Editor
{
    /// <summary>
    /// 获取当前打包资源的平台
    /// </summary>
    /// <returns></returns>
    private static BuildTarget GetTarget()
    {
        BuildTarget target;
#if UNITY_IOS
        target = BuildTarget.iOS;
#elif UNITY_ANDROID
        target = BuildTarget.Android;
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
        target = BuildTarget.StandaloneWindows;
#endif
        return target;
    }

    /// <summary>
    /// 通常忽略列表
    /// </summary>
    private static List<string> commonIgnoreLs = new List<string>() { ".cs", ".meta" };

    /// <summary>
    /// UI忽略列表
    /// </summary>
    private static List<string> uiIgnoreLs = new List<string>() { ".cs", ".meta", ".shader", ".txt" };

    /// <summary>
    ///获取打包资源根目录
    /// </summary>`1
    /// <returns></returns>
    private static string GetResTempPath()
    {
        string outputPath = Path.Combine(Application.dataPath, StringUtil.AppendFormat("../ABFiles/{0}", GetPlatformName())).ToForwardSlash().ToSingleForwardSlash();
        string dir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return outputPath;
    }

    /// <summary>
    /// 获取资源平台绝对路径
    /// </summary>
    /// <returns></returns>
    private static string GetPlatformName()
    {
        string platformName;
#if UNITY_ANDROID
        platformName = "Android/";
#elif UNITY_IOS
        platformName = "iOS/";
#else
        platformName = "Windows/";
#endif
        return platformName;
    }

    /// <summary>
    /// 把导出的配置改名字
    /// </summary>
    /// <param name="outPutPath"></param>
    /// <param name="changName"></param>
    private static void ChangeMainAbAndManifestName(string outPutPath, string changName)
    {
        string targetName = GetPlatformName().Replace("/", "");
        string originPath = Path.Combine(outPutPath, targetName).ToForwardSlash();
        string originManifestPath = originPath + ".manifest";
        string destPath = Path.Combine(outPutPath, changName).ToForwardSlash();
        string destManifestPath = destPath + ".manifest";
        File.Copy(originPath, destPath, true);
        File.Copy(originManifestPath, destManifestPath, true);
        File.Delete(originPath);
        File.Delete(originManifestPath);
    }

    /// <summary>
    /// 开始打包资源
    /// </summary>
    /// <param name="outPutPath"></param>
    /// <param name="abbs"></param>
    /// <param name="manifestName"></param>
    /// <param name="buildTarget"></param>
    /// <param name="options"></param>
    private static void CreateAssetBundle(string outPutPath, AssetBundleBuild[] abbs, string mainname, BuildTarget buildTarget, BuildAssetBundleOptions options)
    {
        try
        {
            BuildPipeline.BuildAssetBundles(outPutPath, abbs, options, buildTarget);
            if (!string.IsNullOrEmpty(mainname))
            {
                ChangeMainAbAndManifestName(outPutPath, mainname);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Message " + e.Message.ToString() + " StackTrace" + e.StackTrace.ToString());
        }
    }

    /// <summary>
    /// 搜集所有文件
    /// </summary>
    /// <param name="resPath">根目录</param>
    /// <param name="singleFileToAB">是否单独文件成为一个ab文件</param>
    /// <param name="goNextDir">下一级文件夹寻找 最大建议为1</param>
    /// <param name="outDic">导出的文件字典</param>
    /// <param name="exLs">后缀排除列表</param>
    public static void SearchFiles(string resPath, bool singleFileToAB, int goNextDir, ref Dictionary<string, List<string>> outDic, List<string> exLs)
    {
        if (goNextDir > 0)
        {
            string[] dirNames = Directory.GetDirectories(resPath);

            for (int i = 0; i < dirNames.Length; ++i)
            {
                SearchFiles(dirNames[i], singleFileToAB, goNextDir - 1, ref outDic, exLs);
            }
            return;
        }
        resPath = resPath.ToForwardSlash();
        if (resPath.EndsWith("/"))
        {
            resPath = resPath.Substring(0, resPath.Length - 1);
        }
        string[] fileNames;
        fileNames = Directory.GetFiles(resPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < fileNames.Length; i++)
        {
            string fileName = fileNames[i].ToForwardSlash();
            string ex = Path.GetExtension(fileName);
            if (exLs.Contains(ex))
            {
                continue;
            }
            string bundleName;

            if (singleFileToAB)
            {
                bundleName = fileName.Replace(ex, "");
            }
            else
            {
                bundleName = resPath;
            }
            bundleName = bundleName.ToForwardSlash();
            List<string> lstFiles = null;
            if (!outDic.TryGetValue(bundleName, out lstFiles))
            {
                lstFiles = new List<string>();
                outDic[bundleName] = lstFiles;
            }
            lstFiles.Add(fileName);
        }
    }

    /// <summary>
    /// 根据搜集到的字典文件来进行
    /// 创建资源文件
    /// </summary>
    /// <param name="dictFiles"></param>
    /// <returns></returns>
    private static AssetBundleBuild[] CreateAssetBundleBuild(Dictionary<string, List<string>> dictFiles)
    {
        AssetBundleBuild[] abb = new AssetBundleBuild[dictFiles.Count];
        int index = 0;
        foreach (var item in dictFiles)
        {
            abb[index].assetBundleName = PathUtil.GetBundleName(item.Key).ToLower().ToAssetBundleUrl();
            List<string> assetNames = new List<string>();
            foreach (string fileName in item.Value)
            {
                string newFileName = fileName.ToForwardSlash();
                newFileName = PathUtil.ConvertFullPathToAssetPath(newFileName);
                assetNames.Add(newFileName);
            }
            abb[index].assetNames = assetNames.ToArray();
            index++;
        }
        return abb;
    }

    /// <summary>
    /// 搜索所有的文件
    /// </summary>
    private static List<string> SearchAllFiles(string relativePath, string flag, List<string> ignoreLs)
    {
        string rootPath = Path.Combine(Application.dataPath, relativePath);
        string[] files = Directory.GetFiles(rootPath, flag, SearchOption.AllDirectories);
        List<string> shaderLs = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (ignoreLs.Contains(Path.GetExtension(files[i])))
            {
                continue;
            }
            shaderLs.Add(files[i]);
        }
        return shaderLs;
    }

    /// <summary>
    /// 生成文件的版本信息
    /// 简单文件的形式
    /// 后期在考虑加密
    /// </summary>
    private static void CreateVersionFile(string code, string zhdesc, string fzhdesc, string endesc)
    {
        string resPath = GetResTempPath();
        string versioncode = code;
        string[] resFiles = Directory.GetFiles(resPath, "*.assetbundle", SearchOption.AllDirectories);
        string versionPath = Path.Combine(Application.dataPath, "Res/Local/Version/" + GetPlatformName() + "version.txt");
        if (File.Exists(versionPath))
        {
            File.Delete(versionPath);
        }
        StreamWriter versionSw = File.CreateText(versionPath);
        versionSw.WriteLine(versioncode + "|" + zhdesc + "|" + fzhdesc + "|" + endesc);
        for (int i = 0; i < resFiles.Length; i++)
        {
            string fullPath = resFiles[i];
            if (fullPath.Contains("version.assetbundle"))//跳过version文件本身
            {
                continue;
            }
            string relativePath = resFiles[i].Replace(resPath, "");
            string md5 = Util.GetMd5ForFile(fullPath);
            long size = Util.GetFileLength(fullPath);
            string newMsg = relativePath.ToForwardSlash() + "|" + md5 + "|" + size;
            if (!string.IsNullOrEmpty(newMsg))
            {
                versionSw.WriteLine(newMsg);
            }
            // if (i == resFiles.Length - 1)
            // {
            //     versionSw.Write(newMsg);
            // }
            // else
            // {
            //     versionSw.Write(newMsg + "\n");
            // }
        }
        versionSw.Close();
        versionSw.Dispose();
        AssetDatabase.Refresh();
        AssetBundleBuild[] builds = new AssetBundleBuild[1];
        builds[0] = new AssetBundleBuild
        {
            assetBundleName = "share/version".ToAssetBundleUrl()
        };

        string rlath = PathUtil.ConvertFullPathToAssetPath(versionPath);
        string[] assets = new string[] { rlath };
        builds[0].assetNames = assets;
        CreateAssetBundle(GetResTempPath(), builds, string.Empty, GetTarget(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle);
    }

    /// <summary>
    /// 生成预加载的路径
    /// </summary>
    private static void CreatePreLoadMsgFile()
    {
        string sharePath = Path.Combine(GetResTempPath(), "share/");
        string[] atlasFiles = Directory.GetFiles(sharePath, "*.assetbundle", SearchOption.AllDirectories);
        string preLoadPath = Path.Combine(Application.dataPath, "Res/Local/preload.txt");

        if (File.Exists(preLoadPath))
        {
            File.Delete(preLoadPath);
        }
        StreamWriter preLoadSw = File.CreateText(preLoadPath);
        for (int i = 0; i < atlasFiles.Length; i++)
        {
            if (string.IsNullOrEmpty(atlasFiles[i]) || atlasFiles[i].Contains("preload") || atlasFiles[i].Contains("version")) //不需要预加载版本信息
            {
                continue;
            }
            string atlasRelativePath = atlasFiles[i].Replace(GetResTempPath(), "").ToKeyUrl();
            if (!string.IsNullOrEmpty(atlasRelativePath))
            {
                preLoadSw.WriteLine(atlasRelativePath);
            }
        }
        preLoadSw.Close();
        preLoadSw.Dispose();
        AssetDatabase.Refresh();

        AssetBundleBuild[] builds = new AssetBundleBuild[1];
        builds[0] = new AssetBundleBuild
        {
            assetBundleName = "share/preload".ToAssetBundleUrl()
        };

        string relativePath = PathUtil.ConvertFullPathToAssetPath(preLoadPath);

        string[] assets = new string[] { relativePath, };
        builds[0].assetNames = assets;
        CreateAssetBundle(GetResTempPath(), builds, string.Empty, GetTarget(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle);
    }


    // [MenuItem("BuildAssetBundle/打包本地表数据(C#)")]
    private static void BuildDataAssetBundle()
    {
        Dictionary<string, List<string>> fileDic = new Dictionary<string, List<string>>();
        fileDic["share/data"] = SearchAllFiles("Resources/Data", "*.asset", commonIgnoreLs);
        AssetBundleBuild[] builds = CreateAssetBundleBuild(fileDic);
        CreateAssetBundle(GetResTempPath(), builds, string.Empty, GetTarget(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle);
    }

    /// <summary>
    /// 拆分UI 主要是为了 图片和prefab脱离unity的本身形成的依赖管理使用自己的依赖管理
    /// 暂时用于UI，后期加入支持特效等
    /// </summary>
    [MenuItem("XYFramework/AssetBundle/序列化Prafab")]
    public static void SerilizeUI()
    {
        string targetRootPath = Path.Combine(Application.dataPath, "Res/").ToSingleForwardSlash().ToForwardSlash();
        string uiRootPath = Path.Combine(Application.dataPath, "Resources/").ToSingleForwardSlash().ToForwardSlash();
        string prefabmsgpath = Path.Combine(targetRootPath + "Local/Version", GetPlatformName() + "PrefabVersion.txt"); //分平台 记录变动的prefab
        string prefabmsgdir = Path.GetDirectoryName(prefabmsgpath);
        List<string> uiFilesPath = SearchAllFiles("Resources/Prefab/UI", "*prefab", uiIgnoreLs);
        Dictionary<string, string> oldprefabmsgdic = null;
        List<string> newuiFilesPath = null;
        if (File.Exists(prefabmsgpath))
        {
            oldprefabmsgdic = new Dictionary<string, string>();
            newuiFilesPath = new List<string>();
            string[] oldprefabfilesmsg = File.ReadAllLines(prefabmsgpath);
            if (oldprefabfilesmsg != null)
            {
                for (int i = 0; i < oldprefabfilesmsg.Length; i++)
                {
                    string[] msgarry = oldprefabfilesmsg[i].Split('|');
                    oldprefabmsgdic[msgarry[0]] = msgarry[1];
                }
            }

            for (int i = 0; i < uiFilesPath.Count; i++)
            {
                string filepath = uiFilesPath[i].ToSingleForwardSlash().ToForwardSlash();
                string key = filepath.Replace(uiRootPath, "");
                string newmd5 = Util.GetMd5ForFile(filepath);

                string oldmd5 = "";
                if (oldprefabmsgdic != null && oldprefabmsgdic.TryGetValue(key, out oldmd5))
                {
                    if (oldmd5 == newmd5)
                    {
                        continue;
                    }
                    else
                    {
                        newuiFilesPath.Add(filepath);
                    }
                }
                else
                {
                    newuiFilesPath.Add(filepath);
                }
            }
        }
        else
        {
            newuiFilesPath = uiFilesPath;
        }

        if (!Directory.Exists(prefabmsgdir))
        {
            Directory.CreateDirectory(prefabmsgdir);
        }
        if (File.Exists(prefabmsgpath))
        {
            File.Delete(prefabmsgpath);
        }
        StreamWriter sw = File.CreateText(prefabmsgpath);
        for (int i = 0; i < uiFilesPath.Count; i++)
        {
            string filepath = uiFilesPath[i].ToSingleForwardSlash().ToForwardSlash();
            string key = filepath.Replace(uiRootPath, "");
            string newmd5 = Util.GetMd5ForFile(filepath);
            sw.WriteLine(key + "|" + newmd5);
        }
        sw.Close();
        sw.Dispose();

        for (int i = 0; i < newuiFilesPath.Count; i++)
        {
            EditorUtility.DisplayProgressBar("正在拆分UI", newuiFilesPath[i], i / (float)newuiFilesPath.Count);
            string localPath = newuiFilesPath[i].ToSingleForwardSlash().ToForwardSlash().Replace(uiRootPath, "").Replace(".prefab", "");
            GameObject obj = GameObject.Instantiate(Resources.Load(localPath)) as GameObject;
            SerilizeInfo info = obj.AddComponent<SerilizeInfo>();
            info.Serilize();
            info.ClearAsset();
            string targetRelativePath = localPath + ".prefab";
            string targetpathAbs = Path.Combine(targetRootPath, targetRelativePath);
            string targetDir = Path.GetDirectoryName(targetpathAbs);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            string targetPath = PathUtil.ConvertFullPathToAssetPath(Path.Combine(targetRootPath, targetRelativePath));
            EditorUtility.SetDirty(obj);
            GameObject saveobj = PrefabUtility.CreatePrefab(targetPath, obj);
            GameObject.DestroyImmediate(obj, true);
            EditorUtility.SetDirty(saveobj);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/复制lua的绝对路径")]
    private static void GetRelativePath()
    {
        Object select = Selection.activeObject;
        if (select != null)
        {
            string path = AssetDatabase.GetAssetPath(select);
            path = path.Replace("Assets/", "").Replace("Res/", "");
            GUIUtility.systemCopyBuffer = path;
        }
    }


    /// <summary>
    /// 打包
    /// </summary>
    [MenuItem("XYFramework/AssetBundle/打包/1.差异打包资源(比较实用,比较快)")]
    private static void AutoBuildUIOrAssetBundle()
    {
        BuildAll(null, false);
    }

    /// <summary>
    /// 打包
    /// </summary>
    [MenuItem("XYFramework/AssetBundle/打包/2.强制打包资源")]
    private static void ForceBuildUIOrAssetBundle()
    {
        BuildAll(null, true);
    }

    private static void BuildUIOrOtherAssetBundle(bool force = false)
    {
        SerilizeUI();//序列化ui主要是拆分 依赖 自己保存asset的依赖关系
        Dictionary<string, List<string>> fileDic = new Dictionary<string, List<string>>();
        //shader资源独立出包
        // fileDic["share/shader"] = SearchAllFiles("Res/Shaders", "*shader", commonIgnoreLs);
        fileDic["share/font"] = SearchAllFiles("Res/Font", "*TTF", commonIgnoreLs);
        // 图集独立文件夹形式出包
        SearchFiles(Path.Combine(Application.dataPath, "Res/UIAltals"), true, 0, ref fileDic, commonIgnoreLs);
        //按文件夹形式打包大图
        SearchFiles(Path.Combine(Application.dataPath, "Res/UITexture"), false, 1, ref fileDic, commonIgnoreLs);
        //UI独立出包，依赖关系通过序列化得知
        SearchFiles(Path.Combine(Application.dataPath, "Res/Prefab/UI"), false, 1, ref fileDic, commonIgnoreLs);
        //音效
        SearchFiles(Path.Combine(Application.dataPath, "Res/Sound"), false, 1, ref fileDic, commonIgnoreLs);
        Dictionary<string, List<string>> newfiledic = null;
        if (force)
        {
            newfiledic = fileDic;
            SaveIncrementResFileVersion(newfiledic);//只是保存信息
        }
        else
        {
            newfiledic = SaveIncrementResFileVersion(fileDic, (path) =>
            {
                EditorUtility.DisplayProgressBar("正在进行版本比对", path, 0.5f);
            });
        }

        EditorUtility.DisplayProgressBar("开始压缩资源包", "", 0.7f);
        AssetBundleBuild[] builds = CreateAssetBundleBuild(newfiledic);
        CreateAssetBundle(GetResTempPath(), builds, "ui", GetTarget(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle);
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 产生当前打包的ui及其相关资源的信息
    /// </summary>
    static Dictionary<string, List<string>> SaveIncrementResFileVersion(Dictionary<string, List<string>> allfiles, System.Action<string> callbak = null)
    {
        string respath = GetResTempPath() + "resversion.txt";
        Dictionary<string, List<string>> newallfiles = new Dictionary<string, List<string>>();
        Dictionary<string, Dictionary<string, string>> oldallfiles = new Dictionary<string, Dictionary<string, string>>();
        if (File.Exists(respath))
        {
            string[] allmsg = File.ReadAllLines(respath);
            string lastkey = "";
            for (int i = 0; i < allmsg.Length; i++)
            {
                string[] msg = allmsg[i].Split('|');
                string filekey = msg[0];
                string filepath = msg[1];
                string oldfilemd5 = msg[2];
                if (lastkey != filekey)
                {
                    oldallfiles[filekey] = new Dictionary<string, string>();
                }
                oldallfiles[filekey][filepath] = oldfilemd5;
                lastkey = filekey;
                if (callbak != null)
                {
                    callbak(filepath);
                }
            }
            File.Delete(respath);
        }
        else
        {
            newallfiles = allfiles;
        }
        StreamWriter sw = File.CreateText(respath);
        foreach (var files in allfiles)
        {
            List<string> newfilels = files.Value; // 在新的版本中的当前key的所有文件
            foreach (var file in newfilels) //把最新的写入一遍
            {
                string msg = "";
                msg = files.Key + "|" + file + "|" + Util.GetMd5ForFile(file);
                sw.WriteLine(msg);
            }
            if (oldallfiles.Count > 0)// 如果有以前的记录
            {
                if (files.Key.Contains("share/font")) //如果是 不需要比对 字体直接进入 因为有依赖
                {
                    newallfiles[files.Key] = newfilels;
                    continue;
                }

                Dictionary<string, string> oldfiledic = null;
                if (oldallfiles.TryGetValue(files.Key, out oldfiledic))//在以前版本中发现一样key
                {
                    if (newfilels.Count != oldfiledic.Count) //数量不一致说明数量发生了变化 直接打包 然后循环下一个
                    {
                        if (callbak != null)
                        {
                            callbak(files.Key);
                        }
                        newallfiles[files.Key] = newfilels;
                        continue;
                    }
                    else
                    {
                        bool md5isequal = true; //判断md5是否相等
                        for (int i = 0; i < newfilels.Count; i++)
                        {
                            string oldmd5value = "";
                            if (oldfiledic.TryGetValue(newfilels[i], out oldmd5value))//在老的文件信息中是否找到最新的文件信息
                            {
                                if (oldmd5value != Util.GetMd5ForFile(newfilels[i]))
                                {
                                    md5isequal = false;
                                }
                            }
                            else
                            {
                                md5isequal = false;
                            }
                        }
                        if (!md5isequal)
                        {
                            if (callbak != null)
                            {
                                callbak(files.Key);
                            }
                            newallfiles[files.Key] = newfilels;
                        }
                    }
                }
                else
                {
                    if (callbak != null)
                    {
                        callbak(files.Key);
                    }
                    newallfiles[files.Key] = files.Value; //表示是新的
                }
            }
        }
        sw.Close();
        sw.Dispose();
        return newallfiles;
    }

    [MenuItem("XYFramework/AssetBundle/复制到服务器资源文件夹")]
    // 复制到服务器资源库里面
    public static void CopyToServerRes()
    {
        string targetdir = Path.Combine(Application.dataPath, "../../ServerResData/" + GetPlatformName());
        string rootpath = GetResTempPath();
        string[] allassetbundle = Directory.GetFiles(rootpath, "*.assetbundle", SearchOption.AllDirectories);
        int len = allassetbundle.Length;
        for (int i = 0; i < len; i++)
        {
            EditorUtility.DisplayProgressBar("复制资源", allassetbundle[i], i / (float)len);
            string relativeoriginpath = allassetbundle[i].Replace(rootpath, "");
            string newfilep = Path.Combine(targetdir, relativeoriginpath);
            string newdic = Path.GetDirectoryName(newfilep);
            if (!Directory.Exists(newdic))
            {
                Directory.CreateDirectory(newdic);
            }
            File.Copy(allassetbundle[i], newfilep, true);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("XYFramework/AssetBundle/打包/3.打包lua脚本")]
    public static void BuildLuaScript()
    {
        Dictionary<string, List<string>> fileDic = new Dictionary<string, List<string>>();
        List<string> allFiles = SearchAllFiles("Res/Lua", "*.txt", commonIgnoreLs);
        List<string> newLuas = new List<string>();
        for (int i = 0; i < allFiles.Count; i++)
        {
            newLuas.Add(allFiles[i]);
        }
        fileDic["share/lua"] = newLuas;
        AssetBundleBuild[] builds = CreateAssetBundleBuild(fileDic);
        CreateAssetBundle(GetResTempPath(), builds, string.Empty, GetTarget(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle);
    }

    public static void CreateVersiomsg(string code, string zh, string fzh, string en)
    {
        CreatePreLoadMsgFile();
        CreateVersionFile(code, zh, fzh, en);
    }

    public static void BuildAll(System.Action callback, bool force = false)
    {
        if (force)
        {
            string respath = GetResTempPath();
            if (Directory.Exists(respath))
            {
                Directory.Delete(respath, true);
            }
            Directory.CreateDirectory(respath);
        }
        InputDescWinEditor.Show((code, zh, fzh, en) =>
        {
            ExcelExportDataEditor.ExportAllExcelToLua();
            BuildUIOrOtherAssetBundle(force);
            BuildLuaScript();
            CreateVersiomsg(code, zh, fzh, en);
            CopyToServerRes();
            if (callback != null)
            {
                callback();
            }
        });
    }

    /// <summary>
    /// 删除Resources 下面的所有prefab 
    /// 为了防止unity打包时候自己
    /// 收集依赖产生多余的本地资源
    /// </summary>
    public static void DelResourcesPrefab()
    {
        string path = Path.Combine(Application.dataPath, "Resources/Prefab");
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 复制资源到streamasset 文件夹中
    /// </summary>
    public static void CopyToStreamAsset()
    {
        string targetrootdic = Path.Combine(Application.streamingAssetsPath, GetPlatformName()).ToSingleForwardSlash().ToForwardSlash();
        if (Directory.Exists(targetrootdic))
        {
            Directory.Delete(targetrootdic, true);
        }
        AssetDatabase.Refresh();
        string rootpath = GetResTempPath();
        string[] allabfiles = Directory.GetFiles(rootpath, "*.assetbundle", SearchOption.AllDirectories);
        for (int i = 0; i < allabfiles.Length; i++)
        {
            string originpath = allabfiles[i].ToSingleForwardSlash().ToForwardSlash();
            string relativeoriginpath = originpath.Replace(rootpath, "");
            string newfilep = Path.Combine(targetrootdic, relativeoriginpath);
            string newdic = Path.GetDirectoryName(newfilep);
            if (!Directory.Exists(newdic))
            {
                Directory.CreateDirectory(newdic);
            }
            File.Copy(originpath, newfilep, true);
        }
        AssetDatabase.Refresh();
    }

}
