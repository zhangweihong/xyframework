using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 切记同步和异步绝对不可以在
/// 同一时间调用不然会卡主线程造成
/// 不必要的卡顿
/// </summary>
public class ResourcesManager : SingletonMono<ResourcesManager>
{
    /// <summary>
    ///当前加载的Ab文件字典
    /// </summary>
    private Dictionary<string, BundleInfo> curBundleInfoDic = new Dictionary<string, BundleInfo>();
    /// <summary>
    /// 正在加载的资源
    /// 主要是用在异步在家的时候避免重复加载
    /// </summary>
    private Dictionary<string, BundleInfo> loadingBundleInfoDic = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// 使用resource方式加载进来的资源
    /// 基本很少使用
    /// </summary>
    private Dictionary<string, object> curLoaclAssetDic = new Dictionary<string, object>();

    /// <summary>
    /// 预加载文件信息
    /// </summary>
    private const string preLoadName = "share/preload.assetbundle";

    public void Init()
    {
    }

    /// <summary>
    /// 预加载ab文件
    /// </summary>
    public void PreLoad(Action<float> progressAction = null)
    {
        this.Load(preLoadName, "preload", (object msg) =>
        {
            TextAsset asset = msg as TextAsset;
            string[] allFiles = asset.text.Replace("\r", string.Empty).Split('\n'); //\r 是因为写入的使用的是wirteline
            this.SyncLoadABs(Util.ArryConvertList<string>(allFiles), (List<BundleInfo> infols) =>
            {
            }, (float p) =>
            {
                if (progressAction != null)
                {
                    progressAction(p);
                }
            });

        }, AssetType.File, false, true);
    }

    /// <summary>   
    /// nameEx 必须带后缀名
    /// useAb 是否使用ab文件加载
    /// </summary>
    /// <param name="dirPath"></param>
    /// <param name="nameEx"></param>
    /// <param name="async"></param>
    public void Load(string dirPath, string nameEx, Action<object> callBack, AssetType type, bool async = false, bool isAlwaysInMemory = false, bool isall = false)
    {
        if (GameSettingUtil.IsAssetBundle)
        {
            if (async)
            {
                AsyncLoadAssetWithAB(dirPath, nameEx, callBack, type, isAlwaysInMemory, isall);
            }
            else
            {
                SyncLoadAssetWithAB(dirPath, nameEx, callBack, type, isAlwaysInMemory, isall);
            }
        }
        else
        {
            string fileName = Path.GetFileName(nameEx);
            string path = Path.Combine(dirPath, fileName);
            if (async)
            {
                AsyncResourcesLoadAsset(path, callBack);
            }
            else
            {
                SyncResourcesLoadAsset(path, callBack);
            }
        }
    }

    /// <summary>
    /// 同步加载大量的ab文件
    /// 主要是预加载 依赖关系的ab文件
    /// 使用同步加载
    /// 因为同步相对于异步是要快些的
    /// 在loading的时候等待时间比较短
    /// </summary>
    /// <param name="bunldeLs"></param>
    private void SyncLoadABs(List<string> bunldeLs, Action<List<BundleInfo>> callBack, Action<float> progressCallBack = null)
    {
        List<BundleInfo> bundleInfos = new List<BundleInfo>();
        bool isExsit = false;
        float count = bunldeLs.Count;
        for (int i = 0; i < bunldeLs.Count; i++)
        {
            if (string.IsNullOrEmpty(bunldeLs[i]))
            {
                continue;
            }
            BundleInfo bundleInfo = null;
            string bundleName = bunldeLs[i];
            bundleName = bundleName.ToAssetBundleUrl().ToKeyUrl();

            if (curBundleInfoDic.TryGetValue(bundleName, out bundleInfo))
            {
                if (bundleInfo != null && bundleInfo.AssetBundle != null)
                {
                    bundleInfos.Add(bundleInfo);
                    isExsit = true;
                }
                else
                {
                    isExsit = false;
                }
            }

            if (!isExsit)
            {
                string bundlePath = PathUtil.GetABFilePath(bundleName);
                string persistentPath = PathUtil.ConvertRelativePathToPersistentPath(bundleName); //先检查 缓存目录中是否有 最新的资源
                if (File.Exists(persistentPath))
                {
                    bundlePath = persistentPath;
                }
                if (bundleInfo == null)
                {
                    bundleInfo = new BundleInfo();
                }
                bundleInfo.AssetBundle = AssetBundle.LoadFromFile(bundlePath);
                bundleInfo.WaiteFinishAction = null;
                bundleInfo.AssetbundleName = bundleName;
                curBundleInfoDic[bundleName] = bundleInfo;
                bundleInfos.Add(bundleInfo);
            }

            if (progressCallBack != null)
            {
                progressCallBack((i + 1) / count);
            }
        }
        if (callBack != null)
        {
            callBack(bundleInfos);
        }
    }

