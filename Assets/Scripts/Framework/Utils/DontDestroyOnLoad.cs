using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无视场景加载永不摧毁
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

}
