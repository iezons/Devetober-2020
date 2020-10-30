using EvtGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay;
using DiaGraph;

public class NpcController_SpecialPrisoner : NpcController
{
    public DialogueGraph NoThingTalk;
    public DialogueGraph MedicalKit;
    public DialogueGraph JoinIn;

    public int Stage = 0;

    void Talking(object obj)
    {
        if (Stage == 0)
            GameManager.GetInstance().CurrentRoom.PlayingDialogue(NoThingTalk);
        else if (Stage == 1)
            GameManager.GetInstance().CurrentRoom.PlayingDialogue(MedicalKit);
        else if (Stage == 2)
            GameManager.GetInstance().CurrentRoom.PlayingDialogue(JoinIn);
    }
}