using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 简单重写了slider
/// 为了获取拖拽结束事件
/// </summary>
public class UGUISlider : Slider
{
    private UnityAction<float> OnChangeAction;
    private System.Action OnFinishAction;


    public void SetSliderChangeActionAndFinishAction(UnityAction<float> changaction, System.Action finishaction)
    {
        this.onValueChanged.RemoveAllListeners();
        OnChangeAction = changaction;
        OnFinishAction = finishaction;
        this.onValueChanged.AddListener(OnChangeAction);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (OnFinishAction != null)
        {
            OnFinishAction();
        }

    }
}
