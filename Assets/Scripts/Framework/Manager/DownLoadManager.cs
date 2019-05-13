using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BestHTTP;

public class DownLoadManager : SingletonMono<DownLoadManager>
{
    /// <summary>
    /// 最大下载线程
    /// 依赖besthttp插件下载
    /// </summary>
    private byte maxDownThread = 5;

    void Start()
    {

    }

    public void Init()
    {
        HTTPManager.MaxConnectionPerServer = maxDownThread;
    }

    public void SendNetRequstBackJson_Lua(string host, string url, XLua.LuaTable fieldtb, Action<string> finishAction, XLua.LuaTable files = null)
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        fieldtb.ForEach<string, string>((key, value) =>
        {
            dic[key] = value;
        });
        Dictionary<string, string> binarydic = null;
        if (files != null)
        {
            binarydic = new Dictionary<string, string>();
            files.ForEach<string, string>((key, value) =>
            {
                binarydic[key] = value;
            });
        }
        this.SendNetRequstBackJson(host, url, dic, finishAction, binarydic);
    }

    public void SendNetRequstBackJson(string host, string url, Dictionary<string, string> fielddic, Action<string> finishAction, Dictionary<string, string> Binarydatas = null)
    {
        HTTPRequest request = new HTTPRequest(new Uri(StringUtil.AppendFormat("{0}{1}", host, url)));
        foreach (var item in fielddic)
        {
            request.AddField(item.Key, item.Value);
        }
        if (Binarydatas != null)
        {
            foreach (var item in Binarydatas)
            {
                request.AddBinaryData(item.Key, Util.GetBytes(item.Value));
            }
        }
        request.MethodType = HTTPMethods.Post;
        request.Callback = (req, resp) =>
        {
            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    string data = System.Text.Encoding.UTF8.GetString(resp.Data);
                    if (finishAction != null)
                    {
                        finishAction(data);
                    }
                    break;
                case HTTPRequestStates.Error:
                case HTTPRequestStates.Aborted:
                case HTTPRequestStates.TimedOut:
                case HTTPRequestStates.ConnectionTimedOut:
                    if (finishAction != null)
                    {
                        finishAction(null);
                    }
                    break;
                default:
                    break;
            }
        };
        HTTPManager.SendRequest(request);
    }

    public void StartDownloads(string host, List<string> filepaths, Action<float, string, byte[]> progressAction, Action finishAction, Action<string> erroAction = null)
    {
        _StartDownload(host, filepaths, 0, progressAction, finishAction, erroAction);
    }

    private void _StartDownload(string host, List<string> filepaths, int index, Action<float, string, byte[]> progressAction, Action finishAction, Action<string> erroAction = null)
    {
        if (index >= filepaths.Count)
        {
            if (finishAction != null)
            {
                finishAction();
            }
            return;
        }
        string relapath = filepaths[index];
        string url = StringUtil.AppendFormat("{0}/{1}{2}", host, PathUtil.GetPlatformName(), relapath);
        StartDownLoad(url, (p) =>
        {
        }, (bytes) =>
        {
            if (progressAction != null)
            {
                progressAction(index / (float)filepaths.Count, relapath, bytes);
            }
            index++;
            _StartDownload(host, filepaths, index, progressAction, finishAction, erroAction);
        }, (error) =>
        {
            Debug.LogError("下载错误 ！ " + error);
            index++;
            if (erroAction != null)
            {
                erroAction(error);
            }
            _StartDownload(host, filepaths, index, progressAction, finishAction, erroAction);
        });
    }

    /// <summary>
    /// 开始下载
    /// 可以下载任何数据
    /// 包括 大文件，图片，以及加密数据等等
    /// </summary>
    /// <param name="url"></param>
    /// <param name="progressAction"></param>
    /// <param name="finishAction"></param>
    public void StartDownLoad(string url, Action<float> progressAction, Action<byte[]> finishAction, Action<string> erroAction = null)
    {
        float filelength = 0f;
        HTTPRequest request = HTTPManager.SendRequest(url, (req, resp) =>
        {
            switch (req.State)
            {
                case HTTPRequestStates.Initial:
                    filelength = float.Parse(resp.GetFirstHeaderValue("content-length"));
                    Debug.LogWarning("初始化完成 " + url);
                    break;
                case HTTPRequestStates.Queued:
                    Debug.LogWarning("已经进入下载队列 " + url);
                    break;
                case HTTPRequestStates.Processing:
                    caclProgress(resp.GetStreamedFragments(), progressAction, filelength);
                    break;
                case HTTPRequestStates.Finished:
                    if (finishAction != null)
                    {
                        finishAction(resp.Data);
                    }
                    break;
                case HTTPRequestStates.Error:
                    string error = "请求完结出错! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    Debug.LogError(error);
                    if (erroAction != null)
                    {
                        erroAction("404");
                    }
                    break;
                case HTTPRequestStates.Aborted:
                    break;
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("链接超时！！！" + url);
                    if (erroAction != null)
                    {
                        erroAction("502");
                    }
                    break;
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("请求超时！！！ " + url);
                    if (erroAction != null)
                    {
                        erroAction("503");
                    }
                    break;
                default:
                    break;
            }
        });
        if (request == null)
        {
            Debug.Log("请求创建失败，确实是否达到了最大下载线程！！！");
            return;
        }
    }
    /// <summary>
    /// 计算当前的进度
    /// </summary>
    /// <param name="bLs"></param>
    /// <param name="progressAction"></param>
    /// <param name="fileSize"></param>
    private void caclProgress(List<byte[]> bLs, Action<float> progressAction, float fileSize)
    {
        if (fileSize < 0.0001f)
        {
            Debug.LogError("获取的文件过小,请仔细检查！！！");
            return;
        }
        int sumByteSize = 0;
        for (int i = 0; i < bLs.Count; i++)
        {
            sumByteSize += bLs[i].Length;
        }
        if (progressAction != null)
        {
            progressAction(sumByteSize / fileSize);
        }
    }
}
