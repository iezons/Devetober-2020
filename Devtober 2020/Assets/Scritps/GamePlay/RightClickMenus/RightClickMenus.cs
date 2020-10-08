using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void MenuHandler(object obj);
public class RightClickMenus
{
    public string unchangedName;
    public string functionName;
    public event MenuHandler function;
    public void DoFunction(object obj)
    {
        function(obj);
    }
}
