using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(SButtonUGUI))]
public class SButtonUGUIEditor : XLuaMonoBehaviourEditor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SButtonUGUI sbut = target as SButtonUGUI;

        if (sbut != null && sbut.gameObject != null)
        {
            if (sbut.Listener == null)
            {
                sbut.Listener = sbut.gameObject.AddComponent<EventTriggerListener>();
            }
        }
    }
}
