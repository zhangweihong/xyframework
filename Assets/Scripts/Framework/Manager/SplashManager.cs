using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 顺序展示图片splash 为了方便又N个splash需要显示
/// </summary>
/// <typeparam name="SplashManager"></typeparam>
public partial class SplashManager : SingletonMono<SplashManager>
{
    public GameObject m_SplashObj;
    public Image[] m_RawImgArry;
    public App m_App;
    private int m_Index = 0;
    void Awake()
    {
    }

    void Start()
    {
        m_SplashObj.SetActive(true);
        m_Index = 0;
        BeginSplashAni();
    }

    private void BeginSplashAni()
    {
        if (m_Index >= m_RawImgArry.Length)
        {
            m_App.enabled = true;
            m_SplashObj.SetActive(false);
            return;
        }
        m_RawImgArry[m_Index].gameObject.SetActive(true);
        AnimationUtil.Fade(m_RawImgArry[m_Index], 0, 1, 2f, () =>
        {
            m_RawImgArry[m_Index].gameObject.SetActive(false);
            m_Index++;
            BeginSplashAni();
        }, DG.Tweening.Ease.Linear);
    }


}