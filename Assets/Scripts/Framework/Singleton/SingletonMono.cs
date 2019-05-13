using UnityEngine;
using System.Collections;

/// <summary>
/// 基于mono的简单单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T instance;
    public static T I
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                if (instance == null)
                {
                    Debug.LogError(typeof(T) + " is nothing");
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        CheckInstance();
    }

    protected bool CheckInstance()
    {
        if (this == I) { return true; }
        Destroy(this);
        return false;
    }

    static public bool IsValid()
    {
        return (instance != null);
    }
}