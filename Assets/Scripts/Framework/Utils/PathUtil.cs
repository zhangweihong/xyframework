using UnityEngine;
using System.IO;
using System.Collections;

public static class PathUtil
{

    /// <summary>
    /// 原始资源所在相对根目录
    /// </summary>
    private static readonly string rootPath = "Resources/";

    /// <summary>
    /// 无依赖原始资源所在相对根目录
    /// </summary>
    private static readonly string rootResPath = "Res/";

    /// <summary>
    /// 工程的相对根
    /// </summary>
    private static readonly string assetsPath = "Assets/";

    /// <summary>
    /// 转换为真实路径，目前路径均为小写
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string ToKeyUrl(this string path)
    {
        return path.ToForwardSlash().ToLower();
    }

    /// <summary>
    /// 判断是否ab文件
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static bool IsAssetBundleUrl(this string relativePath)
    {
        return relativePath.EndsWith(".assetbundle");
    }

    /// <summary>
    /// 转换为扩展名.assetbundle的路径
    /// </summary>
    /// <param name="relativePath">路径</param>
    /// <returns></returns>
    public static string ToAssetBundleUrl(this string relativePath)
    {
        if (relativePath.IsAssetBundleUrl())
        {
            return relativePath;
        }
        return StringUtil.AppendFormat("{0}.assetbundle", relativePath);
    }

    /// <summary>
    ///获取打包资源根目录 
    /// </summary>
    /// <returns></returns>
    public static string GetEditorABRootPath()
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
    public static string GetPlatformName()
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
    /// 将反斜杠转换为正斜杠
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string ToForwardSlash(this string path)
    {
        return path.Replace(@"\", @"/");
    }

    /// <summary>
    /// 将双斜杠转换为单斜杠
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ToSingleForwardSlash(this string path)
    {
        return path.Replace(@"//", @"/");
    }

    /// <summary>
    /// 合并路径，并转换为正斜杠
    /// </summary>
    /// <param name="path1">路径1</param>
    /// <param name="path2">路径2</param>
    /// <returns></returns>
    public static string CombinePath(string path1, string path2)
    {
        return Path.Combine(path1, path2).ToForwardSlash();
    }

    /// <summary>
    /// 检查文件夹，如果不存在则创建
    /// </summary>
    /// <param name="path"></param>
    public static void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// 获取资源的绝对路径
    /// 编辑器模式下默认使用的是 ../TeamData 全路径
    /// 其他平台默认获取StreamingPath 全路径
    /// </summary>
    /// <param name="relativePath"></param>
    public static string GetABFilePath(string relativePath)
    {
        string path = "";
#if UNITY_EDITOR
        path = ConvertRelativePathToEditorOutputPath(relativePath);
#else
        path = ConvertRelativePathToStreamingPath(relativePath);
#endif
        return path;
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool CheckFileExist(string path)
    {
        return File.Exists(path);
    }
    /// <summary>
    /// 将字节写入一个文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bytes"></param>
    public static void CreateFile(string path, byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);
    }
    /// <summary>
    /// 将字符串写入一个全新的文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="contents"></param>
    public static void CreateFile(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    /// <summary>
    /// 获得指定路径下的所有文件夹
    /// </summary>
    /// <param name="path"></param>
    /// <param name="searchOption"></param>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    public static string[] GetDirectoryNames(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly, string searchPattern = "*")
    {
        if (Directory.Exists(path))
        {
            return Directory.GetDirectories(path, searchPattern, searchOption);
        }
        return null;
    }

    /// <summary>
    /// 获得文件夹路径
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetDirectoryPath(string path)
    {
        path = path.ToForwardSlash();

        if (path.EndsWith(@"/"))
        {
            return path;
        }

        int lastSlashIndex = path.LastIndexOf(@"/");

        if (lastSlashIndex >= 0 && lastSlashIndex < path.Length)
        {
            return path.Substring(0, lastSlashIndex);
        }
        return path;
    }

    /// <summary>
    /// 获得文件夹名称
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFolderName(string path)
    {
        path = path.ToForwardSlash();

        if (path.EndsWith(@"/"))
        {
            path = path.Substring(0, path.Length - 1);
        }

        int lastSlashIndex = path.LastIndexOf(@"/");

        if (lastSlashIndex >= 0 && lastSlashIndex < path.Length)
        {
            return path.Substring(lastSlashIndex + 1, path.Length - lastSlashIndex - 1);
        }
        return path;
    }

    /// <summary>
    /// 获得指定路径下的所有文件名
    /// </summary>
    /// <param name="path"></param>
    /// <param name="searchOption"></param>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    public static string[] GetFileNames(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly, string searchPattern = "*")
    {
        if (Directory.Exists(path))
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }
        return null;
    }

    /// <summary>
    /// 获取文件名不带后缀
    /// </summary>
    /// <param name="pathName"></param>
    /// <returns></returns>
    public static string GetFilePathNameWithoutExtension(string pathName)
    {
        return Path.Combine(GetDirectoryPath(pathName), Path.GetFileNameWithoutExtension(pathName));
    }

    public static string GetFileExtension(string pathName)
    {
        string[] splitNames = pathName.Split('.');
        if (splitNames.Length < 2)
        {
            return null;
        }

        return splitNames[splitNames.Length - 1];
    }
    /// <summary>
    /// 获取文件名带后缀
    /// </summary>
    /// <param name="pathName"></param>
    /// <param name="withoutExtension"></param>
    /// <returns></returns>
    public static string GetFileName(string pathName, bool withoutExtension = false)
    {
        if (withoutExtension)
        {
            return Path.GetFileNameWithoutExtension(pathName);
        }
        else
        {
            return Path.GetFileName(pathName);
        }
    }

    /// <summary>
    /// 将相对路径转换为编辑器的输出文件夹路径
    /// </summary>
    /// <param name="relativePath">相对路径</param>
    /// <returns></returns>
    public static string ConvertRelativePathToEditorOutputPath(string relativePath)
    {
        string platformPath;
#if UNITY_ANDROID
        platformPath = "Android/";
#elif UNITY_IOS
        platformPath = "iOS/";
#else
        platformPath = "Windows/";
#endif
        string outputPath = Path.Combine(Application.dataPath, StringUtil.AppendFormat("../ABFiles/{0}", platformPath));
        string dir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(outputPath, relativePath.ToSingleForwardSlash()).ToForwardSlash();
    }

    /// <summary>
    /// 获得相对路径
    /// </summary>
    /// <param name="path">完整路径</param>
    /// <param name="directory">相对文件夹</param>
    /// <returns></returns>
    public static string GetRelativePath(string path, string directory)
    {
        path = path.ToForwardSlash();
        directory = directory.ToForwardSlash();
        return path.Replace(directory, "");
    }

    /// <summary>
    /// 将相对路径转换为Persistent文件夹路径
    /// </summary>
    /// <param name="relativePath">相对路径</param>
    /// <returns></returns>
    public static string ConvertRelativePathToPersistentPath(string relativePath)
    {
        return Path.Combine(Application.persistentDataPath, relativePath.ToSingleForwardSlash()).ToForwardSlash();
    }

    /// <summary>
    /// 将相对路径转换为Streaming文件夹路径
    /// </summary>
    /// <param name="relativePath">相对路径</param>
    /// <returns></returns>
    public static string ConvertRelativePathToStreamingPath(string relativePath)
    {
        string platformPath;
#if UNITY_ANDROID
        platformPath = "Android/";
#elif UNITY_IOS
        platformPath = "iOS/";
#else
        platformPath = "Windows/";
#endif
        return Path.Combine(Application.streamingAssetsPath, Path.Combine(platformPath, relativePath.ToSingleForwardSlash())).ToForwardSlash();
    }

    /// <summary>
    /// 将完整路径转换为Assets路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ConvertFullPathToAssetPath(string path)
    {
        path = GetRelativePath(path, Application.dataPath);
        return ("Assets/" + path).ToSingleForwardSlash();
    }
    /// <summary>
    /// 将Assets路径转化为完整路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ConvertAssetPathToFullPath(string path)
    {
        path = GetRelativePath(path, "Assets");
        return (Application.dataPath + path).ToSingleForwardSlash();
    }

    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="path"></param>
    /// <param name="recursive"></param>
    public static void DeleteDirectory(string path, bool recursive = false)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path"></param>
    public static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 删除所有文件
    /// </summary>
    /// <param name="targetPath"></param>
    /// <param name="searchPattern"></param>
    /// <param name="searchOption"></param>
    public static void DeleteAllFiles(string targetPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(targetPath))
        {
            return;
        }

        string[] allFiles = Directory.GetFiles(targetPath, searchPattern, searchOption);
        for (int i = 0; i < allFiles.Length; ++i)
        {
            File.Delete(allFiles[i]);
        }
    }

