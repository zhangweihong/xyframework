using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;

/// <summary>
/// 对应的asset 基本信息
/// </summary>
public class AssetInfo
{
    /// <summary>
    /// 引入计数
    /// </summary>
    private int refer = 0;

    /// <summary>
    /// asset内部对象
    /// </summary>
    private object _asset;

    /// <summary>
    /// 等待加载完成的回调
    /// </summary>
    public Action<AssetInfo> WaiteFinishAction;

    /// <summary>
    /// 真实资源
    /// 慎重引用，有计数加入
    /// 必须在bundleinfo中引用
    /// </summary>
    public object Asset
    {
        get
        {
            if (this._asset != null)
            {
                this.refer++;
            }
            return this._asset;
        }
        set
        {
            this._asset = value;
        }
    }

    /// <summary>
    /// 资源类型
    /// </summary>
    public AssetType AssetOfType;

    /// <summary>
    /// 是否永驻内存
    /// </summary>
    public bool IsAlwaysInMemory = false;

    /// <summary>
    /// Asset的名字
    /// </summary>
    public string AssetName;

    /// <summary>
    /// 卸载资源
    /// 强制卸载的含义是不需要理睬
    /// 引用计数强制性卸载掉资源
    /// </summary>
    /// <param name="finishUnLoad"></param>
    /// <param name="forceUnload"></param>
    public void UnLoad(Action finishUnLoad, bool forceUnload = false)
    {

        if (forceUnload)
        {
            _Unload(finishUnLoad);
        }
        else
        {
            this.refer--;
            if (this.refer <= 0)
            {
                _Unload(finishUnLoad);
            }
        }
    }
    /// <summary>
    /// 真实卸载
    /// 回调的是为了从bundleinfo中移除
    /// </summary>
    /// <param name="finishUnLoad"></param>
    private void _Unload(Action finishUnLoad)
    {
        if (this.IsAlwaysInMemory)
        {
            Debug.LogWarning(AssetName + " 是永驻内存的注意内存释放！！！");
            return;
        }
        // if (AssetOfType != AssetType.None)
        // {
        //     if (AssetOfType != AssetType.GameObjct)
        //     {
        //         if (this._asset is Array)
        //         {
        //             UnityEngine.Object[] objs = this._asset as UnityEngine.Object[];
        //             for (int i = 0; i < objs.Length; i++)
        //             {
        //                 UnloadAsset(objs[i] as UnityEngine.Object);
        //             }
        //         }
        //         else if (this._asset is UnityEngine.Object)
        //         {
        //             UnloadAsset(this._asset as UnityEngine.Object);
        //         }
        //     }
        //     else
        //     {
        //         if (this._asset is Array)
        //         {
        //             UnityEngine.Object[] objs = this._asset as UnityEngine.Object[];
        //             for (int i = 0; i < objs.Length; i++)
        //             {
        //                 UnloadAsset(objs[i] as UnityEngine.Object);
        //             }
        //         }
        //         else if (this._asset is UnityEngine.Object)
        //         {
        //             UnloadAsset(this._asset as UnityEngine.Object);
        //         }
        //     }
        // }
        // else
        // {
        //     Type t = this._asset.GetType();
        //     if (t == typeof(Texture) || t == typeof(Texture2D) || t == typeof(Mesh) || t == typeof(AudioClip) || t == typeof(Material) || t == typeof(Shader) || t == typeof(SpriteAtlas) || t == typeof(AnimationClip))
        //     {
        //         if (this._asset is Array)
        //         {
        //             UnityEngine.Object[] objs = this._asset as UnityEngine.Object[];
        //             for (int i = 0; i < objs.Length; i++)
        //             {
        //                 UnloadAsset(objs[i] as UnityEngine.Object);
        //             }
        //         }
        //         else if (this._asset is UnityEngine.Object)
        //         {
        //             UnloadAsset(this._asset as UnityEngine.Object);
        //         }
        //     }
        //     else
        //     {
        //         if (this._asset is UnityEngine.Object[])
        //         {
        //             UnityEngine.Object[] objs = this._asset as UnityEngine.Object[];
        //             for (int i = 0; i < objs.Length; i++)
        //             {
        //                 UnloadAsset(objs[i] as UnityEngine.Object);
        //             }
        //         }
        //         else if (this._asset is UnityEngine.Object)
        //         {
        //             UnloadAsset(this._asset as UnityEngine.Object);
        //         }
        //     }
        // }
        if (this._asset is Array)
        {
            UnityEngine.Object[] objs = this._asset as UnityEngine.Object[];
            for (int i = 0; i < objs.Length; i++)
            {
                UnloadAsset(objs[i], AssetOfType);
            }
        }
        else if (this._asset is UnityEngine.Object)
        {
            UnloadAsset(this._asset, AssetOfType);
        }
        this._asset = null;
        this.refer = 0;
        this.WaiteFinishAction = null;
        this.AssetName = string.Empty;
        if (finishUnLoad != null)
        {
            finishUnLoad();
        }
    }

    private void UnloadAsset(object O, AssetType AssetOfType)
    {
        if (AssetOfType != AssetType.None && AssetOfType != AssetType.GameObjct)
        {
            Resources.UnloadAsset(O as UnityEngine.Object);
        }
        else
        {
            Type t = O.GetType();
            if (t == typeof(GameObject) || t == typeof(Component) || t == typeof(AssetBundle))
            {
                //什么也不做
            }
            else
            {
                Resources.UnloadAsset(O as UnityEngine.Object);
            }
        }
        // UnityEngine.Object.DestroyImmediate(O, true); // 资源会直接从bundle 里面卸载掉再也 不能加载出来了 慎用
    }
}
