using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

public class HiddenPos : Interact_SO
{
    private void Awake()
    {
        Anim = GetComponent<Animator>();
        AddMenu("Hide In", "Hide In", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

}
