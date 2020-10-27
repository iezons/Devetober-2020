using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBordPos : Interact_SO
{
    [HideInInspector]
    public DoorController door = null;

    public bool isPowerOff;

    public bool isFixing;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        AddMenu("Operate", "Operate", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

    private void Update()
    {
        DoorStatus();
        if (isPowerOff)
        {
            Fixing();
        }
    }

    void DoorStatus()
    {
        if (currentHealth <= 0 && !isPowerOff)
        {
            door.currentHealth = 0;
            RemoveAndInsertMenu("Operate", "Repair", "Repair", true, SendFixingNPC, 1 << LayerMask.NameToLayer("NPC"));
            isPowerOff = true;
        }
    }

    void SendFixingNPC(object obj)
    {
        Debug.Log("Got a Fix Guy");
        GameObject gameObj = (GameObject)obj;
        NpcController npc = gameObj.GetComponent<NpcController>();
        npc.fixTarget = gameObject;
        npc.Dispatch(Locators[0].Locator.position);
        npc.TriggerFixing();
    }

    void Fixing()
    {
        if (isFixing)
        {
            //currentHealth += npc.status.fixRate * Time.deltaTime;
            currentHealth += 10 * Time.deltaTime;
            if(currentHealth >= maxHealth)
            {
                currentHealth = maxHealth;
                door.currentHealth = door.maxHealth;
                door.isPowerOff = false;
                door.isLocked = false;
                isPowerOff = false;
                isFixing = false;
            }
        }
    }
}
