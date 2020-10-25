using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestingPos : Interact_SO
{
    public float RecovryRate;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        AddMenu("RestIn", "Rest in", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
        recordColliderSize = GetComponent<BoxCollider>().size;
        recordColliderCenter = GetComponent<BoxCollider>().center;
    }

    private void Update()
    {
        for (int i = 0; i < Locators.Count; i++)
        {
            if(Locators[i].npc != null)
            {
                Locators[i].npc.RecoverStamina(RecovryRate);
            }
        }
    }
}
