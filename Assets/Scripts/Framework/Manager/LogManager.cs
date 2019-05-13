using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 为了方便输出log日志
/// </summary>
public class LogManager : MonoBehaviour
{
    private string log = "";
    private string logpath = "";
    void Awake()
    {
        Application.logMessageReceived += LogHandle;
        logpath = Path.Combine(Application.persistentDataPath, "log.txt").ToSingleForwardSlash().ToForwardSlash();
    }
    // Use this for initialization
    void Start()
    {
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "输出日志"))
        {
            if (File.Exists(logpath))
            {
                File.Delete(logpath);
            }
            GamePlayerPrefs.DeleteAll();
            StreamWriter sw = File.CreateText(logpath);
            sw.Write(log);
            sw.Close();
            sw.Dispose();
            Debug.Log(logpath);
        }
    }

    private void LogHandle(string condition, string stackTrace, LogType type)
    {
        log = log + (type + " " + condition + "  " + stackTrace + "  \n");
    }
}
