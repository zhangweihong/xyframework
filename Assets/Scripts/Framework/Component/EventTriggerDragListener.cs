using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 带有drag事件的监听脚本
/// </summary>
public class EventTriggerDragListener : EventTriggerListener, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public UGUIScrollRect ScrollRect;
    // Use this for initialization
    void Start()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (onDragStart != null)
        {
            onDragStart(this.gameObject);
        }
        if (ScrollRect != null)
        {
            ScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null)
        {
            onDrag(this.gameObject, eventData.delta);
        }
        if (ScrollRect != null)
        {
            ScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (onDragEnd != null)
        {
            onDragEnd(this.gameObject);
        }
        if (ScrollRect != null)
        {
            ScrollRect.OnEndDrag(eventData);
        }
    }
}
