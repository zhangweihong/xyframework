using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;

/// <summary>
/// 继承工具集
/// </summary>
public static class Util
{

    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string GetMd5ForFile(string file)
    {
        StringBuilder sb = new StringBuilder();
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(file + "获取MD5 错误 Message" + ex.Message + ex.StackTrace);
        }
        return sb.ToString();
    }
    /// <summary>
    /// 获取字符串的MD5值
    /// </summary>
    public static string GenerateMD5(string str)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] byteArray = Encoding.ASCII.GetBytes(str);
        byteArray = md5.ComputeHash(byteArray);
        string hashedValue = "";
        foreach (byte b in byteArray)
        {
            hashedValue += b.ToString("x2");
        }
        return hashedValue;
    }

    /// <summary>
    /// 获取path下的文件大小
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static long GetFileLength(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError(path + " 不存在");
            return 0;
        }

        long len = 0;
        FileInfo fileInfo = new FileInfo(path);
        len = fileInfo.Length;
        return len;
    }
    /// <summary>
    /// 数组转list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tArry"></param>
    /// <returns></returns>
    public static List<T> ArryConvertList<T>(T[] tArry)
    {
        if (tArry == null)
        {
            Debug.LogError("ArryConvertList 失败！！！");
            return null;
        }
        List<T> ts = new List<T>();
        for (int i = 0; i < tArry.Length; i++)
        {
            ts.Add(tArry[i]);
        }
        return ts;
    }

    private static int sortmin;
    public static void Sort(string[] list)
    {
        for (int i = 0; i < list.Length - 1; i++)
        {
            sortmin = i;
            for (int j = i + 1; j < list.Length; j++)
            {
                if (string.Compare(list[j], list[sortmin]) < 0)
                    sortmin = j;
            }
            string t = list[sortmin];
            list[sortmin] = list[i];
            list[i] = t;
        }
    }


    /// <summary>
    /// 获取type
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static AssetType GetAssetType(string ex)
    {
        AssetType t = AssetType.None;
#if UNITY_EDITOR
        switch (ex)
        {
            case ".png":
            case ".tga":
                t = AssetType.Texture;
                break;
            case ".prefab":
                t = AssetType.GameObjct;
                break;
        }
#endif
        return t;
    }

    /// <summary>
    /// 根据RectTransform 获取当前相机下的真实pos
    /// </summary>
    /// <param name="rtr"></param>
    /// <param name="inputpos"></param>
    /// <param name="cam"></param>
    /// <returns></returns>
    public static Vector3 GetPosition(RectTransform rtr, Vector3 inputpos, Camera cam)
    {
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rtr, inputpos, cam, out pos);
        return new Vector3(pos.x, pos.y, 0);
    }

    /// <summary>
    /// 根据RectTransform 获取当前相机下的真实local pos
    /// </summary>
    /// <param name="rtr"></param>
    /// <param name="inputpos"></param>
    /// <param name="cam"></param>
    /// <returns></returns>
    public static Vector3 GetLocalPosition(RectTransform rtr, Vector3 inputpos, Camera cam)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rtr, inputpos, cam, out pos);
        return new Vector3(pos.x, pos.y, 0);
    }

    /// <summary>
    /// 当前环境
    /// </summary>
    /// <returns></returns>
    public static string GetCurAppSetting()
    {
        if (Application.isEditor)
        {
            return "Editor";
        }
        else
        {
            return "Player";
        }
    }

    /// <summary>
    /// 获取真实年龄
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    public static int GetAge(int year, int month, int day)
    {
        int age = 0;
        age = DateTime.Now.Year - year;
        if (month > DateTime.Now.Month || (month == DateTime.Now.Month && day > DateTime.Now.Day))
        {
            age--;
        }
        return age < 0 ? 0 : age;

    }

    /// <summary>
    /// 自动检测添加组件
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T AddMissComponent<T>(this GameObject obj) where T : UnityEngine.Component
    {
        T t = default(T);
        if (obj.GetComponent<T>() == null)
        {
            t = obj.AddComponent<T>();
        }
        return t;
    }

    /// <summary>
    /// 保存字符串到本地缓存
    /// </summary>
    /// <param name="relapath"></param>
    /// <param name="savestr"></param>
    public static void SaveFileToPersistent(string relapath, string savestr)
    {
        string filepath = "";
#if UNITY_EDITOR
        filepath = PathUtil.ConvertRelativePathToEditorOutputPath(relapath);
#else
        filepath = PathUtil.ConvertRelativePathToPersistentPath(relapath);
#endif
        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }
        string dic = Path.GetDirectoryName(filepath);
        if (!Directory.Exists(dic))
        {
            Directory.CreateDirectory(dic);
        }
        StreamWriter sw = File.CreateText(filepath);
        sw.Write(savestr);
        sw.Close();
        sw.Dispose();
    }

    /// <summary>
    /// 读取本地缓存文件
    /// </summary>
    /// <param name="relapath"></param>
    /// <returns></returns>
    public static string ReadFileToPersistent(string relapath)
    {
        string filepath = "";
#if UNITY_EDITOR
        filepath = PathUtil.ConvertRelativePathToEditorOutputPath(relapath);
#else
        filepath = PathUtil.ConvertRelativePathToPersistentPath(relapath);
#endif
        string content = "";
        if (File.Exists(filepath))
        {
            content = File.ReadAllText(filepath);
        }
        else
        {
            Debug.LogError("不存在 文件 " + relapath);
        }
        return content;
    }

    /// <summary>
    /// 保存字符串到指定目录
    /// </summary>
    /// <param name="str"></param>
    /// <param name="filepath"></param>
    public static void SaveStrToFile(string str, string filepath)
    {
        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }
        string dir = Path.GetDirectoryName(filepath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        StreamWriter sw = File.CreateText(filepath);
        sw.Write(str);
        sw.Close();
        sw.Dispose();
    }

    /// <summary>
    /// 加载指定目录文件
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    public static string LoadFile(string filepath)
    {
        if (!File.Exists(filepath))
        {
            Debug.Log("文件不存在");
            return null;
        }
        string str = File.ReadAllText(filepath);
        return str;
    }

    /// <summary>
    /// 保存byte为file isasync是否异步
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="filepath"></param>
    /// <param name="isasync"></param>
    public static void SaveBytesToFile(byte[] bytes, string filepath, bool isasync = false)
    {
        if (isasync)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate (object state)
            {
                WriteFile(bytes, filepath);
            }));
        }
        else
        {
            WriteFile(bytes, filepath);
        }
    }

    /// <summary>
    /// 保存byte为file
    /// </summary>
    public static void WriteFile(byte[] bytes, string filepath)
    {
        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }
        string dir = Path.GetDirectoryName(filepath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        FileStream fs = File.Create(filepath);
        fs.Write(bytes, 0, bytes.Length);
        fs.Close();
        fs.Dispose();
    }

    //压缩字符串
    public static string CompressGZip(string str)
    {
        byte[] rawData = System.Text.Encoding.UTF8.GetBytes(str);
        MemoryStream ms = new MemoryStream();
        GZipOutputStream compressedzipStream = new GZipOutputStream(ms);
        compressedzipStream.Write(rawData, 0, rawData.Length);
        compressedzipStream.Close();
        string base64str = System.Convert.ToBase64String(ms.ToArray());
        compressedzipStream.Close();
        compressedzipStream.Dispose();
        return base64str;
    }

    public static string GetPlatform()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "IOS";
