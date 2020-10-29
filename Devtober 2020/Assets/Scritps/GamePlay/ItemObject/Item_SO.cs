using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item_SO : ControllerBased
{
    public enum ItemType
    {
        None,
        MedicalKit,
        RepairedPart,
        Key
    }

    public ItemType type;

    public void CallNPC(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        NpcController npc = gameObj.GetComponent<NpcController>();
        npc.ReceiveInteractCall(gameObject);
    }

    public virtual void NPCInteract(int InteractWay = 0)
    {
        
    }

    public virtual void NPCInteractFinish(object obj)
    {
        
    }
}
