using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 点击 和 按下 时间的监听脚本 
/// 没有加drag相关的主要是为了
/// 防止当前对象在scroll子节点下
/// drag事件会自动被覆盖掉
/// /// </summary>
public class EventTriggerListener : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IPointerExitHandler
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void BoolDelegate(GameObject go, bool state);
    public delegate void FloatDelegate(GameObject go, float delta);
    public delegate void VectorDelegate(GameObject go, Vector2 delta);
    public delegate void ObjectDelegate(GameObject go, GameObject obj);
    public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

    public object parameter;

    public VoidDelegate onSubmit;
    public VoidDelegate onClick;
    public VoidDelegate onClickPlaySound;
    public VoidDelegate onClickPlayAni;

    public VoidDelegate onDoubleClick;
    public BoolDelegate onHover;
    public BoolDelegate onPress;
    public BoolDelegate onSelect;
    public FloatDelegate onScroll;
    public VoidDelegate onDragStart;
    public VectorDelegate onDrag;
    public VoidDelegate onDragOver;
    public VoidDelegate onDragOut;
    public VoidDelegate onDragEnd;
    public ObjectDelegate onDrop;
    public KeyCodeDelegate onKey;
    public BoolDelegate onTooltip;

    void Start()
    {

    }

    private float clickt1;
    private float clickt2;
    // Use this for initialization
    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
        {
            onClick(this.gameObject);

        }

        if (onClickPlaySound != null)
        {
            onClickPlaySound(this.gameObject);
        }

        if (onClickPlayAni != null)
        {
            onClickPlayAni(this.gameObject);
        }

        clickt2 = Time.realtimeSinceStartup;
        if (clickt2 - clickt1 < 0.2f)
        {
            if (onDoubleClick != null)
            {
                onDoubleClick(this.gameObject);
            }
        }
        clickt1 = clickt2;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onPress != null)
        {
            onPress(this.gameObject, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onPress != null)
        {
            onPress(this.gameObject, false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onDragOut != null)
        {
            onDragOut(this.gameObject);
        }
    }

}