    /// <summary>
    /// 异步加载Asset文件
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="assetName"></param>
    /// <param name="callBack"></param>
    /// <param name="type"></param>
    /// <param name="isAlwaysInMemory"></param>
    /// <returns></returns>
    private void AsyncLoadAssetWithAB(string bundleName, string assetName, Action<object> callBack, AssetType type = AssetType.None, bool isAlwaysInMemory = false, bool isall = false)
    {
        CoroutineManager.I.AddTask(_AsyncLoadAssetWithAB(bundleName, assetName, callBack, type, isAlwaysInMemory, isall));
    }

    private IEnumerator _AsyncLoadAssetWithAB(string bundleName, string assetName, Action<object> callBack, AssetType type = AssetType.None, bool isAlwaysInMemory = false, bool isall = false)
    {
        bundleName = bundleName.ToAssetBundleUrl().ToKeyUrl();
        BundleInfo bundleInfo = null;
        //如果当前正在加载
        if (loadingBundleInfoDic.TryGetValue(bundleName, out bundleInfo))
        {
            //检测意外的info为空
            if (bundleInfo != null)
            {
                bundleInfo.WaiteFinishAction += (BundleInfo newInfo) =>
                {
                    CoroutineManager.I.AddTask(newInfo.AsyncLoadAsset(assetName, callBack, type, isAlwaysInMemory, isall));
                };
            }
            else
            {
                Debug.LogError(bundleName + " 在正在加载字典中为 null，请检查是否被null！！！");
            }
            yield break;
        }
        //已将在当前的缓存中
        if (curBundleInfoDic.TryGetValue(bundleName, out bundleInfo))
        {
            if (bundleInfo != null)
            {
                CoroutineManager.I.AddTask(bundleInfo.AsyncLoadAsset(assetName, callBack, type, isAlwaysInMemory, isall));
            }
        }
        //如果没有在缓存中找到，开始加载
        if (bundleInfo == null)
        {
            string bundlePath = PathUtil.GetABFilePath(bundleName);
            string persistentPath = PathUtil.ConvertRelativePathToPersistentPath(bundleName); //先检查 缓存目录中是否有 
            //优先判断缓存中是否有
            if (File.Exists(persistentPath))
            {
                bundlePath = persistentPath;
            }

            bundleInfo = new BundleInfo
            {
                AssetbundleName = bundleName
            };
            loadingBundleInfoDic[bundleName] = bundleInfo;
            AssetBundleCreateRequest cReq = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return cReq;
            loadingBundleInfoDic.Remove(bundleName);
            if (cReq.isDone)
            {
                bundleInfo.AssetBundle = cReq.assetBundle;
                curBundleInfoDic[bundleName] = bundleInfo;
                if (bundleInfo.WaiteFinishAction != null)
                {
                    bundleInfo.WaiteFinishAction(bundleInfo);
                }
                bundleInfo.WaiteFinishAction = null;
                CoroutineManager.I.AddTask(bundleInfo.AsyncLoadAsset(assetName, callBack, type, isAlwaysInMemory, isall));
            }
            else
            {
                if (callBack != null)
                {
                    callBack(null);
                }
                Debug.LogError(bundleName + " 加载出错 !!!");
            }
        }
        yield break;
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="assetName"></param>
    /// <param name="callBack"></param>
    /// <param name="type"></param>
    /// <param name="isAlwaysInMemory"></param>
    private void SyncLoadAssetWithAB(string bundleName, string assetName, Action<object> callBack, AssetType type = AssetType.None, bool isAlwaysInMemory = false, bool isall = false)
    {
        bundleName = bundleName.ToAssetBundleUrl().ToKeyUrl();
        BundleInfo bundleInfo = null;
        if (loadingBundleInfoDic.TryGetValue(bundleName, out bundleInfo))
        {
            if (bundleInfo != null)
            {
                bundleInfo.WaiteFinishAction += (BundleInfo newInfo) =>
                {
                    newInfo.SyncLoadAsset(assetName, callBack, type, isAlwaysInMemory, isall);
                };
            }
            else
            {
                Debug.LogError(bundleName + " 在正在加载字典中为 null，请检查是否被null！！！");
            }
            return;
        }

        curBundleInfoDic.TryGetValue(bundleName, out bundleInfo);
        if (bundleInfo != null)
        {
            bundleInfo.SyncLoadAsset(assetName, callBack, type, isAlwaysInMemory, isall);
        }
        else
        {
            bundleInfo = new BundleInfo
            {
                AssetbundleName = bundleName
            };
            loadingBundleInfoDic[bundleName] = bundleInfo;
            string bundlePath = PathUtil.GetABFilePath(bundleName);
            string persistentPath = PathUtil.ConvertRelativePathToPersistentPath(bundleName);
            //先检查 缓存目录中是否有 最新的资源
            if (File.Exists(persistentPath))
            {
                bundlePath = persistentPath;
            }
            try
            {
                bundleInfo.AssetBundle = AssetBundle.LoadFromFile(bundlePath);
                loadingBundleInfoDic.Remove(bundleName);
                curBundleInfoDic[bundleName] = bundleInfo;
                bundleInfo.SyncLoadAsset(assetName, callBack, type, isAlwaysInMemory, isall);

                if (bundleInfo.WaiteFinishAction != null)
                {
                    bundleInfo.WaiteFinishAction(bundleInfo);
                }
                bundleInfo.WaiteFinishAction = null;
            }
            catch (Exception e)
            {
                loadingBundleInfoDic.Remove(bundleName);
                Debug.LogError(bundlePath + " 加载失败 message " + e.Message + " StackTrace" + e.StackTrace);
                if (callBack != null)
                {
                    callBack(null);
                }
            }
        }
    }

    /// <summary>
    /// bundle 只要加载了不会被卸载
    /// 因为加载的只是bundle的索引文件
    /// 其实并不是太大，在android每个
    /// bundle占用0.5M的大小貌似是android的机制
    /// 这个只是老版本unity的问题新版本还没有检查
    /// 真实的大小在于asset
    /// 暂时这要考虑
    /// 看后期需求
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="assetName"></param>
    /// <param name="forceUnloadAsset"></param>
    public void UnLoadAssetBundleAsset(string bundleName, string assetName, bool forceUnloadAsset = false)
    {
        bundleName = bundleName.ToKeyUrl().ToAssetBundleUrl();
        BundleInfo bundleInfo = null;
        if (curBundleInfoDic.TryGetValue(bundleName, out bundleInfo))
        {
            if (bundleInfo != null)
            {
                bundleInfo.UnLoad(assetName, forceUnloadAsset);
            }
            else
            {
                curBundleInfoDic.Remove(bundleName);
                Debug.LogError(bundleName + " 当前字典中已经为null 请检查是不是已经卸载 ！！！");
            }
        }
        else
        {
            Debug.LogError(bundleName + " 未在当前字典中找到 ！！！");
        }

    }

    /// <summary>
    /// 使用本地的方式进行异步加载
    /// 方便快捷开发使用
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="callBack"></param>
    private void AsyncResourcesLoadAsset(string assetPath, Action<object> callBack)
    {
        object obj = null;
        if (CheckLocalAssetInDic(assetPath, ref obj))
        {
            if (callBack == null)
            {
                callBack(obj);
            }
        }
        else
        {
            CoroutineManager.I.AddTask(_AsyncResourcesLoadAsset(assetPath, callBack));
        }
    }

    /// <summary>
    /// 异步加载本地资源
    /// </summary>
    /// <param name="AssetPath"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    private IEnumerator _AsyncResourcesLoadAsset(string AssetPath, Action<object> callBack)
    {
        ResourceRequest req = Resources.LoadAsync(AssetPath);
        yield return req;
        if (req.isDone)
        {
            curLoaclAssetDic[AssetPath] = req.asset;
            if (callBack == null)
            {
                callBack(req.asset);
            }
        }
        else
        {
            if (callBack == null)
            {
                callBack(null);
            }
            Debug.LogError(AssetPath + " 加载错误 ！！！");
        }
    }

    /// <summary>
    /// 检查使用resource方式加载的资源
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private bool CheckLocalAssetInDic(string assetPath, ref object obj)
    {
        if (curLoaclAssetDic.TryGetValue(assetPath, out obj))
        {
            if (obj == null)
            {
                Debug.LogError(assetPath + " 在本地资源字典中已经被为空，是否已经卸载掉了，请仔细检查！！！");
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 使用本地方式进行同步加载
    /// 方便快捷开发使用
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="callBack"></param>
    private void SyncResourcesLoadAsset(string assetPath, Action<object> callBack)
    {
        object obj = null;
        if (CheckLocalAssetInDic(assetPath, ref obj))
        {
            if (callBack == null)
            {
                callBack(obj);
            }
        }
        else
        {
            try
            {
                obj = Resources.Load(assetPath);
                curLoaclAssetDic[assetPath] = obj;
                if (callBack == null)
                {
                    callBack(obj);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(assetPath + " 加载错误 ！！！message " + e.Message + " StackTrace " + e.StackTrace);
            }
        }
    }

    /// <summary>
    /// 卸载掉使用resource来加载的的asset
    /// </summary>
    /// <param name="assetPath"></param>
    public void UnloadResourcesAsset(string assetPath)
    {
        object obj = null;
        if (curLoaclAssetDic.TryGetValue(assetPath, out obj))
        {
            obj = curLoaclAssetDic[assetPath];
            if (obj == null)
            {
                Debug.LogError(assetPath + " 在本地缓存列表中已经为null ，是否已经卸载掉！！！");
            }
            else
            {
                if (obj.GetType() != typeof(GameObject))
                {
                    Resources.UnloadAsset(obj as UnityEngine.Object);
                }
                obj = null;
                curLoaclAssetDic.Remove(assetPath);
            }
        }
        else
        {
            Debug.LogError(assetPath + " 不在本地缓存列表中，是否已将卸载掉！！！");

        }
    }

    /// <summary>
    /// 强制遍历游戏中所有的Objcet对象来卸载资源
    /// 消耗比较大
    /// 建议只在加载场景或者出现loading的时候来进行调用
    /// 参数备用是否来主动调用GC
    /// </summary>
    /// <param name="isActiveGC"></param>
    public void UnLoadForEveryObjct(bool isActiveGC = false)
    {
        Resources.UnloadUnusedAssets();
        if (isActiveGC)
        {
            GC.Collect();
        }
    }
}
