using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RightClickButtonSC : MonoBehaviour
{
    public RightClickMenus menu;
    public object obj;

    public void AfterInstantiate()
    {
        GetComponentInChildren<Text>().text = menu.functionName;
    }

    public void DoFunction()
    {
        if(menu.NeedTarget)
        {
            GameManager.GetInstance().RightClickMs = menu;
            GameManager.GetInstance().IsWaitingForClickObj = true;
            Debug.Log("WaitingForNextObject");
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            menu.DoFunction(obj);
            transform.parent.gameObject.SetActive(false);
        }
    }
}
