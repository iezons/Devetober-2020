using DiaGraph;
using EvtGraph;
using GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCafeTrigger : MonoBehaviour
{
    public NpcController Guard;
    public List<Transform> Waypoints;
    public EventSO evt;
    public RoomTracker room;
    public DialogueGraph graph_Xan;
    public DialogueGraph graph_Sat;
    public DialogueGraph graph_Mel;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("NPC"))
        {
            List<GameObject> Xan = room.NPC().FindAll((x) => x.GetComponent<NpcController>().status.npcName == "Xanthe Eburnus");
            List<GameObject> Sat = room.NPC().FindAll((x) => x.GetComponent<NpcController>().status.npcName == "Saturnus Mocilla");
            List<GameObject> Mel = room.NPC().FindAll((x) => x.GetComponent<NpcController>().status.npcName == "Melpomene Barbatus");
            if (Xan.Count > 0)
            {
                evt.Dialogue_Graph = graph_Xan;
                evt.NPCTalking[0].moveToClasses[0].Obj = Xan[0];
            }
            else if (Sat.Count > 0)
            {
                evt.Dialogue_Graph = graph_Sat;
                evt.NPCTalking[0].moveToClasses[0].Obj = Sat[0];
            }
            else if (Mel.Count > 0)
            {
                evt.Dialogue_Graph = graph_Mel;
                evt.NPCTalking[0].moveToClasses[0].Obj = Mel[0];
            }
            GameManager.GetInstance().SetCurEventNode("EnterCafe", evt);
            EventCenter.GetInstance().EventTriggered("01_IsEnterCafe");
            Destroy(gameObject);
        }
    }
}
