using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;

/// <summary>
/// xlua在 cs 层 mono周期驱动函数
/// </summary>
public class XLuaMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// lua脚本的相对路径
    /// </summary>
    [SerializeField]
    private string m_luaRelativePath = string.Empty;

    /// <summary>
    /// lua层上的view 脚本
    /// </summary>
    private TextAsset m_luaViewText = null;

    /// <summary>
    /// 序列化的对象可注册到脚本中
    /// </summary>
    [SerializeField]
    private SerializeObject[] m_SerializeObjects;

    /// <summary>
    /// lua脚本的C#对象
    /// </summary>
    private LuaTable m_luaScriptObject;

    /// <summary>
    /// lua脚本对象awake的方法的C#对象
    /// </summary>
    private Action luaAwake;

    private Action luaEnable;

    private Action luaDisable;


    /// <summary>
    /// lua脚本对象start的方法的C#对象
    /// </summary>
    private Action luaStart;

    /// <summary>
    /// lua脚本对象update的方法的C#对象
    /// </summary>
    private Action luaUpdate;

    /// <summary>
    /// lua脚本对象ondestroy的方法的C#对象
    /// </summary>
    private Action luaOnDestroy;

    public SerializeObject[] SerializeObjects
    {
        set
        {
            m_SerializeObjects = value;
        }
        get
        {
            return m_SerializeObjects;
        }
    }

    public string LuaRelativePath
    {
        get
        {
            return m_luaRelativePath;
        }
#if UNITY_EDITOR
        set
        {
            m_luaRelativePath = value;
        }
#endif
    }

    public virtual void Awake()
    {
        InitLuaObject();
    }

    /// <summary>
    /// 初始化脚本 注入参数和函数等等
    /// </summary>
    public void InitLuaObject()
    {
        if (string.IsNullOrEmpty(m_luaRelativePath))
        {
            return;
        }
        if (m_luaScriptObject == null)
        {
            if (m_luaViewText == null)
            {
            }
            m_luaViewText = XLuaManager.I.LoadTextAsset(m_luaRelativePath);
            m_luaScriptObject = XLuaManager.I.LoadLua(m_luaViewText.text, this.gameObject.name, null)[0] as LuaTable;
            m_luaScriptObject.Set("self", this);
            m_luaScriptObject.Set("go", this.gameObject);
            for (int i = 0; i < m_SerializeObjects.Length; i++)
            {
                if (m_SerializeObjects[i] != null && m_SerializeObjects[i] != null && !string.IsNullOrEmpty(m_SerializeObjects[i].Name))
                {
                    m_luaScriptObject.Set(m_SerializeObjects[i].Name, m_SerializeObjects[i].Value);
                }
            }
            m_luaScriptObject.Get("awake", out luaAwake);
            m_luaScriptObject.Get("enable", out luaEnable);
            m_luaScriptObject.Get("disable", out luaDisable);
            m_luaScriptObject.Get("start", out luaStart);
            m_luaScriptObject.Get("update", out luaUpdate);
            m_luaScriptObject.Get("ondestroy", out luaOnDestroy);
            if (luaAwake != null)
            {
                luaAwake();
            }
        }
    }

    public LuaTable GetLuaObj()
    {
        InitLuaObject();
        return m_luaScriptObject;
    }

    public virtual void OnEnable()
    {

        if (luaEnable != null)
        {
            luaEnable();
        }
    }

    public virtual void OnDisable()
    {
        if (luaDisable != null)
        {
            luaDisable();
        }
    }

    public virtual void Start()
    {
        if (luaStart != null)
        {
            luaStart();
        }
    }

    public virtual void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
    }

    public virtual void OnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy();
        }
        luaOnDestroy = null;
        luaUpdate = null;
        luaStart = null;
        luaAwake = null;
        if (m_luaScriptObject != null)
        {
            m_luaScriptObject.Dispose();
        }
        m_SerializeObjects = null;
    }
}
