using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// 当前file的状态
/// </summary>
public enum VersionState
{
    None, //不变
    Add,//添加
    Del,// 删除
    Update// 更新
}

/// <summary>
/// 版本文件的基本信息
/// </summary>
public class VersionInfo
{
    public string RelaPath = "";
    public string FileMd5 = "";
    public long FileSize = 0;
    public VersionState VerState = VersionState.None;
}
/// <summary>
/// 版本控制
/// </summary>
/// <typeparam name="VersionManager"></typeparam>
public class VersionManager : SingletonMono<VersionManager>
{

    /// <summary>
    /// 本地包体内的版本信息路径
    /// </summary>
    private string m_Localversionpath = "";
    /// <summary>
    /// 缓存中的版本信息路径
    /// </summary>
    private string m_Cacheversionpath = "";

    // Use this for initialization
    /// <summary>
    /// 本地
    /// </summary>
    private List<VersionInfo> m_LocalVerInfos;
    /// <summary>
    /// 已下载的缓存信息
    /// </summary>
    private List<VersionInfo> m_CacheVerInfos;

    /// <summary>
    /// app 本身所有的版本信息
    /// </summary>
    private List<VersionInfo> m_APPInnerVerInfos;

    /// <summary>
    /// Persistent 中是否存在version信息也就是version.assetbundle
    /// </summary>
    private bool m_CacheVersionExist = false;

    /// <summary>
    /// 网络在线的版本信息
    /// </summary>
    private string m_NetVersionMsg = "";

    /// <summary>
    /// app 内部的版本信息
    /// </summary>
    private string m_LocalVersionMsg = "";

    /// <summary>
    /// app 本地缓存的版本信息
    /// </summary>
    private string m_CacheVersionMsg = "";

    /// <summary>
    /// app 真实可用的的版本信息
    /// </summary>
    private string m_AppResVersionMsg = "";

    private byte[] m_NetBytes = null;

    private const string m_VersionPathName = "share/version.assetbundle";

    private string m_NetResHost = "http://127.0.0.1/res";
    void Awake()
    {
        m_Localversionpath = PathUtil.GetABFilePath(m_VersionPathName);
        m_Cacheversionpath = PathUtil.ConvertRelativePathToPersistentPath(m_VersionPathName);
        if (File.Exists(m_Cacheversionpath))
        {
            m_CacheVersionExist = true;
        }
    }
    void Start()
    {

    }

    public void SetConfig(string host)
    {
        m_NetResHost = host;
    }

    private string GetVersiontext(string versionPath)
    {
        string versiontext = "";
        AssetBundle localab = AssetBundle.LoadFromFile(versionPath);
        if (localab == null)
        {
            Debug.LogError("本地版本信息缺失！！");
        }
        else
        {
            TextAsset asset = localab.LoadAsset("version", typeof(TextAsset)) as TextAsset;
            versiontext = asset.text;
            localab.Unload(true);
        }
        return versiontext;
    }

    private List<VersionInfo> GetVersionInfos(string versiontext, int localorcache)
    {
        if (string.IsNullOrEmpty(versiontext))
        {
            return null;
        }
        List<VersionInfo> verinfos = new List<VersionInfo>();
        string[] versionArry = versiontext.Replace("\r", string.Empty).Split('\n'); // \r 是因为写入的使用的是wirteline
        if (localorcache == 1)
        {
            m_LocalVersionMsg = versionArry[0];
        }
        else if (localorcache == 2)
        {
            m_CacheVersionMsg = versionArry[0];
        }
        else if (localorcache == 3)
        {
            m_NetVersionMsg = versionArry[0];
        }
        for (int i = 1; i < versionArry.Length; i++)
        {
            if (string.IsNullOrEmpty(versionArry[i]))
            {
                continue;
            }
            VersionInfo info = new VersionInfo();
            string[] infoarry = versionArry[i].Split('|');
            info.RelaPath = infoarry[0];
            info.FileMd5 = infoarry[1];
            info.FileSize = long.Parse(infoarry[2]);
            verinfos.Add(info);
        }
        return verinfos;
    }


