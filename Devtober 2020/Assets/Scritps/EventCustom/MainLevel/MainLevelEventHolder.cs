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
    public NpcController Guard;

    [Header("Interact")]
    public HiddenPos hide;

    [Header("Stage02")]
    public NpcController Xan;
    public NpcController Prisoner;

    [Header("01_Dia")]
    public DialogueGraph graph03;
    public EventSO NPCTalking;
    public TerminalPos PC;
    public DoorController Door;
    public HiddenPos HidingLocker;
    public NpcController HolderMKNPC;
    public NpcController Priest;
    public RestingPos resting;

    // Start is called before the first frame update
    void OnEnable()
    {
        a("01_PrisonerCanInteract", () => { NPC_SP.IsInteracting = false; });
        a("01_DiaTwoTrigger", DiaTwoTrigger);//事件机 forcemove
        a("01_ChefOut", () => { hide.IsInteracting = false; Prisoner.Stage = 1; });
        a("01_PriPatrol", () => NPC_SP.SwitchAnimState(false));
        a("01_PrisonerStartTalking", PrisonerStartTalking);
        a("01_XantheTurnToCamera", () => Xan.FacingEachOther(true));
        a("02_PriestHeal", () => { Prisoner.Stage = 2; });
        a("01_PriPatrol", () => { Prisoner.IsPrisoner = false; Prisoner.SwitchAnimState(false); });
        a("02_GuardJoin", GuardGoBack);
        a("02_PrisonerMedicalKit", PrisonerMedicalKit);
    }

    void a(string name, UnityAction action)
    {
        EventCenter.GetInstance().AddEventListener(name, action);
    }

    void b(string name)
    {
        EventCenter.GetInstance().DiaEventTrigger(name);
    }

    void PrisonerMedicalKit()
    {

    }

    void GuardGoBack()
    {
        Debug.Log("GoBack");
        Guard.IsInteracting = false;
        Guard.inAnimState = false;
        Guard.AnimStateName = "";
        Guard.SwitchAnimState(false);

        HolderMKNPC.inAnimState = false;
        HolderMKNPC.AnimStateName = "";
        HolderMKNPC.IsInteracting = false;
        HolderMKNPC.SwitchAnimState(false);
        Priest.IsInteracting = false;
    }

    void DiaTwoTrigger()
    {
        if(GameManager.GetInstance().TriggeringEventNode != null)
        {
            if(GameManager.GetInstance().TriggeringEventNode.EventName == "1_pristay")
            {
                GameManager.GetInstance().EventForceNext();
            }
        }
        GameManager.GetInstance().SetupStage(2);
        GameManager.GetInstance().SetupOption(GameManager.GetInstance().CurrentRoom);
        PC.IsInteracting = false;
        Door.IsInteracting = false;
        HidingLocker.IsInteracting = true;
        hide.IsInteracting = true;
    }

    void PrisonerStartTalking()
    {
        NPC_SP.inAnimState = true;
        NPC_SP.RemoveAllMenu();
        NPC_SP.AddMenu("Talking", "Talking", false, NPC_SP.SpecialTalking);
        if(NPC_SP.CurrentInteractObject != null)
        {
            NPC_SP.CurrentInteractObject.NPCInteractFinish(null);
        }
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
