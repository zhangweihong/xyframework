using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 屏幕的水纹特效的后期处理
/// 这个是参考的网上的一个帖子的
/// 不错可以做过度动画
/// </summary>
public class CameraWaterWaveEffect : MonoBehaviour
{

    //距离系数
    private float distanceFactor = 60.0f;
    //时间系数
    private float timeFactor = -30.0f;
    //sin函数结果系数
    private float totalFactor = 1.0f;

    //波纹宽度
    private float waveWidth = 0.3f;
    //波纹扩散的速度
    private float waveSpeed = 0.8f;

    private float waveStartTime;
    private Vector4 startPos = new Vector4(0.5f, 0.5f, 0, 0);
    public Material _Material;
    // Use this for initialization
    void Start()
    {
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //计算波纹移动的距离，根据enable到目前的时间*速度求解
        float curWaveDistance = (Time.time - waveStartTime) * waveSpeed;
        //设置一系列参数
        _Material.SetFloat("_distanceFactor", distanceFactor);
        _Material.SetFloat("_timeFactor", timeFactor);
        _Material.SetFloat("_totalFactor", totalFactor);
        _Material.SetFloat("_waveWidth", waveWidth);
        _Material.SetFloat("_curWaveDis", curWaveDistance);
        _Material.SetVector("_startPos", startPos);
        Graphics.Blit(source, destination, _Material);
    }

    public void StartWaveAnimation()
    {
        waveStartTime = Time.time;
    }
}