#else
        return "Other";
#endif
    }

    //解压字符串
    public static string DeCompressGZip(string base64str)
    {
        byte[] byteArray = System.Convert.FromBase64String(base64str);
        GZipInputStream gzi = new GZipInputStream(new MemoryStream(byteArray));
        if (base64str.Length > 500000)
        {
            Debug.LogError("字符串太长了~！");
            return string.Empty;
        }
        MemoryStream re = new MemoryStream(500000);
        int count;
        byte[] data = new byte[500000];
        while ((count = gzi.Read(data, 0, data.Length)) != 0)
        {
            re.Write(data, 0, count);
        }
        byte[] overarr = re.ToArray();
        string depressstr = System.Text.Encoding.UTF8.GetString(overarr);
        re.Close();
        re.Dispose();
        return depressstr;
    }

    /// <summary>
    /// 打开网址
    /// </summary>
    /// <param name="url"></param>
    public static void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }
    public static bool RegexpPhone(string phone)
    {
        string reg = "^((13[0-9])|(14[5,7])|(15[0-3,5-9])|(17[0,3,5-8])|(18[0-9])|166|198|199|(147))\\d{8}$";
        return System.Text.RegularExpressions.Regex.IsMatch(phone, reg);
    }

    /// <summary>
    /// 陀螺仪开启关闭
    /// </summary>
    /// <value></value>
    public static bool InputGyroEnabled
    {
        get
        {
            return Input.gyro.enabled;
        }
        set
        {
            Input.gyro.enabled = value;
        }
    }

    /// <summary>
    /// 返回陀螺仪的重力
    /// </summary>
    /// <value></value>
    public static Vector3 InputGyroGravity
    {
        get
        {
            return Input.gyro.gravity;
        }
    }

    /// <summary>
    /// 转换大写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToUpper(string str)
    {
        return str.ToUpper();
    }

    /// <summary>
    /// 转换小写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToLower(string str)
    {
        return str.ToLower();
    }


    public static float MathPow(float x, float y)
    {
        return Mathf.Pow(x, y);
    }

    public static byte[] GetBytes(string msg)
    {
        return System.Text.Encoding.UTF8.GetBytes(msg);
    }


    /// <summary>
    /// 停止dotweener动画
    /// </summary>
    /// <param name="twer"></param>
    public static void KillTweener(DG.Tweening.Tweener twer)
    {
        DG.Tweening.TweenExtensions.Kill(twer);
    }

    /// <summary>
    /// 手机震动
    /// </summary>
    public static void HandheldVibrate()
    {
#if !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// 异或
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static int Tobacknumber(int num)
    {
        return ~num;
    }

    public static string To16Number(int num)
    {
        return System.Convert.ToString(num, 16);
    }

    public static int To10Number(string num)
    {
        return System.Convert.ToInt32(num, 16);
    }
}
