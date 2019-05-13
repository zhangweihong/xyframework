using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

/// <summary>
/// 语言的结构体
/// </summary>
[System.Serializable]
public struct LangClass
{
    public Text Text;
    public string Key;
}

/// <summary>
/// 主要是为了 方便本地化语言的脚本
/// </summary>
public class SerializeLangScript : MonoBehaviour
{
    public string LastLangSetting = "zh";//当前的语言环境
    public List<LangClass> Langls = new List<LangClass>();

    // Use this for initialization
    void Start()
    {

    }
    public void RefreshLocalLangText()
    {
        Text[] ts = gameObject.GetComponentsInChildren<Text>(true);
        Langls.Clear();
        string pattern = "[\u4e00-\u9fbb]";
        for (int i = 0; i < ts.Length; i++)
        {
            if (Regex.IsMatch(ts[i].text, pattern))
            {
                LangClass lc = new LangClass();
                lc.Text = ts[i];
                lc.Key = ts[i].text;
                Langls.Add(lc);
            }
        }
    }

}
