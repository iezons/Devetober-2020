using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBased : MonoBehaviour
{
    #region RightClickMenus
    public bool HasRightClickMenu = false;
    public List<RightClickMenus> rightClickMenus = new List<RightClickMenus>();

    public void AddMenu(string unchangedName, string functionName, bool NeedTarget, MenuHandler function)
    {
        rightClickMenus.Add(new RightClickMenus { unchangedName = unchangedName });
        rightClickMenus[rightClickMenus.Count - 1].functionName = functionName;
        rightClickMenus[rightClickMenus.Count - 1].NeedTarget = NeedTarget;
        rightClickMenus[rightClickMenus.Count - 1].function += function;
    }

    public void InsertMenu(int index, string unchangedName, string functionName, bool NeedTarget, MenuHandler function)
    {
        rightClickMenus.Insert(index, new RightClickMenus { unchangedName = unchangedName });
        rightClickMenus[index].functionName = functionName;
        rightClickMenus[index].NeedTarget = NeedTarget;
        rightClickMenus[index].function += function;
    }

    /// <summary>
    /// WARNING: If you want to remove a menu and insert a new one at same index, PLS use RemoveAndInsertMenu
    /// </summary>
    /// <param name="unchangedName"></param>
    public void RemoveMenu(string unchangedName)
    {
        rightClickMenus.RemoveAll((Rcm) => (Rcm.unchangedName == unchangedName));
    }

    public void RemoveAndInsertMenu(string RemoveUnchangedName, string InsertUnchangedName, string functionName, bool NeedTarget, MenuHandler function)
    {
        int index = rightClickMenus.FindIndex((Rcm) => (Rcm.unchangedName == RemoveUnchangedName));
        if(index >= 0)
        {
            rightClickMenus.RemoveAt(index);
            InsertMenu(index, InsertUnchangedName, functionName, NeedTarget, function);
        }
    }
    #endregion
}
