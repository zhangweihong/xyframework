using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏基本静态参数或者常量
/// 或者一些环境参数等等
/// </summary>
public static class GameSettingUtil
{
    /// <summary>
    /// 是否使用ab文件加载
    /// </summary>
    public static bool IsAssetBundle = true;
    public static int ScreenWidth = 1334;
    public static int ScreenHeight = 750;
    public static string Idf = "com.xyframework.app";
    public static string Version = "1.0.0"; // app的版本号
    public static string Versioncode = "1"; //当前大版本(C#)代码的版本号
}
