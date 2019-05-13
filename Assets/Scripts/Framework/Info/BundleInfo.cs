using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

/// <summary>
/// 对应的 assetbundle的基本信息
/// </summary>
public class BundleInfo
{
    /// <summary>
    /// 引用计数
    /// </summary>
    public int Refer = 0;

    /// <summary>
    /// 当前拥有的所有的asset字典
    /// </summary>
    private Dictionary<string, AssetInfo> curAssetDic = new Dictionary<string, AssetInfo>();

    /// <summary>
    /// 正在加载的asset
    /// </summary>
    private Dictionary<string, AssetInfo> loaingAssetDic = new Dictionary<string, AssetInfo>();


    /// <summary>
    /// 当前的Ab对象
    /// </summary>
    public AssetBundle AssetBundle;

    /// <summary>
    /// 当前的Ab的文件名
    /// </summary>
    public string AssetbundleName;

    /// <summary>
    /// 等待加载完成的回调
    /// </summary>
    public Action<BundleInfo> WaiteFinishAction;

    /// <summary>
    /// 异步加载资源
    /// 说是异步 也是需要耗费 主线程的
    /// 慎用 针对 大于700K的资源 
    /// 估计在一般设备上会产生IO卡顿
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="callBack"></param>
    /// <param name="isAlwaysInMemory"></param>
    /// <returns></returns>
    public IEnumerator AsyncLoadAsset(string assetName, Action<object> callBack, AssetType type = AssetType.None, bool isAlwaysInMemory = false, bool isall = false)
    {
        object asset = null;
        AssetInfo assetInfo = null;

        if (loaingAssetDic.TryGetValue(assetName, out assetInfo))
        {
            if (assetInfo != null)
            {
                assetInfo.WaiteFinishAction += (AssetInfo newInfo) =>
                {
                    if (callBack != null)
                    {
                        callBack(assetInfo.Asset);
                    }
                };
                yield break;
            }
        }

        if (curAssetDic.TryGetValue(assetName, out assetInfo))
        {
            asset = assetInfo.Asset;
        }
        if (asset == null)
        {
            if (AssetBundle != null)
            {
                assetInfo = new AssetInfo
                {
                    AssetName = assetName,
                    AssetOfType = type
                };
                loaingAssetDic[assetName] = assetInfo;
                AssetBundleRequest assetbundleReq = null;
                if (isall)
                {
                    assetbundleReq = AssetBundle.LoadAllAssetsAsync();
                }
                else
                {
                    assetbundleReq = AssetBundle.LoadAssetAsync(assetName);

                }
                yield return assetbundleReq;
                loaingAssetDic.Remove(assetName);
                if (assetbundleReq.isDone)
                {
                    assetInfo.IsAlwaysInMemory = isAlwaysInMemory;

                    if (isall)
                    {
                        assetInfo.Asset = assetbundleReq.allAssets;
                    }
                    else
                    {
                        assetInfo.Asset = assetbundleReq.asset;
                    }


                    if (assetInfo.WaiteFinishAction != null)
                    {
                        assetInfo.WaiteFinishAction(assetInfo);
                    }
                    assetInfo.WaiteFinishAction = null;
                    asset = assetInfo.Asset;
                    curAssetDic[assetName] = assetInfo;
                }
                else
                {
                    Debug.LogError(assetName + " 加载出错 ！！！");
                }
            }
            else
            {
                Debug.LogError(AssetBundle + " 为null 确定是不是已经被卸载了！！！");
            }
        }

        if (callBack != null)
        {
            callBack(asset);
        }
        yield return 0;
    }

    /// <summary>
    /// 同步加载资源
    /// 建议只在loading的是使用不然
    /// 在游戏中使用的话会造成卡顿
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="callBack"></param>
    /// <param name="isAlwaysInMemory"></param>
    public void SyncLoadAsset(string assetName, Action<object> callBack, AssetType type = AssetType.None, bool isAlwaysInMemory = false, bool isall = false)
    {
        object asset = null;
        AssetInfo assetInfo = null;

        if (loaingAssetDic.TryGetValue(assetName, out assetInfo))
        {
            if (assetInfo != null)
            {
                assetInfo.WaiteFinishAction = (AssetInfo newInfo) =>
                {
                    if (callBack != null)
                    {
                        callBack(assetInfo.Asset);
                    }
                };
            }
        }

        if (curAssetDic.TryGetValue(assetName, out assetInfo))
        {
            asset = assetInfo.Asset;
        }
        if (asset == null)
        {
            if (AssetBundle != null)
            {
                assetInfo = new AssetInfo();
                loaingAssetDic[assetName] = assetInfo;
                assetInfo.AssetName = assetName;
                assetInfo.AssetOfType = type;
                assetInfo.IsAlwaysInMemory = isAlwaysInMemory;
                if (isall)
                {
                    assetInfo.Asset = AssetBundle.LoadAllAssets();
                }
                else
                {
                    assetInfo.Asset = AssetBundle.LoadAsset(assetName);
                }
                loaingAssetDic.Remove(assetName);
                if (assetInfo.WaiteFinishAction != null)
                {
                    assetInfo.WaiteFinishAction(assetInfo);
                }
                assetInfo.WaiteFinishAction = null;
                asset = assetInfo.Asset;
            }
            else
            {
                Debug.LogError(AssetBundle + " 为null 确定是不是已经被卸载了！！！");
            }
        }

        if (callBack != null)
        {
            callBack(asset);
        }
    }

    /// <summary>
    /// 卸载asset
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="forceUnLoad"></param>
    public void UnLoad(string assetName, bool forceUnLoad = false)
    {
        AssetInfo assetInfo = null;
        curAssetDic.TryGetValue(assetName, out assetInfo);
        if (assetInfo != null)
        {
            assetInfo.UnLoad(() =>
            {
                curAssetDic.Remove(assetName);
            }, forceUnLoad);
        }
        else
        {
            Debug.LogError(assetName + "为null,请检查是不是已经卸载过了 卸载失败！！！");
        }
    }
}
