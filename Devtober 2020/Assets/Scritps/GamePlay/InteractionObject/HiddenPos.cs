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
                RemoveAndInsertMenu("Hide In","Leave", "Leave", false, GetOut, 1 << LayerMask.NameToLayer("HiddenPos"));
                break;
            case ItemType.Box:
                break;
            default:
                break;
        }
    }

    public void GetOut(object obj)
    {
        for (int i = 0; i < Locators.Count; i++)
        {
            Locators[i].npc.PlayGetOutAnim(gameObject);
        }
        switch (type)
        {
            case ItemType.Locker:
                PlayAnimation("1");
                break;
            case ItemType.Box:
                break;
            default:
                break;
        }
    }
}
