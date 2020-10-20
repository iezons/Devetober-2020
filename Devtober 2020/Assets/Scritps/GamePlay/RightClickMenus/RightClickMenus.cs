using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void MenuHandler(object obj);
[Serializable]
public class RightClickMenus
{
    public string unchangedName;
    public string functionName;
    public bool NeedTarget = false;
    public LayerMask InteractLayer;
    public event MenuHandler function;
    public object DefaultCallValue = null;
    public void DoFunction(object obj)
    {
        function(obj);
    }
}
