using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightClickButton : MonoBehaviour
{
    public NpcController.RightClickMenus clickMenus;
    // Start is called before the first frame update
    
    public void DoFunction(object obj)
    {
        clickMenus.DoFunction(obj);
    }

}