    /// <summary>
    /// 清理已过期压缩包
    /// </summary>
    /// <param name="targetPath">目标路径</param>
    /// <param name="searchPattern"></param>
    /// <param name="searchOption"></param>
    public static void DeleteUnusedCompressFiles(string targetPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(targetPath))
        {
            return;
        }

        string[] allFiles = Directory.GetFiles(targetPath, searchPattern, searchOption);
        for (int i = 0; i < allFiles.Length; ++i)
        {
            string file = allFiles[i];
            string filePath = Path.GetDirectoryName(file);
            string fileName = Path.GetFileNameWithoutExtension(file);
            string filePathName = Path.Combine(filePath, fileName);

            if (File.Exists(filePathName))
            {
                continue;
            }
            File.Delete(file);
        }
    }
    /// <summary>
    /// 判断文件路径是不是文件夹
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFolderPath(this string path)
    {
        return !path.Contains(".");
    }
    /// <summary>
    /// 判断文件是否是不.meta文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsMetaFile(this string path)
    {
        return path.EndsWith(".meta");
    }


    /// <summary>
    /// 获取资源的相对路径
    /// </summary>
    /// <param name="absPath"></param>
    /// <returns></returns>
    public static string GetBundleName(string absPath)
    {
        string bdbname = PathUtil.ConvertFullPathToAssetPath(absPath).Replace(assetsPath, "").Replace(rootPath, "").Replace(rootResPath, "");
        return bdbname;
    }
}