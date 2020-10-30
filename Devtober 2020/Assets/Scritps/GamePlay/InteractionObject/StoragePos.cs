using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoragePos : Interact_SO
{
    public int MaxStorage = 5;
    public List<Item_SO.ItemType> StorageItem = new List<Item_SO.ItemType>();

    private void Awake()
    {
        outline = GetComponent<Outline>();
        type = InteractType.Storage;
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
            StorageItem.RemoveAt(InteractWay);
        UpdateMenu();
    }

    public void UpdateMenu()
    {
        RemoveAllMenu();
        if(StorageItem.Count < MaxStorage)
        {
            AddMenu("Store", "Store" + StorageItem.Count + "/" + MaxStorage, true, Store, 1 << LayerMask.NameToLayer("NPC"));
        }
        for (int i = 0; i < StorageItem.Count; i++)
        {
            AddMenu("Grab" + i.ToString(), "Grab " + StorageItem[i], true, GrabOut, 1 << LayerMask.NameToLayer("NPC"), i);
        }
    }
}
