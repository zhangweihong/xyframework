using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InputDescWinEditor : EditorWindow
{
    public static System.Action<string, string, string, string> m_CallBack;
    private string m_zhdesc = "更新资源和修复bug";
    private string m_endesc = "Update resources and Fix bugs";
    private string m_fzhdesc = "更新資源和修復bug";
    private string m_code = "";
    private static InputDescWinEditor mywindow;
    public static void Show(System.Action<string, string, string, string> callback)
    {
        m_CallBack = callback;
        mywindow = (InputDescWinEditor)EditorWindow.GetWindow(typeof(InputDescWinEditor));
        mywindow.Show(true);
        EditorUtility.SetDirty(mywindow);
    }

    void OnGUI()
    {
        m_code = EditorGUILayout.TextField("资源版本显示(不参与版本比对)", m_code);
        m_zhdesc = EditorGUILayout.TextField("中文描述", m_zhdesc);
        m_fzhdesc = EditorGUILayout.TextField("繁体中文描述", m_fzhdesc);
        m_endesc = EditorGUILayout.TextField("英文描述", m_endesc);
        if (GUILayout.Button("确定"))
        {
            if (m_CallBack != null)
            {
                m_CallBack(m_code, m_zhdesc, m_fzhdesc, m_endesc);
            }

            if (mywindow != null)
            {
                mywindow.Close();
            }

        }
    }

}
