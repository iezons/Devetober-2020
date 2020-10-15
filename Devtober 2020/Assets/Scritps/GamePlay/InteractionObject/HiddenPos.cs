using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

public class HiddenPos : MonoBehaviour
{
    public bool isTaken;

    public Transform finalPos;
    public List<RightClickMenus> rightClickMenus = new List<RightClickMenus>();

    void Start()
    {
        AddMenu("Hide In", "Hide In", CallNPC);
    }

    public void AddMenu(string unchangedName, string functionName, MenuHandler function)
    {
        rightClickMenus.Add(new RightClickMenus { unchangedName = unchangedName });
        rightClickMenus[rightClickMenus.Count - 1].functionName = functionName;
        rightClickMenus[rightClickMenus.Count - 1].function += function;
    }

    public void CallNPC(object temp)
    {
        NpcController npc = (NpcController)temp;
        npc.ReceiveLockerCall(finalPos);
    }
}
