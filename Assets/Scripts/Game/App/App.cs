using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏主入口
/// </summary>
public class App : SingletonMono<App>
{
    [SerializeField]
    private Camera m_UICamera;
    private GameObject obj;
    private Transform tr;
    public Transform CanvasTr;

    private System.Action m_AppPauseCall = null;
    private System.Action m_AppResumeCall = null;

    protected override void Awake()
    {
        Application.targetFrameRate = 100;
        this.obj = this.gameObject;
        this.tr = this.transform;
    }
    private void Start()
    {
        InitGame();
        this.InitAppLua();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitGame()
    {
#if OutLog
        this.gameObject.AddComponent<LogManager>();
        this.gameObject.AddComponent<FPSInfo>();
#if UNITY_EDITOR
        this.gameObject.AddComponent<DebugUILine>();
#endif 
#endif
        this.gameObject.AddComponent<XLuaManager>().Init();
        this.gameObject.AddComponent<DownLoadManager>().Init();
        this.gameObject.AddComponent<ResourcesManager>().Init();
        this.gameObject.AddComponent<SDKManager>().Init();
        this.gameObject.AddComponent<VersionManager>().Init();
        this.gameObject.AddComponent<SoundManager>().Init();
        this.gameObject.AddComponent<DelayTimeexeManager>().Init();
        SimplePoolManager.I.Init();// 初始化缓存池
        ResourcesManager.I.PreLoad();//因为有预加载和版本更新有密切关系
        SoundManager.I.PreLoadSound();//因为有预加载和版本更新有密切关系
    }

    /// <summary>
    /// lua主入口
    /// </summary>
    private void InitAppLua()
    {
        XLua.LuaTable apptable = XLuaManager.I.LoadAppLua();
        apptable.Get("pause", out m_AppPauseCall);
        apptable.Get("resume", out m_AppResumeCall);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
        {
            if (m_AppPauseCall != null)
            {
                m_AppPauseCall();
            }
        }
        else
        {
            if (m_AppResumeCall != null)
            {
                m_AppResumeCall();
            }
        }
    }
}
