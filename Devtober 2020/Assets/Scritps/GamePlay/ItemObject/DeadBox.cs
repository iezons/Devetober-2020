using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBox : Item_SO
{
    public string code;
    public float HPRecovery;
    public override void NPCInteract(int InteractWay = 0)
    {
        Destroy(gameObject);
    }

    void OnEnable()
    {
        Init();
    }

    private void OnDestroy()
    {
        RemoveMenu("Grab");
    }

    void Init()
    {
        outline = GetComponent<Outline>();
        AddMenu("Grab", "Grab", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }
}
