using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// SDK 相关管理
/// /// </summary>
/// <typeparam name="SDKManager"></typeparam>
public class SDKManager : SingletonMono<SDKManager>
{
#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern void InitSDK(string gameobjectname);
	[DllImport ("__Internal")]
	private static extern string GetIOSLanguage();
    [DllImport ("__Internal")]
	private static extern void GetGPS();
    [DllImport ("__Internal")]
	private static extern int GetIOSVoice();
    [DllImport ("__Internal")]
	private static extern void ExitApplication();
#endif
    private AndroidJavaObject m_AndroidObj = null;
    private System.Action<string> m_GpsCallBack = null;

    public void Init()
    {
#if UNITY_EDITOR
#elif UNITY_ANDROID
	try
	{
			if (m_AndroidObj == null)
			{
				AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
    			m_AndroidObj = jc.GetStatic<AndroidJavaObject> ("currentActivity");
				m_AndroidObj.Call("Init",this.gameObject.name);
			}
	}
	catch (System.Exception e)
	{
		
		Debug.LogError(e.ToString());
	}
		
#elif UNITY_IOS
	InitSDK(this.gameObject.name);
#endif
    }

    public string GetLanguage()
    {
        string lang = "zh_cn";
#if UNITY_EDITOR
#elif UNITY_ANDROID
		lang = m_AndroidObj.Call<string>("GetLanguage");
#elif UNITY_IOS
		lang = GetIOSLanguage();
#endif
        lang = lang.ToLower();
        if (lang.Contains("zh"))
        {
            if (lang.Contains("cn"))
            {
                lang = "zh";
            }
            else
            {
                lang = "fzh";
            }
        }
        else if (lang.Contains("en"))
        {
            lang = "en";
        }
        else
        {
            lang = "en";
        }
        return lang;
    }

    /// <summary>
    /// 设置GPS回调
    /// </summary>
    /// <param name="gpscb"></param>
    public void SetGPS(System.Action<string> gpscb)
    {
        m_GpsCallBack = gpscb;
#if UNITY_EDITOR
#elif UNITY_ANDROID
		m_AndroidObj.Call("GetGps");
#elif UNITY_IOS
        GetGPS();
#endif
    }

    private void GPSCallBack(string gps)
    {
        if (m_GpsCallBack != null)
        {
            m_GpsCallBack(gps);
        }
    }

    /// <summary>
    /// 获取声音 大小 可能api 有点问题 但是能用
    /// </summary>
    /// <returns></returns>
    public int GetVoice()
    {
        int v = 0;
#if UNITY_EDITOR
#elif UNITY_ANDROID
		v = m_AndroidObj.Call<int>("GetVoice");
#elif UNITY_IOS
        v = GetIOSVoice();
#endif
        return v;
    }

    /// <summary>
    /// 退出app IOS上 application.quit 方法无效只能调用oc代码 闪退处理
    /// </summary>
    public void ExitApp()
    {
#if UNITY_EDITOR
#elif UNITY_IOS
    ExitApplication();
#endif
    }

    public float KeyboardHeight()
    {
        float h = 0;

#if UNITY_EDITOR
#elif UNITY_ANDROID
    h = AndroidKeyboardHeight();
#elif UNITY_IOS
    h = TouchScreenKeyboard.area.height;
#endif
        return h;
    }

    private float AndroidKeyboardHeight()
    {
        using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").
                Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

            using (AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect"))
            {
                View.Call("getWindowVisibleDisplayFrame", Rct);
                return Screen.height - Rct.Call<int>("height");
            }
        }
    }
}
