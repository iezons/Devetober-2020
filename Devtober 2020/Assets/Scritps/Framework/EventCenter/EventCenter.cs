using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEventInfo//空接口包裹了EventInfo是一个泛型
{

}

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

public class EventInfo : IEventInfo
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

/// <summary>
/// 事件处理中心模块
/// </summary>
public class EventCenter : SingletonBase<EventCenter>
{
    //声明一个Dictionary变量
    //——key 对应事件的名称
    //——value 对应希望响应事件的函数们
    public Dictionary<string, IEventInfo> EventDic = new Dictionary<string, IEventInfo>();

    /// <summary>
    /// 需接受参数的添加事件委托 请在前括号的前面用<>填上你想要给action处理的参数的类型
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="action">处理该事件的函数</param>
    public void AddEventListener<T>(string name, UnityAction<T> action)
    {
        //检测是否已经有对应事件的监听
        if(EventDic.ContainsKey(name))//如果有：
        {
            (EventDic[name] as EventInfo<T>).actions += action; //将准备进行的函数利用+=的方式就可以直接添加到UnityAction这个类别当中，执行的时候大家都会一起执行
            //Debug.Log("Add Action:   " + EventDic.Count);
        }
        else//如果没有：
        {
            EventDic.Add(name, new EventInfo<T>(action));
        }
    }

    /// <summary>
    /// 无需接受参数的添加事件委托
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="action">处理该事件的函数</param>
    public void AddEventListener(string name, UnityAction action)
    {
        //检测是否已经有对应事件的监听
        if (EventDic.ContainsKey(name))//如果有：
        {
            (EventDic[name] as EventInfo).actions += action; //将准备进行的函数利用+=的方式就可以直接添加到UnityAction这个类别当中，执行的时候大家都会一起执行
        }
        else//如果没有：
        {
            EventDic.Add(name, new EventInfo(action));
        }
    }

    /// <summary>
    /// 需接受参数的移除事件委托 请在前括号的前面用<>填上你想要给action处理的参数的类型
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="action">准备删除的函数</param>
    public void RemoveEventListener<T>(string name, UnityAction<T> action)
    {
        if (EventDic.ContainsKey(name))
            (EventDic[name] as EventInfo<T>).actions -= action;
    }

    /// <summary>
    /// 无需接受参数的移除事件委托
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="action">准备删除的函数</param>
    public void RemoveEventListener(string name, UnityAction action)
    {
        if (EventDic.ContainsKey(name))
            (EventDic[name] as EventInfo).actions -= action;
    }

    /// <summary>
    /// 直接移除指定Keys下的所有事件监听
    /// </summary>
    /// <param name="name">事件名称</param>
    public void RemoveEventListenerKeys(string name)
    {
        EventDic.Remove(name);
    }

    public void RemoveEventListenerValue(UnityAction acition)
    {
        foreach (var keys in EventDic.Keys)
        {
            (EventDic[keys] as EventInfo).actions -= acition;
        }
    }

    public void RemoveEventListenerValue<T>(UnityAction<T> acition)
    {
        foreach (var keys in EventDic.Keys)
        {
            (EventDic[keys] as EventInfo<T>).actions -= acition;
        }
    }

    /// <summary>
    /// 清空事件中心
    /// 主要用于场景切换
    /// </summary>
    public void Clear()
    {
        EventDic.Clear();
    }

    /// <summary>
    /// 需要传递参数的事件触发
    /// </summary>
    /// <param name="name">被触发的事件的名字</param>
    public void EventTriggered<T>(string name, T obj)
    {
        //检测是否已经有对应事件的监听
        if (EventDic.ContainsKey(name))//如果有：
        {
            if((EventDic[name] as EventInfo<T>).actions != null)
                (EventDic[name] as EventInfo<T>).actions.Invoke(obj);
        }
    }

    /// <summary>
    /// 无需传递参数的事件触发
    /// </summary>
    /// <param name="name">被触发的事件的名字</param>
    public void EventTriggered(string name)
    {
        //检测是否已经有对应事件的监听
        if (EventDic.ContainsKey(name))//如果有：
        {
            if ((EventDic[name] as EventInfo).actions != null)
                (EventDic[name] as EventInfo).actions.Invoke();
        }
    }

    public void DiaEventTrigger(string name)
    {
        //检测是否已经有对应事件的监听
        if (EventDic.ContainsKey(name))//如果有：
        {
            if ((EventDic[name] as EventInfo<string>).actions != null)
                (EventDic[name] as EventInfo<string>).actions.Invoke(name);
        }
    }

    public Coroutine AddTimeListener(float seconds, UnityAction callback)
    {
        return StartCoroutine(WaitingTimes(seconds, callback));
    }

    IEnumerator WaitingTimes(float seconds, UnityAction callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    public void AddTimeListener<T>(float seconds, UnityAction<T> callback, T obj)
    {
        StartCoroutine(WaitingTimes(seconds, callback, obj));
    }

    IEnumerator WaitingTimes<T>(float seconds, UnityAction<T> callback, T obj)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke(obj);
    }
}
