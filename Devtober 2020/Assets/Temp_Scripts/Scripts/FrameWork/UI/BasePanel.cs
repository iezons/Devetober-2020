using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 面板基类 找到所有子对象
/// 提供显示和隐藏的接口
/// </summary>
public class BasePanel : MonoBehaviour
{
    //通过里式转换原则 来存储所有控件
    Dictionary<string, List<UIBehaviour>> controlDic = new Dictionary<string, List<UIBehaviour>>();

    void Awake()
    {
        FindComponentsInChildren<Button>();
        FindComponentsInChildren<Image>();
        FindComponentsInChildren<Text>();
        FindComponentsInChildren<Slider>();
        FindComponentsInChildren<Toggle>();
        FindComponentsInChildren<ScrollRect>();
    }

    /// <summary>
    /// 找到子对象的对应控件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void FindComponentsInChildren<T>() where T:UIBehaviour
    {
        T[] controls = GetComponentsInChildren<T>();
        string objName;
        for (int i = 0; i < controls.Length; i++)
        {
            objName = controls[i].gameObject.name;
            if (controlDic.ContainsKey(objName))
            {
                controlDic[objName].Add(controls[i]);
            }
            else
            {
                controlDic.Add(objName, new List<UIBehaviour>() { controls[i] });
            }
        }
    }

    /// <summary>
    /// 获得子物体的控件
    /// </summary>
    /// <typeparam name="T">要获取的控件类型(Button Image Text Slider Toggle ScrollRect)</typeparam>
    /// <param name="controlName">要获得名称</param>
    /// <returns></returns>
    protected T GetControl<T>(string controlName) where T:UIBehaviour
    {
        if(controlDic.ContainsKey(controlName))
        {
            for(int i = 0; i < controlDic[controlName].Count; i++)
            {
                if(controlDic[controlName][i] is T)
                {
                    return controlDic[controlName][i] as T;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 显示自己
    /// </summary>
    public virtual void ShowPanel()
    {

    }

    /// <summary>
    /// 隐藏自己
    /// </summary>
    public virtual void HidePanel()
    {

    }
}
