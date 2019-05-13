using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 普通按钮的监听脚本
/// 继承XLuaMonoBehaviour
/// </summary>
public class SButtonUGUI : XLuaMonoBehaviour
{
    [SerializeField]
    private ButtonAniType AniType = ButtonAniType.Scale;
    [SerializeField]
    private Sprite m_PressSprite;
    [SerializeField]
    private Sprite m_DisableSprite;
    [SerializeField]
    private EventTriggerListener listener;
    private Image m_Image;
    private Sprite m_NormalSprite;

    public EventTriggerListener Listener
    {
        get
        {
            return listener;
        }
        set
        {
            listener = value;
        }
    }

    public Sprite PressSprite
    {
        get
        {
            return m_PressSprite;
        }
        set
        {
            m_PressSprite = value;
        }
    }

    public Sprite DisableSprite
    {
        get
        {
            return m_DisableSprite;
        }
        set
        {
            m_DisableSprite = value;
        }
    }

    public bool Disable
    {
        set
        {
            Listener.enabled = value;
            if (m_Image != null && m_DisableSprite != null)
            {
                if (value)
                {
                    m_Image.sprite = m_DisableSprite;
                }
                else
                {
                    m_Image.sprite = m_NormalSprite;
                }
            }
        }
        get
        {
            return listener.enabled;
        }
    }

    public override void Awake()
    {
        base.Awake();
        m_Image = this.GetComponent<Image>();
        if (m_Image != null)
        {
            m_NormalSprite = m_Image.sprite;
        }
    }

    /// <summary>
    /// 原本想设置为委托但是想了下感觉没必要
    /// </summary>
    /// <param name="dlgate"></param>
    /// <param name="isclicksound"></param>
    /// <param name="isclickani"></param>
    public void SetClick(EventTriggerListener.VoidDelegate dlgate, bool isclicksound = true, bool isclickani = true)
    {
        if (dlgate != null)
        {
            Listener.onClick = dlgate;
            if (isclicksound)
            {
                Listener.onClickPlaySound = (GameObject go) =>
                {
                    SoundManager.I.PlaySound("buttonclick");//统一添加click音效
                };
            }
            if (isclickani)
            {
                Listener.onClickPlayAni = (GameObject go) =>
                {
                    if (AniType == ButtonAniType.Scale)
                    {
                        go.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutQuint).onComplete = () =>
                        {
                            go.transform.DOScale(Vector3.one * 1f, 0.1f).SetEase(Ease.OutQuint);
                        };
                    }
                };
            }


        }
    }

    public void SetDoubleClick(EventTriggerListener.VoidDelegate dlgate)
    {
        if (dlgate != null)
        {
            Listener.onDoubleClick = dlgate;
        }
    }

    public void SetDrag(EventTriggerListener.VectorDelegate dlgate)
    {
        if (dlgate != null)
        {
            Listener.onDrag = dlgate;
        }
    }

    public void SetPress(EventTriggerListener.BoolDelegate dlgate)
    {
        if (dlgate != null)
        {
            Listener.onPress = (GameObject obj, bool state) =>
            {
                dlgate(obj, state);
                if (state)
                {
                    if (m_Image != null && m_PressSprite != null)
                    {
                        m_Image.sprite = m_PressSprite;
                    }
                }
                else
                {
                    if (m_Image != null && m_NormalSprite != null)
                    {
                        m_Image.sprite = m_NormalSprite;
                    }
                }
            };
        }
    }

    public void SetDragStart(EventTriggerListener.VoidDelegate dlgate)
    {
        if (dlgate != null)
        {
            Listener.onDragStart = dlgate;
        }
    }

    public void SetDragOver(EventTriggerListener.VoidDelegate dlgate)
    {
        if (dlgate != null)
        {
            Listener.onDragOver = dlgate;
        }
    }

    public void SetDragOut(EventTriggerListener.VoidDelegate dlgate)
    {
        if (dlgate != null)
        {
            Listener.onDragOut = dlgate;
        }
    }

    public void SetDragEnd(EventTriggerListener.VoidDelegate dlgate)
    {
        if (dlgate != null)
        {
            Listener.onDragEnd = dlgate;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Listener.onClick = null;
        listener.onPress = null;
        listener.onDrag = null;
        listener.onDragStart = null;
        listener.onDragOver = null;
        listener.onDragEnd = null;
    }




}
