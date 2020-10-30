using DiaGraph;
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

    [Header("Dia")]
    public DialogueGraph graph03;

    // Start is called before the first frame update
    void OnEnable()
    {
        a("01_PrisonerCanInteract", () => { NPC_SP.IsInteracting = false; });
        a("01_DiaTwoTrigger", () => {GameManager.GetInstance().EventForceNext(); GameManager.GetInstance().SetupStage(2); });//事件机 forcemove
        //a("01_XantheTurnToCamera", () => {; });
        //a("01_GuardGetOut", () => {; })
        a("01_03DiaPlay", () => RoomFor03.PlayingDialogue(graph03));
        a("01_ChefOut", () => hide.IsInteracting = false);
        a("01_PriPatrol", () => NPC_SP.SwitchAnimState(false));
        a("01_PrisonerStartTalking", PrisonerStartTalking);
    }

    void a(string name, UnityAction action)
    {
        EventCenter.GetInstance().AddEventListener(name, action);
    }

    void b(string name)
    {
        EventCenter.GetInstance().DiaEventTrigger(name);
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
