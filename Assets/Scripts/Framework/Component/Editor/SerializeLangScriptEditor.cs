using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[CustomEditor(typeof(SerializeLangScript))]
public class SerializeLangScriptEditor : Editor
{
    private SerializeLangScript t;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        t = (SerializeLangScript)target;
        if (GUILayout.Button("刷新本地语言列表"))
        {
            t.RefreshLocalLangText();
        }
    }

   
}
