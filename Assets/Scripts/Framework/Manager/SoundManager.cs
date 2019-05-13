using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 声音管理器
/// </summary>
/// <typeparam name="SoundManager"></typeparam>
public class SoundManager : SingletonMono<SoundManager>
{
    /// <summary>
    /// 背景音乐音源
    /// </summary>
    private AudioSource m_BGMSource = null;
    /// <summary>
    /// 声效声音音源
    /// </summary>
    private AudioSource m_SoundSource = null;

    /// <summary>
    /// 背景音乐静音
    /// </summary>
    public bool MuteBgm
    {
        get
        {
            return m_MuteBgm;
        }
        set
        {
            m_MuteBgm = value;
            m_BGMSource.mute = value;
        }
    }
    private bool m_MuteBgm = false;
    /// <summary>
    /// 音效静音
    /// </summary>
    public bool MuteSound
    {
        get
        {
            return m_MuteSound;
        }
        set
        {
            m_MuteSound = value;
            m_SoundSource.mute = value;
        }
    }
    private bool m_MuteSound = false;

    /// <summary>
    /// fadein 的动画id
    /// </summary>
    private const int m_FadeInId = 2001;

    /// <summary>
    /// fadeout的动画id
    /// </summary>
    private const int m_FadeOutId = 2002;


    /// <summary>
    /// 音效声音大小 应该存入缓存或者网络数据
    /// </summary>
    private float m_SoundVolume = 1f;

    /// <summary>
    /// 背景音乐声音大小 应该存入缓存或者网络数据
    /// </summary>
    private float m_BgmVolume = 1f;
    /// <summary>
    /// 所有音效的 ab路径
    /// </summary>
    private const string m_SoundAbPath = "Sound/Sound";

    /// <summary>
    /// 所有Bgm的 ab路径
    /// </summary>
    private const string m_BgmAbPath = "Sound/Bgm";

    private Dictionary<string, AudioClip> m_AllClips = new Dictionary<string, AudioClip>();

    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    /// 预加载所有音效 bgm
    /// 我这里全部加载
    /// 有需求的可以改造
    /// 自己需要的加载方式
    ///</summary>
    public void PreLoadSound()
    {
        //加载音效  
        ResourcesManager.I.Load(m_SoundAbPath, m_SoundAbPath, (clips) =>
        {
            Object[] cliparry = clips as Object[];
            for (int i = 0; i < cliparry.Length; i++)
            {
                string clipname = System.IO.Path.GetFileNameWithoutExtension(cliparry[i].name);
                string key = StringUtil.AppendFormat("{0}/{1}", m_SoundAbPath, clipname);
                m_AllClips[key] = cliparry[i] as AudioClip;
            }
        }, AssetType.AudioClip, false, true, true);

        //加载bgm
        ResourcesManager.I.Load(m_BgmAbPath, m_BgmAbPath, (bgmclips) =>
        {
            Object[] bgmcliparry = bgmclips as Object[];
            for (int i = 0; i < bgmcliparry.Length; i++)
            {
                string clipname = System.IO.Path.GetFileNameWithoutExtension(bgmcliparry[i].name);
                string key = StringUtil.AppendFormat("{0}/{1}", m_BgmAbPath, clipname);
                m_AllClips[key] = bgmcliparry[i] as AudioClip;
            }
        }, AssetType.AudioClip, false, true, true);
    }

    /// <summary>
    /// 卸载所有音效
    /// </summary>
    public void UnloadSound()
    {
        m_AllClips.Clear();
        ResourcesManager.I.UnLoadAssetBundleAsset(m_SoundAbPath, m_SoundAbPath, true);
        ResourcesManager.I.UnLoadAssetBundleAsset(m_BgmAbPath, m_BgmAbPath, true);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        if (m_BGMSource == null)
        {
            m_BGMSource = this.gameObject.AddComponent<AudioSource>();
            m_BGMSource.playOnAwake = false;
        }
        if (m_SoundSource == null)
        {
            m_SoundSource = this.gameObject.AddComponent<AudioSource>();
            m_SoundSource.playOnAwake = false;
        }
        //设置音量
        m_BGMSource.volume = m_BgmVolume;
        m_BGMSource.loop = true;
        m_SoundSource.volume = m_SoundVolume;
    }

    /// <summary>
    /// 设置bgm
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isautoplay"></param>
    public void SetBgmClip(string name, bool isautoplay = true)
    {
        string key = StringUtil.AppendFormat("{0}/{1}", m_BgmAbPath, name);
        AudioClip clip = m_AllClips[key];
        SetBgmClip(clip, isautoplay);
    }
    /// <summary>
    /// 设置bgm的clip 并 自动播放
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="isautoplay"></param>
    public void SetBgmClip(AudioClip clip, bool isautoplay = true)
    {
        if (m_BGMSource.clip != null)
        {
            AnimationUtil.StopForID(m_FadeOutId);
            AnimationUtil.Fade(m_BGMSource, m_BGMSource.volume, 0.1f, m_FadeOutId, 0.1f, () =>
            {
                m_BGMSource.clip = clip;
                if (isautoplay)
                {
                    PlayBgm(true);
                }
            });
        }
        else
        {
            m_BGMSource.clip = clip;
            if (isautoplay)
            {
                PlayBgm(true);
            }
        }
    }

    /// <summary>
    /// bgm播放
    /// </summary>
    public void PlayBgm(bool isfade = true)
    {
        if (MuteBgm)
        {
            return;
        }
        if (true)
        {
            AnimationUtil.StopForID(m_FadeInId);
            AnimationUtil.Fade(m_BGMSource, 0.1f, m_BgmVolume, m_FadeInId);
        }

        m_BGMSource.Play();
    }

    /// <summary>
    /// 暂停bgm
    /// </summary>
    public void PauseBgm()
    {
        m_BGMSource.Pause();
    }

    /// <summary>
    /// 从暂停的bgm中继续
    /// </summary>
    public void UnPauseBgm()
    {
        m_BGMSource.UnPause();
    }

    /// <summary>
    /// 停止bgm播放
    /// </summary>
    public void StopBgm()
    {
        m_BGMSource.Stop();
    }

    /// <summary>
    /// 设置bgm音量大小
    /// </summary>
    /// <param name="v"></param>
    public void SetBgmVolume(float v)
    {
        m_BGMSource.volume = v;
    }

    /// <summary>
    /// 设置音效音量大小
    /// </summary>
    /// <param name="v"></param>
    public void SetSoundVolume(float v)
    {
        m_SoundSource.volume = v;
    }

    public void PlaySound(string name, bool isstopre = false, string langsetting = "zh")
    {
        string key = StringUtil.AppendFormat("{0}/{1}", m_SoundAbPath, name);
        string newkey = key;
        if (langsetting == "fzh")
        {
            newkey = StringUtil.AppendFormat("{0}_{1}", key, "fzh");
        }
        else if (langsetting == "en")
        {
            newkey = StringUtil.AppendFormat("{0}_{1}", key, "en");
        }
        AudioClip clip = null;
        if (m_AllClips.ContainsKey(newkey))
        {
            clip = m_AllClips[newkey];
        }
        else
        {
            clip = m_AllClips[key];
        }
        PlaySound(clip, isstopre);
    }

    /// <summary>
    /// 播放音效大小    
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySound(AudioClip clip, bool isstopre = false)
    {
        if (MuteSound)
        {
            return;
        }
        if (isstopre)
        {
            StopSound();
        }
        m_SoundSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 暂停音效
    /// </summary>
    public void PauseSound()
    {
        m_SoundSource.Pause();
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    public void StopSound()
    {
        m_SoundSource.Stop();
    }

}
