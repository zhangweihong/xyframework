using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 实时显示fps的多少
/// </summary>
public class FPSInfo : MonoBehaviour
{

    public float showTime = 1f;
    private string strFpsInfo = "";
    private int count = 0;
    private float deltaTime = 0f;
    // Update is called once per frame

    void Awake()
    {

    }
    void Update()
    {
        count++;
        deltaTime += Time.deltaTime;
        if (deltaTime >= showTime)
        {
            float fps = count / deltaTime;
            float milliSecond = deltaTime * 1000 / count;
            strFpsInfo = string.Format(" 当前每帧渲染间隔：{0:0.0} ms ({1:0.} 帧每秒)", milliSecond, fps);
            count = 0;
            deltaTime = 0f;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 300, 0, 300, 60), strFpsInfo);
    }
}