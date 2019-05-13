using System;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// 字符串拼装函数 为了减少GC 使用stringbuilder
/// </summary>
public static class StringUtil
{
    public static StringBuilder sb = new StringBuilder();
    public static string AppendFormat(string str, object obj)
    {
        sb.Length = 0;
        sb.AppendFormat(str, obj);
        return sb.ToString();
    }

    public static string AppendFormat(string str, params object[] infos)
    {
        sb.Length = 0;
        sb.AppendFormat(str, infos);
        return sb.ToString();
    }
}
