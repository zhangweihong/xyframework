using UnityEngine;
using System.Collections;

/// <summary>
/// 简单单例基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    static protected T mInstance;
    static public T I
    {
        get
        {
            if (mInstance == null)
                mInstance = new T();
            return mInstance;
        }
    }
}