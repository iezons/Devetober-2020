using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// UI层级
/// </summary>
public enum UI_Layer
{
    Bottom,
    Middle,
    Top,
    System
}

public class UIMgr : SingleClassBase<UIMgr>
{

    public Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    Transform Bottom;
    Transform Middle;
    Transform Top;
    Transform System;

    GameObject CurrentBottomPanel;
    GameObject CurrentMiddlePanel;
    GameObject CurrentTopPanel;
    GameObject CurrentSystemPanel;

    public void OnEnable()
    {
        //寻找或创建Canvas，然后设置它
        if(GameObject.Find("Canvas") == null)
        {
            ResMgr.GetInstance().LoadAsync<GameObject>("UI/Canvas", SetCanvas);
        }

        //寻找或创建EventSystem，然后设置它
        if (GameObject.Find("EventSystem") == null)
        {
            //创建EventSystem，让其过场的时候不被移除
            ResMgr.GetInstance().LoadAsync<GameObject>("UI/EventSystem", o =>
            {
                o.name = "EventSystem";
                DontDestroyOnLoad(o);
            });
        }
    }

    private void SetCanvas(GameObject obj)
    {
        obj.name = "Canvas";
        //寻找Canvas，让其过场的时候不被移除
        Transform Canvas = obj.transform;
        obj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(Screen.width, Screen.height);
        DontDestroyOnLoad(obj);
        //找到各个层级
        Bottom = Canvas.Find("Bottom");
        Middle = Canvas.Find("Middle");
        Top = Canvas.Find("Top");
        System = Canvas.Find("System");
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="PanelName">面板名</param>
    /// <param name="layer">面板应该显示在哪一层</param>
    /// <param name="callBack">面板创建成功后要执行的内容</param>
    public void ShowPanel<T>(string PanelName, UI_Layer layer = UI_Layer.Bottom, UnityAction<T> callBack = null) where T:BasePanel
    {
        switch (layer)
        {
            case UI_Layer.Bottom:
                if(CurrentBottomPanel != null)
                {
                    HidePanel(CurrentBottomPanel.gameObject.name);
                    CurrentBottomPanel = null;
                }
                break;
            case UI_Layer.Middle:
                if (CurrentMiddlePanel != null)
                {
                    HidePanel(CurrentMiddlePanel.gameObject.name);
                    CurrentMiddlePanel = null;
                }
                break;
            case UI_Layer.Top:
                if (CurrentTopPanel != null)
                {
                    HidePanel(CurrentTopPanel.gameObject.name);
                    CurrentTopPanel = null;
                }
                break;
            case UI_Layer.System:
                if (CurrentSystemPanel != null)
                {
                    HidePanel(CurrentSystemPanel.name);
                    CurrentSystemPanel = null;
                }
                break;
            default:
                break;
        }

        if (panelDic.ContainsKey(PanelName))
        {
            panelDic[PanelName].ShowPanel();
            callBack?.Invoke(panelDic[PanelName] as T);
            return;
        }
        else
        {
            ResMgr.GetInstance().LoadAsync<GameObject>("UI/" + PanelName, (obj) =>
            {
                //将其移到Canvas中的各个层级之下
                Transform father = Bottom;
                switch (layer)
                {
                    case UI_Layer.Bottom:
                        CurrentBottomPanel = obj;
                        break;
                    case UI_Layer.Middle:
                        CurrentMiddlePanel = obj;
                        father = Middle;
                        break;
                    case UI_Layer.Top:
                        CurrentTopPanel = obj;
                        father = Top;
                        break;
                    case UI_Layer.System:
                        CurrentSystemPanel = obj;
                        father = System;
                        break;
                }

                //设置父对象
                obj.transform.SetParent(father);

                //设置其相对位置
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;

                (obj.transform as RectTransform).offsetMax = Vector2.zero;
                (obj.transform as RectTransform).offsetMin = Vector2.zero;

                //得到预设体身上的BasePanel脚本
                T panel = obj.GetComponent<T>();
                panel.name = PanelName;
                panel.ShowPanel();
                //将其添加到PanelDic中
                panelDic.Add(PanelName, panel);
                //处理面板创建完成后，要执行的方法
                callBack?.Invoke(panel);
            });
        }
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="panelName">将要隐藏的面板</param>
    public void HidePanel(string panelName)
    {
        if(panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].HidePanel();
            GameObject.Destroy(panelDic[panelName].gameObject);
            panelDic.Remove(panelName);
        }
    }
}
