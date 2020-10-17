using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

public class HiddenPos : Item_SO
{

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        AddMenu("Hide In", "Hide In", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

    public override void NPCInteract(int InteractWay = 0)
    {
        switch (type)
        {
            case ItemType.Locker:
                PlayAnimation(InteractWay.ToString());
                break;
            case ItemType.Box:
                break;
            case ItemType.Bed:
                break;
            case ItemType.Chair:
                break;
            case ItemType.Terminal:
                break;
            default:
                break;
        }
    }
}
