using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

public class HiddenPos : ControllerBased
{
    public bool isTaken;

    public Transform finalPos;

    void Awake()
    {
        HasRightClickMenu = true;
    }

    void Start()
    {
        AddMenu("Hide In", "Hide In", true, CallNPC);
    }

    public void CallNPC(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        NpcController npc = gameObj.GetComponent<NpcController>();
        npc.ReceiveLockerCall(finalPos.position);
    }
}
