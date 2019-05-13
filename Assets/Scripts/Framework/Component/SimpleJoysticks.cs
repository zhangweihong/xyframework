using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
///  虚拟摇杆
/// </summary>
public class SimpleJoysticks : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject JoyRoot;
    public System.Action<Vector2, float> JoysticksCallBack;
    public System.Action JoysticksEndCallBack;
    public RectTransform m_CanvasRtr;
    public RectTransform m_CanvasRootRtr;
    public Transform m_RollTr;
    //虚拟方向按钮初始位置
    public Vector3 InitPosition;
    public Vector3 InitLocalPosition;
    //虚拟方向按钮可移动的半径
    public float r;
    private float m_ystrength = 0;
    private float m_xstrength = 0;

    private RectTransform m_SelfRtr;

    void Start()
    {
        InitPosition = transform.localPosition;
        m_SelfRtr = this.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 pos = Util.GetLocalPosition(m_CanvasRootRtr, Input.mousePosition, Camera.main);
        JoyRoot.transform.localPosition = pos - new Vector3(200, 200, 0);
        m_SelfRtr.sizeDelta = Vector2.one * 88;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_SelfRtr.sizeDelta = Vector2.one * 400;
        JoyRoot.transform.localPosition = new Vector3(-577, -322, 0);
    }

    //鼠标拖拽
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mpos = Vector3.zero;
        mpos = Util.GetLocalPosition(m_CanvasRtr, Input.mousePosition, Camera.main);
        float dis = Vector3.Distance(mpos, InitPosition);
        Vector3 dir = mpos - InitPosition;
        if (dis < r)
        {
            transform.localPosition = mpos;
        }
        else
        {
            transform.localPosition = InitPosition + dir.normalized * r;
        }
        m_xstrength = (transform.localPosition.x - InitPosition.x) / r;
        m_ystrength = (transform.localPosition.y - InitPosition.y) / r;
        m_xstrength = m_xstrength > 0.95 ? 1 : m_xstrength;
        m_ystrength = m_ystrength > 0.95 ? 1 : m_ystrength;

        m_xstrength = m_xstrength < -0.95 ? -1 : m_xstrength;
        m_ystrength = m_ystrength < -0.95 ? -1 : m_ystrength;

        Vector2 s = new Vector2(m_xstrength, m_ystrength);
        float p = s.magnitude;
        m_RollTr.position = transform.position;
        if (JoysticksCallBack != null)
        {
            JoysticksCallBack(s, p);
        }
    }

    //鼠标松开
    public void OnEndDrag(PointerEventData eventData)
    {
        //松开鼠标虚拟摇杆回到原点
        transform.localPosition = InitPosition;
        if (JoysticksEndCallBack != null)
        {
            JoysticksEndCallBack();
        }
        m_RollTr.position = transform.position;
    }
}
