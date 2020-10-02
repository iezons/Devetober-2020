using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例模式基础模块
/// </summary>
/// <typeparam name="T"></typeparam>

public abstract class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
{
    private static T single = null;

    public static T GetInstance()
    {
        if (single == null)
            single = FindObjectOfType(typeof(T)) as T;
        if (single == null)
        {
            single = new GameObject("Single " + typeof(T).ToString(), typeof(T)).GetComponent<T>();
        }
        GameObject.DontDestroyOnLoad(single);
        return single;
    }
}