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
        if (menu.unchangedName == "Move")
        {
            GameManager.GetInstance().IsWaitingForMovePoint = true;
            GameManager.GetInstance().MovePointFunction = menu;
            Debug.Log("Move Do Function");
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            menu.DoFunction(obj);
            transform.parent.gameObject.SetActive(false);
        }
    }
}
