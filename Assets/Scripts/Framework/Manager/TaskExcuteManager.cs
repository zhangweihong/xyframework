using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 队列任务管理基类
/// 抽象队列任务管理，一个任务执行完成以后释放队列执行资源，可进行下一步操作
/// 抽象数据为范型类T，可重载范型T的任务执行方法
/// </summary>
/// <typeparam name="T"></typeparam>
public class TaskExcuteManager<T>
{
    /// <summary>
    /// 最多同时请求数量
    /// </summary>
    protected int MaxRequest = 10;
    /// <summary>
    /// 当前执行中的任务数量
    /// </summary>
    protected int currRunTasks = 0;
    /// <summary>
    /// 返回一个线程安全的队列，或者依据优先级获取的
    /// </summary>
    protected Queue requests = Queue.Synchronized(new Queue());

    /// <summary>
    /// 添加新任务
    /// </summary>
    public virtual void AddTask(T t)
    {
        requests.Enqueue(t);
        RunNext();
    }

    /// <summary>
    /// 获取当前可执行的任务
    /// </summary>
    protected virtual T GetCurrTask()
    {
        if (requests == null || requests.Count == 0)
            return default(T);
        return (T)requests.Dequeue();
    }

    protected virtual void ExcuteTask(T t, Action finish)
    {
        if (finish != null)
        {
            finish();
        }
    }

    private bool excuteLock = false;
    /// <summary>
    /// 停止当前处理器，正在进行的不会停止，将要进行的会被暂停
    /// </summary>
    public void Stop()
    {
        excuteLock = true;
    }

    /// <summary>
    /// 重新开启处理器
    /// </summary>
    public void ReStart()
    {
        excuteLock = false;
        //启动的时候开启多线程一起
        for (int i = 0; i < MaxRequest; i++)
        {
            RunNext();
        }
    }
    protected void RunNext()
    {
        //只有执行锁未被开启的时候执行
        if (excuteLock)
            return;
        if (currRunTasks >= MaxRequest)
        {
            //当前任务超多了，不能再执行了
            return;
        }
        T currT = GetCurrTask();
        if (currT == null)
        {
            return;
        }
        currRunTasks++;
        ExcuteTask(currT, () =>
        {
            //完成以后需要将Task减减 
            currRunTasks--;
            RunNext();
        });
    }
}

/// <summary>
/// 协程队列管理
/// 通过调用协程Coroutine.AddTask(IEnumerator) 执行协程任务
/// </summary>
public class CoroutineManager : TaskExcuteManager<IEnumerator>
{
    #region CoroutineMgr Init Static
    private static CoroutineManager singleton = null;
    private static readonly object singletonLock = new object();
    public static CoroutineManager I
    {
        get
        {
            if (singleton == null)
            {
                Init();
            }
            return singleton;
        }
    }
    private static void Init()
    {
        if (singleton != null)
        {
            return;
        }

        lock (singletonLock)
        {
            if (singleton != null)
            {
                return;
            }
            singleton = new CoroutineManager();
        }
    }
    #endregion

    public CoroutineManager()
    {
        //设置线程数    
        MaxRequest = 5;
    }
    /// <summary>
    /// 如何执行任务
    /// 暂时用挂在在GameRoot物体上
    /// </summary>
    protected override void ExcuteTask(IEnumerator t, Action callback)
    {
        App.I.StartCoroutine(CallWarpper(t, callback));
    }

    IEnumerator CallWarpper(IEnumerator t, Action callback)
    {
        yield return null;
        IEnumerator e = t;
        while (true)
        {
            if (e != null && e.MoveNext())
            {
                yield return e.Current;
            }
            else
            {
                break;
            }
        }
        if (callback != null)
        {
            callback();
        }
    }
}