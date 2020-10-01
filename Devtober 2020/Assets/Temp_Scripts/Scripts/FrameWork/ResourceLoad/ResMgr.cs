using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源加载模块
/// </summary>
public class ResMgr : SingleClassBase<ResMgr>
{
    
    /// <summary>
    /// 同步场景资源加载
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="name">资源路径</param>
    /// <returns></returns>
    public T Load<T>(string name) where T:Object
    {
        T res = Resources.Load<T>(name);
        //如果对象是GameObject 则实例化后再返回出去 
        if(res is GameObject)
        {
            return GameObject.Instantiate(res);
        }
        else//AudioClip TextAsset 等无需实例化的物体可以直接加载
        {
            return res;
        }
    }

    /// <summary>
    /// 异步场景资源加载
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="name">资源路径</param>
    /// <param name="callback">加载完成后执行的代码，没有请传null，代码请使用lambda表达式来简化</param>
    public void LoadAsync<T>(string name, UnityAction<T> callback) where T:Object
    {
        StartCoroutine(LoadAsynsAction<T>(name, callback));
    }


    private IEnumerator LoadAsynsAction<T>(string name, UnityAction<T> callback) where T : Object
    {
        ResourceRequest res =  Resources.LoadAsync<T>(name);
        yield return res;
        if(callback != null)
        {
            if (res.asset is GameObject)
            {
                callback(GameObject.Instantiate(res.asset) as T);
            }
            else
            {
                callback(res.asset as T);
            }
        }
    }
	
	public void LoadObjectAsyns<T>(Object obj, UnityAction<T> callback) where T : Object
	{
			bool flag = callback != null;
			if (flag)
			{
				bool flag2 = obj is GameObject;
				if (flag2)
				{
					callback.Invoke(Object.Instantiate(obj) as T);
				}
				else
				{
					callback.Invoke(obj as T);
				}
			}
	}
}