    public void Init()
    {
        m_LocalVerInfos = GetVersionInfos(GetVersiontext(m_Localversionpath), 1);
        if (m_CacheVersionExist) //如果缓存中有	
        {
            m_CacheVerInfos = GetVersionInfos(GetVersiontext(m_Cacheversionpath), 2);
            m_APPInnerVerInfos = m_CacheVerInfos;
            m_AppResVersionMsg = m_CacheVersionMsg;
            try
            {
                string version = m_AppResVersionMsg.Split('|')[1];
                if (version != GameSettingUtil.Version)
                {
                    Util.ClearCache();
                    m_APPInnerVerInfos = m_LocalVerInfos;//缓存版本不一致的删除完之后 还原本地信息
                    m_AppResVersionMsg = m_LocalVersionMsg;
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
        else
        {
            m_APPInnerVerInfos = m_LocalVerInfos;
            m_AppResVersionMsg = m_LocalVersionMsg;
        }
    }

    public string AppNowResVersionmsg
    {
        get
        {
            return m_AppResVersionMsg;
        }
    }

    public void CheckVersion(System.Action<long, List<string>, string> precallback, System.Action<string> errorcallback)
    {
        DownLoadManager.I.StartDownLoad(StringUtil.AppendFormat("{0}/{1}{2}", m_NetResHost, PathUtil.GetPlatformName(), m_VersionPathName), (p) =>
         {
         }, (bytes) =>
         {
             if (bytes == null)
             {
                 if (errorcallback != null)
                 {
                     errorcallback("102"); // 网络版本信息 返回为空
                 }
                 return;
             }
             m_NetBytes = bytes;
             AssetBundle netverab = AssetBundle.LoadFromMemory(m_NetBytes);
             TextAsset nettext = netverab.LoadAsset("version", typeof(TextAsset)) as TextAsset;
             if (nettext == null)
             {
                 if (errorcallback != null)
                 {
                     errorcallback("103"); // 网络版本信息 不包含 version信息 可能打包version信息的时候 资源出了问题
                 }
                 return;
             }
             List<VersionInfo> netverinfos = GetVersionInfos(nettext.text, 3);
             netverab.Unload(true);
             StartCheck(netverinfos, m_APPInnerVerInfos, precallback, errorcallback);
         }, (error) =>
         {
             errorcallback("101"); // 网络版本信息 下载出错
             Debug.LogError("下载错误 ！ " + error);
         });
    }

    public void CheckProgress(float p, string relapath, byte[] bytes)
    {
        Debug.Log("p " + p + " relapath " + relapath + " bytes " + bytes.Length);
        string path = PathUtil.ConvertRelativePathToPersistentPath(relapath);
        Util.SaveBytesToFile(bytes, path);
    }

    public void CheckFinish()
    {
        if (m_NetBytes != null)
        {
            Util.SaveBytesToFile(m_NetBytes, m_Cacheversionpath);
        }
        m_AppResVersionMsg = m_NetVersionMsg;// 更新完成 内部资源信息变更
        m_NetBytes = null;
    }

    private void StartCheck(List<VersionInfo> netverinfos, List<VersionInfo> appverinfos, Action<long, List<string>, string> preAction, Action<string> errorAction)
    {
        if (appverinfos == null)
        {
            if (errorAction != null)
            {
                errorAction("201");// 没有在本地找到任何版本信息 app是否被修改过
            }
            return;
        }
        List<VersionInfo> diffverInfos = new List<VersionInfo>();
        List<string> downverInfos = new List<string>();
        for (int i = 0; i < netverinfos.Count; i++)
        {
            VersionInfo netverInfo = netverinfos[i];
            VersionInfo findverInfo = appverinfos.Find((item) => item.RelaPath == netverInfo.RelaPath); //在当前app版本信息中寻找是否找的到
            if (findverInfo == null)
            {
                netverInfo.VerState = VersionState.Add;//找不到 说明是最新的 添加状态
                diffverInfos.Add(netverInfo);
            }
            else
            {
                if (netverInfo.FileMd5 != findverInfo.FileMd5)
                {
                    netverInfo.VerState = VersionState.Update;//找到了如果md5不一致 进行更新状态
                    diffverInfos.Add(netverInfo);
                }
            }
        }

        for (int i = 0; i < appverinfos.Count; i++)
        {
            VersionInfo appverInfo = appverinfos[i];
            VersionInfo findverInfo = netverinfos.Find((item) => item.RelaPath == appverInfo.RelaPath); //在 最新版本寻找是否找的到
            if (findverInfo == null)
            {
                appverInfo.VerState = VersionState.Del;//找不到执行 说明新版本已经删除
                diffverInfos.Add(appverInfo);
            }
        }
        long filesize = 0;
        for (int i = 0; i < diffverInfos.Count; i++)
        {
            VersionInfo diffinfo = diffverInfos[i];
            if (diffinfo.VerState == VersionState.Add || diffinfo.VerState == VersionState.Update)
            {
                downverInfos.Add(diffinfo.RelaPath);
                filesize += diffverInfos[i].FileSize;
            }
            else if (diffinfo.VerState == VersionState.Del)
            {
                string cachepath = PathUtil.ConvertRelativePathToPersistentPath(diffinfo.RelaPath);
                if (File.Exists(cachepath))
                {
                    File.Delete(cachepath);
                }
            }
        }
        if (preAction != null)
        {
            preAction(filesize, downverInfos, m_NetVersionMsg);
        }
    }

    public void StartDown(List<string> downverInfos, Action<float> progressAction, Action finishAction, Action<string> errorAction)
    {
        if (downverInfos.Count > 0)
        {
            DownLoadManager.I.StartDownloads(m_NetResHost, downverInfos, (p, path, bytes) =>
            {
                CheckProgress(p, path, bytes);
                if (progressAction != null)
                {
                    progressAction(p);
                }

            }, () =>
            {
                CheckFinish();
                if (finishAction != null)
                {
                    finishAction();
                }
            }, errorAction);
        }
        else
        {
            if (finishAction != null)
            {
                finishAction();
            }
        }
    }

}
