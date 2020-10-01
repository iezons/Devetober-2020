using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 缓存池中的抽屉
/// </summary>
public class PoolData
{
    //类别物体(抽屉)
    public GameObject fatherObj;
    //物体的容器
    public List<GameObject> PoolList;

    public PoolData(GameObject obj, GameObject PoolObj)//PoolObj就是一个衣柜，类别物体是衣柜的抽屉，实际物体就是衣服
    {
        //创建类别物体并将其挂到缓存池物体(PoolObj)上
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = PoolObj.transform;
        PoolList = new List<GameObject>() {};
        Put(obj);
    }

    /// <summary>
    /// 将物体放入缓存池
    /// </summary>
    /// <param name="obj"></param>
    public void Put(GameObject obj)
    {
        obj.SetActive(false);
        PoolList.Add(obj);
        obj.transform.parent = fatherObj.transform;
    }

    /// <summary>
    /// 从缓存池获取一个物体
    /// </summary>
    /// <returns></returns>
    public GameObject Get()
    {
        GameObject obj = null;
        obj = PoolList[0];
        PoolList.RemoveAt(0);
        obj.SetActive(true);
        obj.transform.parent = null;
        return obj;
    }
}


/// <summary>
/// 缓存池基础模块
/// </summary>
public class CachePool : SingleClassBase<CachePool>
{
    public Dictionary<string, PoolData> PoolDic = new Dictionary<string, PoolData>();

    GameObject Pool;

    /// <summary>
    /// 从缓存池取出物体
    /// </summary>
    /// <param name="name">物体路径</param>
    /// <param name="callback">传入物体实例化后需要调用的方法，此项会返回取出的物体</param>
    public void GetObject(string name, UnityAction<GameObject> callback)
    {
        if(PoolDic.ContainsKey(name) && PoolDic[name].PoolList.Count > 0)//判断存在该类缓存池 并且缓存池中有物体
        {
            callback?.Invoke(PoolDic[name].Get());
        }
        else//以上条件任意一点不满足都可以直接实例化新物体
        {
            //通过异步加载资源
            ResMgr.GetInstance().LoadAsync<GameObject>(name, o =>
            {
                o.name = name;
                callback?.Invoke(o);
            });
        }
    }

    /// <summary>
    /// 将物体放入缓存池
    /// </summary>
    /// <param name="name">将要放入的物体的类型名</param>
    /// <param name="obj">将要放入的物体本身</param>
    public void PutObject(string name, GameObject obj)
    {
        //检查是否存在缓存池物体，不存在则新建一个
        if(Pool == null)
        {
            Pool = new GameObject("Pool");
        }

        if(PoolDic.ContainsKey(name))//有对应类的缓存池
        {
            PoolDic[name].Put(obj);
        }
        else//无对应类的缓存池
        {
            PoolDic.Add(name, new PoolData(obj, Pool));
        }
    }

    /// <summary>
    /// 清空缓存池
    /// 主要用于场景切换
    /// </summary>
    public void Clear()
    {
        PoolDic.Clear();
        Pool = null;
    }
}
