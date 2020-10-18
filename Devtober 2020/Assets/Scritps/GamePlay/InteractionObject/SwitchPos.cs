using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

public class SwitchPos : Item_SO
{
    public DoorController door;

    private void Awake()
    {
        type = ItemType.Switch;
        AddMenu("TurnON", "TurnON", true, CallNPC, 1<<LayerMask.NameToLayer("NPC"));
    }

    public override void NPCInteract(int InteractWay = 0)
    {
        if(door != null)
        {
            door.isLocked = !door.isLocked;
        }
        
        if(MenuContains("TurnON") >= 0)
        {
            RemoveAndInsertMenu("TurnON", "TurnOFF", "TurnOFF", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        }
        else
        {
            RemoveAndInsertMenu("TurnOFF", "TurnON", "TurnON", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        }
    }
}
