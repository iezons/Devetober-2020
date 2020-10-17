using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalPos : Item_SO
{
    private void Awake()
    {
        AddMenu("Operate", "Operate", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

}
