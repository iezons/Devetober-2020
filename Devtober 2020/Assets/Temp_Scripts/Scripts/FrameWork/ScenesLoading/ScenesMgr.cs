using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换模块
/// </summary>
public class ScenesMgr : SingletonBase<ScenesMgr>
{

    /// <summary>
    /// 同步加载场景 容易卡顿 建议使用LoadSceneAsyn
    /// </summary>
    /// <param name="name">场景名称</param>
    /// <param name="function">切换场景后调用的函数，没有请传null</param>
    public void LoadScene(string name, UnityAction function)
    {
        //场景同步加载
        SceneManager.LoadScene(name);

        function?.Invoke(); //function不为空则执行
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="name">场景名称</param>
    /// <param name="function">切换场景后调用的函数，没有请传null</param>
    public void LoadSceneAsyn(string name, UnityAction function)
    {
        StartCoroutine(LoadSceneAsynAction(name, function));
    }

    private IEnumerator LoadSceneAsynAction(string name, UnityAction funtion)
    {
        AsyncOperation AO = SceneManager.LoadSceneAsync(name);
        yield return AO;
        
        while(!AO.isDone)
        {
            //直接给事件中心发送进度条更新事件
            EventCenter.GetInstance().EventTriggered("ProgressUpdate", AO.progress);
            //做一个返回数值，主要用于将下一次循环放到下一帧再执行
            yield return AO.progress;
        }
        funtion?.Invoke(); //function不为空则执行
    }
}
