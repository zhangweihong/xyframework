using UnityEngine;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

/// <summary>
/// 保存本地的缓存数据工具
/// </summary>
public static class GamePlayerPrefs
{
    private static string sKEY = "ZTdkNTNmNDE2NTM3MWM0NDFhNTEzNzU1";
    private static string sIV = "4rZymEMfa/PpeJ89qY4gyA==";

    public static void SetInt(string key, int val)
    {
        UnityEngine.PlayerPrefs.SetString(GetHash(key), Encrypt(val.ToString()));
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        string valStr = GetString(key, defaultValue.ToString());
        int val = defaultValue;
        int.TryParse(valStr, out val);
        return val;
    }

    public static void SetFloat(string key, float val)
    {
        UnityEngine.PlayerPrefs.SetString(GetHash(key), Encrypt(val.ToString()));
    }

    public static float GetFloat(string key, float defaultValue = 0.0f)
    {
        string valStr = GetString(key, defaultValue.ToString());
        float val = defaultValue;
        float.TryParse(valStr, out val);
        return val;
    }

    public static void SetString(string key, string val)
    {
        UnityEngine.PlayerPrefs.SetString(GetHash(key), Encrypt(val));
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string dec = defaultValue;
        string enc = PlayerPrefs.GetString(GetHash(key), defaultValue.ToString());
        if (!dec.Equals(enc))
        {
            dec = Decrypt(enc);
        }
        return dec;
    }

    public static bool HasKey(string key)
    {
        string hashedKey = GetHash(key);
        return UnityEngine.PlayerPrefs.HasKey(hashedKey);
    }

    public static void DeleteKey(string key)
    {
        string hashedKey = GetHash(key);
        UnityEngine.PlayerPrefs.DeleteKey(hashedKey);
    }

    public static void DeleteAll()
    {
        UnityEngine.PlayerPrefs.DeleteAll();
    }

    public static void Save()
    {
        UnityEngine.PlayerPrefs.Save();
    }

    public static string Decrypt(string encString)
    {
        var sEncryptedString = encString;

        var myRijndael = new RijndaelManaged()
        {
            Padding = PaddingMode.Zeros,
            Mode = CipherMode.CBC,
            KeySize = 128,
            BlockSize = 128
        };

        var key = Encoding.UTF8.GetBytes(sKEY);
        var IV = Convert.FromBase64String(sIV);
        var decryptor = myRijndael.CreateDecryptor(key, IV);
        var sEncrypted = Convert.FromBase64String(sEncryptedString);
        var fromEncrypt = new byte[sEncrypted.Length];
        var msDecrypt = new MemoryStream(sEncrypted);
        var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

        csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

        return (Encoding.UTF8.GetString(fromEncrypt).TrimEnd('\0'));
    }

    public static string Encrypt(string rawString)
    {
        var sToEncrypt = rawString;

        var myRijndael = new RijndaelManaged()
        {
            Padding = PaddingMode.Zeros,
            Mode = CipherMode.CBC,
            KeySize = 128,
            BlockSize = 128
        };

        var key = Encoding.UTF8.GetBytes(sKEY);
        var IV = Convert.FromBase64String(sIV);
        var encryptor = myRijndael.CreateEncryptor(key, IV);
        var msEncrypt = new MemoryStream();
        var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        var toEncrypt = Encoding.UTF8.GetBytes(sToEncrypt);

        csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
        csEncrypt.FlushFinalBlock();

        var encrypted = msEncrypt.ToArray();

        return (Convert.ToBase64String(encrypted));
    }

    public static string GetHash(string key)
    {
        MD5 md5Hash = new MD5CryptoServiceProvider();
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(key));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }

}
