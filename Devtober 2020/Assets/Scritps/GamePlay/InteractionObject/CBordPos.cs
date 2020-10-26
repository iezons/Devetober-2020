using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBordPos : Interact_SO
{
    public DoorController door;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        AddMenu("Operate", "Operate", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

    private void Update()
    {
        
    }

    void PowerOffDoor()
    {
        if (currentHealth <= 0)
        {
            door.isLocked = true;
            door.isPowerOff = true;
            door.currentHealth = 0;
        }
    }
}
