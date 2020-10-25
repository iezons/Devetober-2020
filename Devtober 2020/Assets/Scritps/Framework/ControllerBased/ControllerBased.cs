using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class ControllerBased : MonoBehaviour
{
    #region MouseHover
    public float OutlineWidth = 3;
    public Outline outline;

    public virtual void SetOutline(bool IsOutline)
    {
        outline.SetOutline(IsOutline);
        if(IsOutline)
        {
            outline.OutlineWidth = OutlineWidth;
        }
        else
        {
            outline.OutlineWidth = 0;
        }
        
    }
    #endregion

    #region RightClickMenus

    [Header("State")]
    public bool IsInteracting = false;

    public List<RightClickMenus> rightClickMenus = new List<RightClickMenus>();

    public void Interacting()
    {
        IsInteracting = true;
    }

    public void NotInteracting()
    {
        IsInteracting = false;
    }

    public void AddMenu(string unchangedName, string functionName, bool NeedTarget, MenuHandler function, int InteractLayer = 1 << 0, object DefaultCallValue = null)
    {
        rightClickMenus.Add(new RightClickMenus { unchangedName = unchangedName });
        rightClickMenus[rightClickMenus.Count - 1].functionName = functionName;
        rightClickMenus[rightClickMenus.Count - 1].NeedTarget = NeedTarget;
        rightClickMenus[rightClickMenus.Count - 1].InteractLayer = InteractLayer;
        rightClickMenus[rightClickMenus.Count - 1].function += function;
        rightClickMenus[rightClickMenus.Count - 1].DefaultCallValue = DefaultCallValue;
    }

    public void InsertMenu(int index, string unchangedName, string functionName, bool NeedTarget, MenuHandler function, int InteractLayer = 1 << 0, object DefaultCallValue = null)
    {
        rightClickMenus.Insert(index, new RightClickMenus { unchangedName = unchangedName });
        rightClickMenus[index].functionName = functionName;
        rightClickMenus[index].NeedTarget = NeedTarget;
        rightClickMenus[index].InteractLayer = InteractLayer;
        rightClickMenus[index].function += function;
        rightClickMenus[index].DefaultCallValue = DefaultCallValue;
    }

    /// <summary>
    /// WARNING: If you want to remove a menu and insert a new one at same index, PLS use RemoveAndInsertMenu
    /// </summary>
    /// <param name="unchangedName"></param>
    public void RemoveMenu(string unchangedName)
    {
        rightClickMenus.RemoveAll((Rcm) => (Rcm.unchangedName == unchangedName));
    }

    public void RemoveAndInsertMenu(string RemoveUnchangedName, string InsertUnchangedName, string functionName, bool NeedTarget, MenuHandler function, int InteractLayer = 1 << 0, object DefaultCallValue = null)
    {
        int index = rightClickMenus.FindIndex((Rcm) => (Rcm.unchangedName == RemoveUnchangedName));
        if(index >= 0)
        {
            rightClickMenus.RemoveAt(index);
            InsertMenu(index, InsertUnchangedName, functionName, NeedTarget, function, InteractLayer, DefaultCallValue);
        }
    }

    public int MenuContains(string UnchangedName)
    {
        return rightClickMenus.FindIndex((Rcm) => (Rcm.unchangedName == UnchangedName));
    }

    public void RemoveAllMenu()
    {
        rightClickMenus.Clear();
    }
    #endregion
}
