using DiaGraph;
using EvtGraph;
using GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainLevelEventHolder : MonoBehaviour
{
    public NpcController NPC_SP;
    public RoomTracker RoomFor03;
    public HiddenPos HidingLocker;

    [Header("Interact")]
    public HiddenPos hide;

    [Header("Stage02")]
    public NpcController Xan;

    [Header("01_Dia")]
    public DialogueGraph graph03;
    public EventSO NPCTalking;
    public TerminalPos PC;
    public DoorController Door;

    // Start is called before the first frame update
    void OnEnable()
    {
        a("01_PrisonerCanInteract", () => { NPC_SP.IsInteracting = false; });
        a("01_DiaTwoTrigger", DiaTwoTrigger);//事件机 forcemove
        a("01_ChefOut", () => hide.IsInteracting = false);
        a("01_PriPatrol", () => NPC_SP.SwitchAnimState(false));
        a("01_PrisonerStartTalking", PrisonerStartTalking);
        a("01_XantheTurnToCamera", () => Xan.FacingEachOther(true));
    }

    void a(string name, UnityAction action)
    {
        EventCenter.GetInstance().AddEventListener(name, action);
    }

    void b(string name)
    {
        EventCenter.GetInstance().DiaEventTrigger(name);
    }

    void DiaTwoTrigger()
    {
        GameManager.GetInstance().EventForceNext();
        GameManager.GetInstance().SetupStage(2);
        PC.IsInteracting = false;
        Door.IsInteracting = false;
    }

    void PrisonerStartTalking()
    {
        NPC_SP.inAnimState = true;
        NPC_SP.RemoveAllMenu();
        NPC_SP.AddMenu("Talking", "Talking", false, NPC_SP.SpecialTalking);
        NPC_SP.CurrentInteractObject.NPCInteractFinish(null);
    }

    // Update is called once per frame
    void Update()
    {
        //01_LookAtMedicalKit
        //e=01_NPCInCafe   Getout
        //              01_Xanthe
        //              01_Saturnus
        //              01_Melpomene
    }
}
