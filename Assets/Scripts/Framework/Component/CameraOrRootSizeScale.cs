using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 双手指缩放函数（可以使用范围 一般app里面的图片两指缩放的效果）
/// 相机和对象必须有一个
/// </summary>
public class CameraOrRootSizeScale : MonoBehaviour
{
    /// <summary>
    /// 缩放的相机
    /// </summary>
    [SerializeField]
    private Camera m_Camera;
    /// <summary>
    ///缩放的对象
    /// </summary>
    [SerializeField]
    private GameObject m_ScaleRoot;
    /// <summary>
    /// 如果缩放在一个scroll的子节点上
    /// </summary>
    [SerializeField]
    private ScrollRect m_Scroll;
    public float maxDistance = 1f;
    public float minDistance = 0.5f;
    public float scaleEdiorFactor = 1f;

    private Vector2 lastSingleTouchPosition;

    private Vector2 nowSingleTouchPosition;

    private float bornscale = 1f;
    private float nowscale = 1f;

    private bool m_IsSingleFinger = false;

    private Vector2 oldPosition1;
    private Vector2 oldPosition2;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_Camera == null && m_ScaleRoot == null)
        {
            return;
        }
#if UNITY_EDITOR
        bornscale -= Input.GetAxis("Mouse ScrollWheel") * scaleEdiorFactor;
        bornscale = Mathf.Clamp(bornscale, minDistance, maxDistance);
        if (Input.GetMouseButtonDown(0))
        {
            lastSingleTouchPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            if (m_Camera != null)
            {
                m_Camera.orthographicSize = bornscale;
            }
            if (m_ScaleRoot != null)
            {
                m_ScaleRoot.transform.localScale = Vector3.one * bornscale;
            }
        }
#else

        if(Input.touchCount == 0)
        {
            if(m_Scroll != null && m_Scroll.enabled == false){
				m_Scroll.enabled = true;
			}
        }
		if (Input.touchCount == 1)
		{
            m_IsSingleFinger = true;
		}

		if (Input.touchCount == 2)
		{
            if(m_Scroll != null && m_Scroll.enabled == true){
				m_Scroll.enabled = false;
			}
            if (m_IsSingleFinger)
            {
                oldPosition1 = Input.GetTouch(0).position;
                oldPosition2 = Input.GetTouch(1).position;
                if (m_Camera != null)
				{
					nowscale =  m_Camera.orthographicSize;
				}
				if (m_ScaleRoot != null)
				{
					nowscale = m_ScaleRoot.transform.localScale.x;
				}
            }
            
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved){
                bornscale = Vector2.Distance (oldPosition1, oldPosition2);
				float nowdis = Vector2.Distance (Input.GetTouch(0).position, Input.GetTouch(1).position);
				float ratio = (nowdis / bornscale);
			 	float scale  = Mathf.Clamp(ratio * nowscale, minDistance, maxDistance);
				if (m_Camera != null)
				{
					m_Camera.orthographicSize = scale;
				}
				if (m_ScaleRoot != null)
				{
					m_ScaleRoot.transform.localScale = Vector3.one * scale;
				}
			}
            m_IsSingleFinger = false;
		}
#endif
    }
}
