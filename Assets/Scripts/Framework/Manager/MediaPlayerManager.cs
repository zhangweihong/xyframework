using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using RenderHeads.Media.AVProVideo;

/// <summary>
/// 播放模式
/// </summary>
public enum MediaPlayMode
{
    Full,
    Half,
    Small
}
/// <summary>
/// 播放管理器
/// </summary>
public class MediaPlayerManager : SingletonMono<MediaPlayerManager>
{
    // [SerializeField]
    // private MediaPlayer m_MainPlayer;
    // private int width = 0;
    // private int height = 0;

    // void Start()
    // {
    // }

    // public void Init()
    // {
    //     width = GameSettingUtil.ScreenWidth;
    //     height = GameSettingUtil.ScreenHeight;
    //     GameObject mediaObj = Resources.Load("Local/MediaPlayer") as GameObject;
    //     mediaObj = GameObject.Instantiate(mediaObj, App.I.CanvasTr);
    //     m_MainPlayer = mediaObj.GetComponent<MediaPlayer>();
    //     m_MainPlayer.gameObject.SetActive(false);
    // }

    // /// <summary>
    // /// 使用主播放器 播放视频
    // /// </summary>
    // /// <param name="urlOrAbspath"></param>
    // /// <param name="isAutoPlay"></param>
    // /// <returns></returns>
    // public MediaPlayer UseMainPlay(string urlOrAbspath, bool isAutoPlay, bool isautoclose = true)
    // {
    //     if (!m_MainPlayer.gameObject.activeInHierarchy)
    //     {
    //         m_MainPlayer.gameObject.SetActive(true);
    //     }
    //     bool isCreatSucess = m_MainPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, urlOrAbspath, isAutoPlay);
    //     m_MainPlayer.Events.AddListener((mp, et, errorCode) =>
    //     {
    //         switch (et)
    //         {
    //             case MediaPlayerEvent.EventType.ReadyToPlay:
    //                 mp.Play();
    //                 break;
    //             case MediaPlayerEvent.EventType.FirstFrameReady:
    //                 break;
    //             case MediaPlayerEvent.EventType.FinishedPlaying:
    //                 mp.CloseVideo();
    //                 m_MainPlayer.gameObject.SetActive(false);
    //                 break;
    //         }
    //     });
    //     if (!isCreatSucess)
    //     {
    //         Debug.LogError("创建视频失败！请检查什么原因！！！");
    //     }
    //     return m_MainPlayer;
    // }
}
