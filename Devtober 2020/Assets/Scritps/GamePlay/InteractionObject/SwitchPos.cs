using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

public class SwitchPos : Interact_SO
{
    public DoorController door;
    public GameObject Light;

    private void Awake()
    {
        type = InteractType.Switch;
        if(door != null)
        {
            AddMenu("UnLock Door", "UnLock Door", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        }
        else if(Light != null)
        {
            AddMenu("TurnON Light", "TurnON Light", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        }
    }

    public override void NPCInteract(int InteractWay = 0)
    {
        if(door != null)
        {
            door.isLocked = !door.isLocked;
            if (MenuContains("UnLock Door") >= 0)
            {
                RemoveAndInsertMenu("UnLock Door", "Lock Door", "Lock Door", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
            }
            else
            {
                RemoveAndInsertMenu("Lock Door", "UnLock Door", "UnLock Door", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
            }
        }
        else if(Light != null)
        {
            Light.SetActive(!Light.activeSelf);
            if (MenuContains("TurnON Light") >= 0)
            {
                RemoveAndInsertMenu("TurnON Light", "Turn OFF Light", "Turn OFF Light", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
            }
            else
            {
                RemoveAndInsertMenu("Turn OFF Light", "TurnON Light", "TurnON Light", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
            }
        }
    }
}
