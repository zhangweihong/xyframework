using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 延迟调用结构
/// </summary>
public class DelayFunStruct
{
    public float DelayTime;
    public Action Fun;
    public string Key;
    public DelayFunStruct(float dt, Action callfun)
    {
        this.Fun = callfun;
        this.DelayTime = dt;
    }
}

/// <summary>
/// 延迟调用管理器
/// </summary>
/// <typeparam name="DelayTimeexeManager"></typeparam>
public class DelayTimeexeManager : SingletonMono<DelayTimeexeManager>
{
    private List<DelayFunStruct> m_DelayFunLs = new List<DelayFunStruct>();
    public void Init()
    {
    }

    public void Add(DelayFunStruct ds)
    {
        m_DelayFunLs.Add(ds);
    }

    public void RemoveWithKey(string key)
    {
        int index = m_DelayFunLs.FindIndex((item) => item.Key == key);
        if (index > -1)
        {
            m_DelayFunLs.RemoveAt(index);
        }
    }

    public void Clear()
    {
        m_DelayFunLs.Clear();
    }

    void Update()
    {
        for (int i = 0; i < m_DelayFunLs.Count; i++)
        {
            DelayFunStruct ds = m_DelayFunLs[i];
            ds.DelayTime -= Time.deltaTime;
            if (ds.DelayTime <= 0)
            {
                if (ds.Fun != null)
                {
                    ds.Fun();
                }
                if (m_DelayFunLs.Contains(ds))
                {
                    m_DelayFunLs.Remove(ds);
                }
            }
        }
    }
}
