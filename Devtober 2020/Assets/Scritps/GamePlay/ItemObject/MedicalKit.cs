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
    public RestingPos resting;

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
                Guard.SwitchAnimState(true, "Talking1");
                Xan[0].GetComponent<NpcController>().SwitchAnimState(true, "Talking2");
                holder.HolderMKNPC = Xan[0].GetComponent<NpcController>();
                room.DiaPlay.PlayDia(graph_Xan);
            }
            else if (Sat.Count > 0)
            {
                Guard.CurrentInteractObject.NPCInteractFinish(null);
                Guard.SwitchAnimState(true, "Talking1");
                Sat[0].GetComponent<NpcController>().SwitchAnimState(true, "Talking2");
                holder.HolderMKNPC = Sat[0].GetComponent<NpcController>();
                room.DiaPlay.PlayDia(graph_Sat);
            }
            else if (Mel.Count > 0)
            {
                Guard.CurrentInteractObject.NPCInteractFinish(null);
                Guard.SwitchAnimState(true, "Talking1");
                Mel[0].GetComponent<NpcController>().SwitchAnimState(true, "Talking2");
                holder.HolderMKNPC = Mel[0].GetComponent<NpcController>();
                room.DiaPlay.PlayDia(graph_Mel);
            }
            resting.IsInteracting = false;
            EventCenter.GetInstance().EventTriggered("01_IsEnterCafe");
            Destroy(gameObject);
        }
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
