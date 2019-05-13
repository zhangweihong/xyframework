using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class UIComponentEditor : Editor
{
    [MenuItem("GameObject/UI/XY/新建SButton(Image)")]
    static void NewSButtonComponent()
    {
        GameObject obj = Selection.activeGameObject;
        GameObject sbutobj = new GameObject();
        if (obj != null)
        {
            sbutobj.transform.SetParent(obj.transform);
        }
        sbutobj.AddMissComponent<Image>();
        sbutobj.AddMissComponent<RectTransform>();
        sbutobj.AddMissComponent<SButtonUGUI>();
        sbutobj.transform.localScale = Vector3.one;
        sbutobj.transform.localPosition = Vector3.zero;
        sbutobj.transform.localEulerAngles = Vector3.zero;
        sbutobj.name = "NewSButton";
        GameObject textobj = new GameObject();
        textobj.transform.SetParent(sbutobj.transform);
        AutoFont(textobj, "Text");
    }

    [MenuItem("GameObject/UI/XY/新建Text(自动更改GameFont字体)")]
    static void NewTextComponet()
    {
        GameObject obj = Selection.activeGameObject;
        GameObject textobj = new GameObject();
        if (obj != null)
        {
            textobj.transform.SetParent(obj.transform);
        }
        AutoFont(textobj, "NewText");
    }

    [MenuItem("GameObject/UI/XY/添加SButton脚本")]
    static void AddSButtonComponent()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj != null)
        {
            var sbut = obj.AddMissComponent<SButtonUGUI>();
            if (sbut != null)
            {
                return;
            }
            obj.AddMissComponent<SButtonUGUI>();
        }
    }

    static Text AutoFont(GameObject textobj, string name)
    {
        textobj.AddMissComponent<RectTransform>();
        Text text = textobj.AddMissComponent<Text>();
        text.color = Color.black;
        Font font = AssetDatabase.LoadAssetAtPath("Assets/Res/Font/GameFont.ttf", typeof(Font)) as Font;
        if (font == null)
        {
            Debug.LogError("Res/Font/GameFont.ttf Not Find");
        }
        text.font = font;
        textobj.transform.localScale = Vector3.one;
        textobj.transform.localPosition = Vector3.zero;
        textobj.transform.localEulerAngles = Vector3.zero;
        textobj.name = name;
        text.text = name;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    public static void CreateViewRoot(string path, string name)
    {
        GameObject obj = Selection.activeGameObject;
        GameObject viewobj = new GameObject();
        if (obj != null)
        {
            viewobj.transform.SetParent(obj.transform);
        }
        viewobj.AddMissComponent<Image>();
        viewobj.AddMissComponent<RectTransform>();
        viewobj.AddMissComponent<XLuaMonoBehaviour>().LuaRelativePath = path;
        viewobj.transform.localScale = Vector3.one;
        viewobj.transform.localPosition = Vector3.zero;
        viewobj.transform.localEulerAngles = Vector3.zero;
        viewobj.name = name + "View";
    }

}
