using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalPos : Interact_SO
{
    private void Awake()
    {
        AddMenu("Operate", "Operate", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

}
