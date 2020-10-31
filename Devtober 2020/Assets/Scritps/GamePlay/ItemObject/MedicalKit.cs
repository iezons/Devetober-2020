using DiaGraph;
using GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalKit : Item_SO
{
    public float HPRecovery = 50f;
    public bool Trigger = false;
    public RoomTracker room;
    public NpcController Guard;
    public MainLevelEventHolder holder;
    public DialogueGraph graph_Xan;
    public DialogueGraph graph_Sat;
    public DialogueGraph graph_Mel;
    public MeshRenderer rende;
    public BoxCollider coll;

    private void Awake()
    {
        rende = GetComponent<MeshRenderer>();
        coll = GetComponent<BoxCollider>();
    }

    public override void NPCInteract(int InteractWay = 0)
    {
        if(Trigger)
        {
            List<GameObject> Xan = room.NPC().FindAll((x) => x.GetComponent<NpcController>().status.npcName == "Xanthe Eburnus");
            List<GameObject> Sat = room.NPC().FindAll((x) => x.GetComponent<NpcController>().status.npcName == "Saturnus Mocilla");
            List<GameObject> Mel = room.NPC().FindAll((x) => x.GetComponent<NpcController>().status.npcName == "Melpomene Barbatus");
            if (Xan.Count > 0)
            {
                Guard.CurrentInteractObject.NPCInteractFinish(null);
                Guard.inAnimState = true;
                Guard.AnimStateName = "Talking1";
                NpcController xannpc = Xan[0].GetComponent<NpcController>();
                xannpc.inAnimState = true;
                xannpc.AnimStateName = "Talking2";
                holder.HolderMKNPC = xannpc;
                room.DiaPlay.PlayDia(graph_Xan);
                xannpc.FacingEachOtherCoro(Guard.transform);
                Guard.FacingEachOtherCoro(xannpc.transform);
            }
            else if (Sat.Count > 0)
            {
                Guard.CurrentInteractObject.NPCInteractFinish(null);
                Guard.inAnimState = true;
                Guard.AnimStateName = "Talking1";
                NpcController satnpc = Sat[0].GetComponent<NpcController>();
                satnpc.inAnimState = true;
                satnpc.AnimStateName = "Talking2";
                holder.HolderMKNPC = satnpc;
                room.DiaPlay.PlayDia(graph_Sat);
                satnpc.FacingEachOtherCoro(Guard.transform);
                Guard.FacingEachOtherCoro(satnpc.transform);
            }
            else if (Mel.Count > 0)
            {
                Guard.CurrentInteractObject.NPCInteractFinish(null);
                Guard.inAnimState = true;
                Guard.AnimStateName = "Talking1";
                NpcController melnpc = Mel[0].GetComponent<NpcController>();
                melnpc.inAnimState = true;
                melnpc.AnimStateName = "Talking2";
                holder.HolderMKNPC = melnpc;
                room.DiaPlay.PlayDia(graph_Mel);
                melnpc.FacingEachOtherCoro(Guard.transform);
                Guard.FacingEachOtherCoro(melnpc.transform);
            }
            EventCenter.GetInstance().EventTriggered("01_IsEnterCafe");
        }
        coll.enabled = false;
        rende.enabled = false;
        Destroy(gameObject, 2f);
    }

    void OnEnable()
    {
        Init();
    }

    private void OnDestroy()
    {
        RemoveMenu("Grab");
    }

    void Init()
    {
        outline = GetComponent<Outline>();
        type = ItemType.MedicalKit;
        AddMenu("Grab", "Grab", true, CallNPC, 1<<LayerMask.NameToLayer("NPC"));
    }
}
