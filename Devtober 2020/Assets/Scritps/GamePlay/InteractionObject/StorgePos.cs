using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorgePos : Interact_SO
{
    public int MaxStorge = 5;
    public List<string> StorgeItem = new List<string>();

    private void Awake()
    {
        type = InteractType.Storge;
        UpdateMenu();
    }

    public void Store(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        gameObj.GetComponent<NpcController>().ReceiveGrabOut(this, -1, false);
    }

    public void GrabOut(object obj)
    {
        DefaultValueWithGO dwg = (DefaultValueWithGO)obj;
        dwg.GO.GetComponent<NpcController>().ReceiveGrabOut(this, (int)dwg.DefaultValue, true);
    }

    public override void NPCInteract(int InteractWay = 0)
    {
        if(InteractWay >= 0)
            StorgeItem.RemoveAt(InteractWay);
        UpdateMenu();
    }

    void UpdateMenu()
    {
        RemoveAllMenu();
        if(StorgeItem.Count < MaxStorge)
        {
            AddMenu("Store", "Store" + StorgeItem.Count + "/" + MaxStorge, true, Store, 1 << LayerMask.NameToLayer("NPC"));
        }
        for (int i = 0; i < StorgeItem.Count; i++)
        {
            AddMenu("Grab" + i.ToString(), "Grab " + StorgeItem[i], true, GrabOut, 1 << LayerMask.NameToLayer("NPC"), i);
        }
    }
}
