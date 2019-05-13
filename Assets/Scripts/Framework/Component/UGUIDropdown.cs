using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 简单重写了 dropdown 
/// </summary>
public class UGUIDropdown : Dropdown
{
    private UnityAction<int> OnChangeAction;
    private System.Action OnFinishAction;
    // Use this for initialization

    public void SetDropDownChangeActionAndFinishAction(UnityAction<int> changaction, System.Action finishaction = null)
    {
        this.onValueChanged.RemoveAllListeners();
        OnFinishAction = finishaction;
        OnChangeAction = changaction;
        this.onValueChanged.AddListener(changaction);
    }
}
