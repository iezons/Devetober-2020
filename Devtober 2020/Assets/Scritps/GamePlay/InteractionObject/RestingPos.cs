using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestingPos : Item_SO
{
    public float RecovryRate = 0.1f;

    private void Awake()
    {
        AddMenu("RestIn", "Rest in", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
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
