using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 重写的debug 方便 日志自己控制
/// </summary>
public class Debug
{
    public static void Log(object message)
    {
#if Debug
        UnityEngine.Debug.Log(message);
#endif  
    }
    public static void Log(object message, Object context)
    {
#if Debug
        UnityEngine.Debug.Log(message);
#endif
    }
    public static void LogError(object message)
    {
#if Debug
        UnityEngine.Debug.LogError(message);
        lstErrors.Add(message.ToString());
#endif
    }
    public static void LogError(object message, Object context)
    {
#if Debug
        UnityEngine.Debug.LogError(message);
        lstErrors.Add(message.ToString());
#endif
    }
    public static void LogWarning(object message)
    {
#if Debug
        UnityEngine.Debug.LogWarning(message);
#endif
    }
    public static void LogWarning(object message, Object context)
    {
#if Debug
        UnityEngine.Debug.LogWarning(message);
#endif
    }
    public static void Assert(bool condition)
    {
#if Debug
        UnityEngine.Debug.Assert(condition);
#endif
    }
    public static void Assert(bool condition, object message)
    {
#if Debug
        UnityEngine.Debug.Assert(condition,message);
#endif
    }
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
#if Debug
        UnityEngine.Debug.DrawLine(start,end, color);
#endif
    }
    public static void LogException(System.Exception exception, Object context)
    {
#if Debug
        UnityEngine.Debug.LogException(exception, context);
#endif
    }
    public static void LogException(System.Exception exception)
    {
#if Debug
        UnityEngine.Debug.LogException(exception);
#endif
    }

    static List<string> lstErrors = new List<string>();
    public static void ClearErrors()
    {
        lstErrors.Clear();
    }
    public static List<string> GetErrors()
    {
        return lstErrors;
    }
}
