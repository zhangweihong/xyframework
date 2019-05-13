using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 缓存类型
/// </summary>
public enum SimplePoolType
{
    None,
    Skill,
    UI,
    Model,
    Effect,
    Other
}

/// <summary>
/// 简单缓存池
/// </summary>
public class SimplePool
{
    public SimplePoolType Tp = SimplePoolType.None;
    public double CurTime = 0;
    public string PoolName = ""; //缓存池的名字  
    private List<Object> Spawnls = new List<Object>(); //显示列表  
    private List<Object> DeSpawnls = new List<Object>(); //隐藏列表  
    public Object Spawn()
    { //从缓存中取  
        if (DeSpawnls.Count > 0)
        {
            Object o = DeSpawnls[0];
            if (!Spawnls.Contains(o))
            {
                Spawnls.Add(o);
            }
            DeSpawnls.RemoveAt(0);
            return o;
        }
        else
        {
            return null;
        }
    }

    public void Despawn(Object obj)
    {
        if (Spawnls.Contains(obj))
        {
            Spawnls.Remove(obj);
        }
        if (!DeSpawnls.Contains(obj))
        {
            DeSpawnls.Add(obj);
        }
    }

    public int DeSpawnCount
    {
        get
        {
            return DeSpawnls != null ? DeSpawnls.Count : 0;
        }
    }

    public int SpawnCount
    {
        get
        {
            return DeSpawnls != null ? DeSpawnls.Count : 0;
        }
    }

    public Object CreateObject(Object o)
    { //创建  
        Object obj = Object.Instantiate(o);
        obj.name = o.name;
        Spawnls.Add(obj);
        return obj;
    }

    public void CleanAll()
    {
        for (int i = 0; i < Spawnls.Count; i++)
        {
            if (Spawnls[i] != null)
            {
                GameObject.Destroy(Spawnls[i]);
            }
        }
        for (int i = 0; i < DeSpawnls.Count; i++)
        {
            if (DeSpawnls[i] != null)
            {
                GameObject.Destroy(DeSpawnls[i]);
            }
        }
        Spawnls.Clear();
        DeSpawnls.Clear();
    }

    public void CleanDeSpawnLs()
    {
        for (int i = 0, sCount = DeSpawnls.Count; i < sCount; i++)
        {
            if (DeSpawnls[i] != null)
            {
                GameObject.Destroy(DeSpawnls[i]);
            }
        }
        DeSpawnls.Clear();
    }
}

/// <summary>
/// 缓存池管理
/// </summary>
/// <typeparam name="SimplePoolManager"></typeparam>
public class SimplePoolManager : Singleton<SimplePoolManager>
{
    private Transform m_PoolRoot;
    private Dictionary<string, SimplePool> SimplePoolDic = new Dictionary<string, SimplePool>(); //特效gameobject缓存池 防止频繁的

    public void Init()
    {
        m_PoolRoot = GameObject.Find("DontDestroy/PoolRoot").transform;
    }

    /// <summary>
    /// 从缓存中取出来
    /// </summary>
    /// <param name="o"></param>
    /// <param name="poolname"></param>
    /// <returns></returns>
    public Object GetObjectFromCachePool(Object o, string poolname)
    {
        if (o == null)
        {
            Debug.LogError("缓存池获取失败 传入的值为 null");
            return null;
        }
        Object newo = null;
        SimplePool pool;
        if (SimplePoolDic.TryGetValue(poolname, out pool))
        {
            newo = pool.Spawn();
            if (newo == null)
            {
                newo = pool.CreateObject(o);
            }
        }
        else
        {
            pool = new SimplePool();
            pool.PoolName = poolname;
            newo = pool.CreateObject(o);
            SimplePoolDic[poolname] = pool;
        }
        return newo;
    }

    /// <summary>
    /// 回收到缓存池中
    /// </summary>
    /// <param name="o"></param>
    /// <param name="poolname"></param>
    public void DeSpawnObject(Object o, string poolname)
    {
        if (o == null)
        {
            Debug.LogError("缓存池返还失败 传入的值为 null");
            return;
        }
        SimplePool pool = SimplePoolDic[poolname];
        if (pool != null)
        {
            pool.Despawn(o);
            if (o is GameObject)
            {
                GameObject obj = (o as GameObject);
                if (obj != null)
                {
                    obj.transform.SetParent(m_PoolRoot);
                }
                obj.SetActive(false);

            }
        }
    }
    /// <summary>
    /// 强制卸载缓存池
    /// </summary>
    /// <param name="poolname"></param>
    public void ForceClearPool(string poolname)
    {
        SimplePool pool = SimplePoolDic[poolname];
        if (pool != null)
        {
            pool.CleanAll();
        }

    }
}
