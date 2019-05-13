using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 简单重写scrollrect 
/// 带有 page 效果
/// </summary>
public class UGUIScrollRect : ScrollRect
{
    public bool IsUsePage = false;
    public System.Action<GameObject, GameObject, GameObject> FinishCallBack;


    [SerializeField]
    private GridLayoutGroup grid = null;
    private float finalPosX = 0;
    private int CurrentObjIndex = 0;
    private Tweener m_MoveTw = null;
    private Tweener m_ScaleTw = null;


    // Use this for initialization
    void Start()
    {
        grid = content.GetComponent<GridLayoutGroup>();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (!IsUsePage)
        {
            if (m_MoveTw != null)
            {
                m_MoveTw.Kill();
                m_MoveTw = null;
            }
            if (m_ScaleTw != null)
            {
                m_ScaleTw.Kill();
                m_ScaleTw = null;
            }
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (!IsUsePage)
        {
            return;
        }
        //計算當前於可視區域內的物件index
        int index = ProcessCurrentObjIndex();
        //若與目前的物件index不同則呼叫事件
        if (CurrentObjIndex != index)
        {
            CurrentObjIndex = index;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        RefreshPageview();
    }


    public void RefreshPageview()
    {
        if (!IsUsePage)
        {
            return;
        }
        float cellSizeX = grid.cellSize.x;
        CurrentObjIndex = ProcessCurrentObjIndex();
        finalPosX = cellSizeX / 2 + CurrentObjIndex * cellSizeX;
        m_MoveTw = content.DOLocalMoveX(-finalPosX, 0.3f);
        Transform tr = content.GetChild(CurrentObjIndex);
        m_ScaleTw = tr.DOScale(Vector3.one * 1.2f, 0.2f);
        ScaleToOneLJ(CurrentObjIndex, tr.gameObject);

    }

    private void ScaleToOneLJ(int currentindex, GameObject indexobj)
    {
        int preindex = currentindex - 1;
        int backindex = currentindex + 1;
        GameObject preobj = null;
        GameObject backobj = null;
        if (preindex > -1)
        {
            Transform tr = content.GetChild(preindex);
            tr.DOScale(Vector3.one, 0.1f);
            preobj = tr.gameObject;
        }

        if (backindex < content.childCount)
        {
            Transform tr = content.GetChild(backindex);
            tr.DOScale(Vector3.one, 0.1f);
            backobj = tr.gameObject;
        }

        if (FinishCallBack != null)
        {
            FinishCallBack(indexobj, preobj, backobj);
        }
    }


    /// <summary>
    /// 計算處於可視區域的物件index
    /// </summary>
    /// <returns></returns>
    private int ProcessCurrentObjIndex()
    {
        int contentChildCount = content.childCount;

        float contentEndPosX = content.localPosition.x;
        float cellSizeX = grid.cellSize.x;

        int resultIndex = Mathf.Abs((int)((contentEndPosX) / cellSizeX));

        //index超出範圍的保護
        if (resultIndex < 0) resultIndex = 0;
        if (resultIndex > contentChildCount - 1) resultIndex = contentChildCount - 1;

        return resultIndex;
    }

}
