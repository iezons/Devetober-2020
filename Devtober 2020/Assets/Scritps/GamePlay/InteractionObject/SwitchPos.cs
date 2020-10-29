using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

public class SwitchPos : Interact_SO
{
    public CBordPos cBord;
    public DoorController door;
    public GameObject Light;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        type = InteractType.Switch;
        if(door != null && cBord.isLocked)
        {
            if(!door.isLocked)
                AddMenu("SwtichState", "Lock Door", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
            else
                AddMenu("SwtichState", "UnLock Door", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        }
        else if(Light != null)
        {
            AddMenu("TurnON Light", "TurnON Light", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        }
    }

    public override void SetOutline(bool IsOutline)
    {
        base.SetOutline(IsOutline);
        if(door != null)
        {
            door.SetOutline(IsOutline);
        }
    }

    private void Update()
    {
        if(rightClickMenus.Count != 0)
        {
            if (!door.isLocked && rightClickMenus[0].functionName != "Lock Door")
            {
                rightClickMenus[0].functionName = "Lock Door";
            }
            else if (door.isLocked && rightClickMenus[0].functionName != "UnLock Door")
            {
                rightClickMenus[0].functionName = "UnLock Door";
            }
        }      
    }


    public override void NPCInteract(int InteractWay = 0)
    {
        Debug.Log("Switch Interacted");
        if(door != null)
        {
            if (!door.isPowerOff)
            {
                if (rightClickMenus[0].functionName == "UnLock Door")
                {
                    door.isNPCCalled = true;
                    door.isLocked = false;
                    rightClickMenus[0].functionName = "Lock Door";
                }
                else if (rightClickMenus[0].functionName == "Lock Door")
                {
                    door.isNPCCalled = true;
                    door.isLocked = true;
                    rightClickMenus[0].functionName = "UnLock Door";
                }
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
