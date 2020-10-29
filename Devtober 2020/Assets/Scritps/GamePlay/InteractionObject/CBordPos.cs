using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBordPos : Interact_SO
{
    public bool isPowerOff, isFixing, isLocked;

    public string code;

    public DoorController door = null;
    public SwitchPos swtich = null;
    private void Awake()
    {
        outline = GetComponent<Outline>();
        AddMenu("Operate", "Operate", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        if (code != "")
        {
            isLocked = true;
        }
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
        npc.HasInteract = false;
        npc.fixTargetTransform = Locators[0].Locator;
        npc.Dispatch(Locators[0].Locator.position);
        npc.TriggerFixing();
    }

    void Fixing()
    {
        if (isFixing)
        {
            if(currentHealth >= maxHealth)
            {
                currentHealth = maxHealth;              
                if (!isLocked)
                {
                    door.currentHealth = door.maxHealth;
                    door.isPowerOff = false;
                    door.isLocked = false;
                }             
                isPowerOff = false;
                isFixing = false;
            }
        }
    }
}
