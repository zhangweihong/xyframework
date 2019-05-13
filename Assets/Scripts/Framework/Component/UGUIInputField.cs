using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 自使用大小
/// </summary>
public class UGUIInputField : InputField
{
    private float RESOULUTION_HEIGHT = GameSettingUtil.ScreenHeight;
    private Vector2 m_OriginAnchorPos;
    private RectTransform m_Rtr;
    private bool m_Calc = false;
    // Use this for initialization
    void Start()
    {
        m_Rtr = this.GetComponent<RectTransform>();
        this.onEndEdit.AddListener(this.OnEndEdit);
        m_OriginAnchorPos = this.m_Rtr.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Calc)
        {
            return;
        }
        if (this.isFocused)
        {
            m_Calc = true;
            float keyboardHeight = SDKManager.I.KeyboardHeight() * RESOULUTION_HEIGHT / Screen.height;
            this.m_Rtr.anchoredPosition = Vector3.up * (keyboardHeight);
        }
    }

    void OnEndEdit(string str)
    {
        m_Calc = false;
        this.m_Rtr.anchoredPosition = m_OriginAnchorPos;
    }
}
