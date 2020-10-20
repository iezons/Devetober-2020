using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalKit : Item_SO
{
    public override void NPCInteract(int InteractWay = 0)
    {
        Destroy(gameObject);
    }

    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        RemoveMenu("Grab");
    }

    void Init()
    {
        type = ItemType.MedicalKit;
        AddMenu("Grab", "Grab", true, CallNPC, 1<<LayerMask.NameToLayer("NPC"));
    }
}
