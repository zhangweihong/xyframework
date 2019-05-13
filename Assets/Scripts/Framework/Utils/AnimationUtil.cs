using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// 基于dotween的动画工具
/// </summary>
public static class AnimationUtil
{
    public static void Fade(CanvasGroup cgroup, float startA, float endA, float time = 0.3f, Action callBack = null, float delaytime = 0)
    {
        cgroup.alpha = startA;
        DOTween.To(() => cgroup.alpha, x => cgroup.alpha = x, endA, time).SetEase(Ease.InQuint).OnComplete(() =>
        {
            if (callBack != null)
            {
                callBack();
            }
        }).SetDelay(delaytime);
    }

    public static void Fade(RawImage image, float startA, float endA, float time = 0.5f, Action callBack = null)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, startA);
        DOTween.To(() => image.color, x => image.color = x, new Color(image.color.r, image.color.g, image.color.b, endA), time).SetEase(Ease.InQuint).OnComplete(() =>
           {
               if (callBack != null)
               {
                   callBack();
               }
           });
    }

    public static void Fade(Image image, float startA, float endA, float time = 0.5f, Action callBack = null, Ease ease = Ease.InQuint)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, startA);
        DOTween.To(() => image.color, x => image.color = x, new Color(image.color.r, image.color.g, image.color.b, endA), time).SetEase(ease).OnComplete(() =>
           {
               if (callBack != null)
               {
                   callBack();
               }
           });
    }

    public static void Fade(AudioSource source, float startv, float endv, int id, float time = 0.3f, Action callBack = null)
    {
        source.volume = startv;
        DOTween.To(() => source.volume, x => source.volume = x, endv, time).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (callBack != null)
            {
                callBack();
            }
        }).SetId(id);
    }

    public static void StopForID(int id)
    {
        DOTween.Kill(id);
    }
    public static void PauseForID(int id)
    {
        DOTween.Pause(id);
    }

    public static void FadeLoop(Image image, float startA, float endA, int id, float time = 0.5f)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, startA);
        DOTween.To(() => image.color, x => image.color = x, new Color(image.color.r, image.color.g, image.color.b, endA), time).SetLoops(-1, LoopType.Yoyo).SetId(id);
    }

    public static void FadeYoyo(Image image, float startA, float endA, float time = 0.5f)
    {
        if (image.color.a - endA < 0.05f)
        {
            endA = startA;
        }
        DOTween.To(() => image.color, x => image.color = x, new Color(image.color.r, image.color.g, image.color.b, endA), time).SetEase(Ease.Linear);
    }

    public static void StopFadeForID(Image image, float endA, int id)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, endA);
        DOTween.Kill(id);
    }


    public static void FadeLoop(RawImage image, float startA, float endA, int id, float time = 0.5f)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, startA);
        DOTween.To(() => image.color, x => image.color = x, new Color(image.color.r, image.color.g, image.color.b, endA), time).SetLoops(-1, LoopType.Yoyo).SetId(id);
    }

    public static void StopFadeForID(RawImage image, float endA, int id)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, endA);
        DOTween.Kill(id);
    }

    public static void RouteLoop(Transform transform, int id, float time = 0.5f)
    {
        transform.localEulerAngles = Vector3.zero;
        transform.DORotate(new Vector3(0, 0, 361), time, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetId(id).SetEase(Ease.Linear);
    }

    public static Tweener ScaleLoop(Transform transform, int id, float scale, float time = 0.5f)
    {
        transform.localScale = Vector3.one;
        return transform.DOScale(Vector3.one * scale, time).SetLoops(-1, LoopType.Yoyo).SetId(id).SetEase(Ease.Linear);
    }

    public static Tweener Move(Transform tr, Vector3 startPos, Vector3 endsPos, float time = 0.3f, Action callBack = null, float delaytime = 0.0f)
    {
        tr.localPosition = startPos;
        Tweener tw = DOTween.To(() => tr.localPosition, x => tr.localPosition = x, endsPos, time).SetEase(Ease.InQuint).OnComplete(() =>
       {
           if (callBack != null)
           {
               callBack();
           }
       }).SetDelay(delaytime);
        tw.IsPlaying();
        return tw;
    }

    public static Tweener SimpleMove(Transform tr, Vector3 startPos, Vector3 endsPos, float time = 0.3f, int looptime = 1, bool islocal = true, Ease easetype = Ease.Linear)
    {
        if (islocal)
        {
            tr.localPosition = startPos;
            return DOTween.To(() => tr.localPosition, x => tr.localPosition = x, endsPos, time).SetEase(easetype).SetLoops(looptime, LoopType.Restart);
        }
        else
        {
            tr.position = startPos;
            return DOTween.To(() => tr.position, x => tr.position = x, endsPos, time).SetEase(easetype).SetLoops(looptime, LoopType.Restart);
        }

    }

    public static Tweener SimpleWorldMove(Transform tr, Vector3 startPos, Vector3 endsPos, float time = 0.3f, int looptime = 1, Ease easetype = Ease.Linear)
    {
        tr.position = startPos;
        return DOTween.To(() => tr.position, x => tr.position = x, endsPos, time).SetEase(easetype).SetLoops(looptime, LoopType.Restart);
    }

    public static bool IsPlay(Tweener tw)
    {
        return tw.IsPlaying();
    }
}
