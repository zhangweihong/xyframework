using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

/// <summary>
/// 参数注入的类
/// name 为对应 lua脚本的变量
/// value 对应 lua脚本对应变量的值
/// </summary>
[System.Serializable]
public class SerializeObject
{
    public string Name;
    public GameObject Value;
#if UNITY_EDITOR
    [System.NonSerialized]
    public System.Type Type = typeof(GameObject);
#endif
}

/// <summary>
/// xlua的管理脚本
/// </summary>
/// <typeparam name="XLuaManager"></typeparam>
public class XLuaManager : SingletonMono<XLuaManager>
{
    /// <summary>
    /// app lua脚本入口路径
    /// </summary>
    private const string appLuaPath = "Lua/app/app.lua.txt";

    /// <summary>
    /// 资源的主目录
    /// </summary>
    public const string luaResRoot = "Assets/Res/";

    /// <summary>
    /// 所有app 用到的lua的主目录
    /// </summary>
    public const string luaRootPath = "share/lua";

    private float m_LastGCTime = 0;
    private float m_GCInterval = 1f;

    /// <summary>
    /// 全局luaenv 有且只能有一个实例整个app中
    /// </summary>
    private LuaEnv env = null;
    public LuaEnv Env
    {
        get
        {
            return env;
        }
    }
    void Start()
    {
    }

    void Update()
    {
        if (Env != null && Time.time - m_LastGCTime > m_GCInterval)
        {
            Env.Tick();
            m_LastGCTime = Time.time;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
	public void Init()
    {
        env = new LuaEnv();
        AddLoadAppLua();
    }

    /// <summary>
    /// 加载脚本string形式
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="name"></param>
    /// <param name="curScriptEnv"></param>
    public object[] LoadLua(string txt, string name, LuaTable curScriptEnv)
    {
        return env.DoString(txt, name, curScriptEnv);
    }

    /// <summary>
    /// byte形式加载
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="name"></param>
    /// <param name="curScriptEnv"></param>
    public object[] LoadLua(byte[] bytes, string name, LuaTable curScriptEnv)
    {
        return env.DoString(bytes, name, curScriptEnv);
    }

    /// <summary>
    /// 加载lua脚本信息
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public TextAsset LoadTextAsset(string relativePath)
    {
        TextAsset appAsset = null;
        string assetPath = System.IO.Path.Combine(luaResRoot, relativePath);
#if UNITY_EDITOR
        string luapath = System.IO.Path.Combine("Assets/Res", relativePath).ToForwardSlash();
        appAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(luapath, typeof(TextAsset)) as TextAsset;
#else
        ResourcesManager.I.Load(luaRootPath, assetPath, (obj) =>
        {
            appAsset = obj as TextAsset;
        }, AssetType.File, false, true);
#endif
        return appAsset;
    }

    /// <summary>
    /// 加载主入口lua脚本并送入解释器
    /// /// </summary>
    public LuaTable LoadAppLua(SerializeObject[] objcts = null)
    {
        TextAsset appAsset = LoadTextAsset(appLuaPath);
        return LoadLua(appAsset.text, "app", null)[0] as LuaTable;
    }

    /// <summary>
    /// 解释lua重载 
    /// 稍后改为bundle的形式
    /// 所有的require 方式都会走这里
    /// luaPath是相对路径哦
    /// </summary>
    private void AddLoadAppLua()
    {
        env.AddLoader((ref string luaPath) =>
        {
            luaPath = StringUtil.AppendFormat("{0}.lua.txt", luaPath);
            TextAsset appAsset = LoadTextAsset(luaPath);
            return appAsset.bytes;
        });
    }


}
