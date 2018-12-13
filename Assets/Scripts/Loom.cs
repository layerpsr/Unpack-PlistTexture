using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// 子线程与主线程交互(委托实现)
/// </summary>
public class Loom : MonoBehaviour
{
    //单例模式(此方法只能在主线程调用)
    public static Loom _instance = null;
    public static void Instance()
    {
        if (_instance == null)
        {
            var obj = new GameObject("Loom");
            DontDestroyOnLoad(obj);
            obj.AddComponent<Loom>();
        }
    }

    //委托队列
    private Queue<Action> asyncQueue = new Queue<Action>();
    private Queue<Action> mainQueue = new Queue<Action>();
    //线程对象
    private Thread thread = null;
    
    //主线程每次Update执行Function数量
    private static int doUpdate = 5;

    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        _instance = this;
        ThreadStart();
    }
    void Update()
    {
        DoFunction();
    }

    //执行Action(根据线程判断对应的方法)
    private void DoFunction()
    {
        if (Thread.CurrentThread == thread)
        {
            if (asyncQueue.Count > 0)
            {
                var func = asyncQueue.Dequeue();
                func();
            }
        }
        else
        {
            if (mainQueue.Count > 0)
            {
                int number = doUpdate;
                do
                {
                    var func = mainQueue.Dequeue();
                    func();
                    number--;
                } while (number > 0 && mainQueue.Count > 0);
            }
        }
    }
    public void AsyncFunction(Action action)
    {
        asyncQueue.Enqueue(action);
    }
    public void MainFunction(Action action)
    {
        mainQueue.Enqueue(action);
    }
    //停止线程
    public void ThreadStop()
    {
        if (thread != null && thread.IsAlive)
        {
            thread.Abort();
            thread = null;
            DestroyImmediate(gameObject);
        }
    }
    void ThreadStart()
    {
        ThreadStop();
        thread = new Thread(new ThreadStart(ThreadDelegate));
        thread.IsBackground = true;
        thread.Start();
    }
    void ThreadDelegate()
    {
        while (this != null)
        {
            DoFunction();
        }
        Debug.Log("Thread DoOver");
    }

    public static void InvokeAsync(Action action)
    {
        if (_instance == null)
        {
            throw new Exception("未实例化Loom对象");
        }
        _instance.AsyncFunction(action);
    }
    public static void InvokeMain(Action action)
    {
        if (_instance == null)
        {
            throw new Exception("未实例化Loom对象");
        }
        _instance.MainFunction(action);
    }
}
