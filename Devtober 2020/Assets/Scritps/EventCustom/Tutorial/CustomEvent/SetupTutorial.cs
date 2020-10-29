using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvtGraph;

public class SetupTutorial : EventScriptInterface
{
    public override void DoEvent(object obj)
    {
        //base.DoEvent(obj);
        //这里Setup场景
        GameManager.GetInstance().SetupStage(0);
        FinishEvent();
    }
}
