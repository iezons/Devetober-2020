using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvtGraph;

public enum GameManagerState
{
    OFF,
    PROGRESSING,
    PAUSED
}

public class GameManager : MonoBehaviour
{
    public EventGraph eventGraph;
    public List<NpcController> NPC;
    public List<GameObject> Room;
    public GameManagerState gmState;

    bool justEnter = true;

    // Start is called before the first frame update
    void Awake()
    {
        SetupScene();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
    }

    void NPCAdd(NpcController NPC_obj)
    {
        NPC.Add(NPC_obj);
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (gmState)
        {
            case GameManagerState.OFF:
                if(justEnter)
                {
                    justEnter = false;
                    Next();
                }
                break;
            case GameManagerState.PROGRESSING:
                if (justEnter)
                {
                    justEnter = false;
                }
                break;
            case GameManagerState.PAUSED:
                if (justEnter)
                {
                    justEnter = false;
                }
                break;
            default:
                break;
        }
    }

    void SetupScene()
    {
        
    }

    void GoToState(GameManagerState next)
    {
        justEnter = true;
        gmState = next;
    }

    void Next()
    {
        switch (gmState)
        {
            case GameManagerState.OFF:
                eventGraph.SetNode();
                eventGraph.Next();
                TriggerEvent();
                break;
            case GameManagerState.PROGRESSING:
                break;
            case GameManagerState.PAUSED:
                eventGraph.Next();
                TriggerEvent();
                break;
            default:
                break;
        }
    }

    void TriggerEvent()
    {
        GoToState(GameManagerState.PROGRESSING);
        EventNode cur =  eventGraph.current as EventNode;
        if(cur != null)
        {
            List<EventSO> eventSO = cur.eventSO;
            for (int i = 0; i < eventSO.Count; i++)
            {
                EventSO evt = eventSO[i];
                if (evt != null)
                {
                    switch (evt.doingWith)
                    {
                        case DoingWith.NPC:
                            switch (evt.doingWithNPC)
                            {
                                case DoingWithNPC.Talking:
                                    for (int a = 0; a < evt.NPCTalking.Count; a++)
                                    {
                                        for (int b = 0; b < NPC.Count; b++)
                                        {
                                            if(NPC[b].npc_so.npcName == evt.NPCTalking[a].MoveToClassA.Name)
                                            {
                                                NPC[b].npc_so.toDoList.Add(evt);
                                            }
                                        }
                                    }
                                    break;
                                case DoingWithNPC.MoveTo:
                                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                                    {
                                        //Debug.Log("2333");
                                        for (int b = 0; b < NPC.Count; b++)
                                        {
                                            //Debug.Log("2333");
                                            if (NPC[b].npc_so.npcName == evt.NPCWayPoint[a].Name)
                                            {
                                                NPC[b].npc_so.toDoList.Add(evt);
                                            }
                                        }
                                    }
                                    break;
                                case DoingWithNPC.Patrol:
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case DoingWith.Room:
                            //Nothing
                            break;
                        case DoingWith.Enemy:
                            //TODO
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        GoToState(GameManagerState.PAUSED);
    }
}
