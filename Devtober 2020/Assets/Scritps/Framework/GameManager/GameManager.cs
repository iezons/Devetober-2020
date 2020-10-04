using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvtGraph;
using DiaGraph;
using TMPro;
using UnityEngine.UI;

public enum GameManagerState
{
    OFF,
    PROGRESSING,
    PAUSED
}

public class GameManager : MonoBehaviour
{
    public EventGraph eventGraph;
    public DialoguePlay DiaPlay;
    public List<NpcController> NPC;
    public List<GameObject> Room;
    public GameManagerState gmState;
    public string HistoryText;
    public TMP_Text TMPText;
    public GameObject OptionButtonPrefab;
    public List<Button> Option;
    public Transform ButtonContent;

    bool justEnter = true;

    // Start is called before the first frame update
    void Awake()
    {
        SetupScene();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
        EventCenter.GetInstance().AddEventListener<DialogueGraph>("GM.DialoguePlay.Start", PlayingDialogue);
        EventCenter.GetInstance().AddEventListener("DialoguePlay.PAUSED", DialoguePaused);
        EventCenter.GetInstance().AddEventListener("DialoguePlay.Finished", DialogueFinish);
        EventCenter.GetInstance().AddEventListener<List<OptionClass>>("DialoguePlay.OptionShowUP", DialogueOptionShowUp);
    }

    void NPCAdd(NpcController NPC_obj)
    {
        NPC.Add(NPC_obj);
    }

    void PlayingDialogue(DialogueGraph graph)
    {
        Debug.Log("2333");
        if (DiaPlay.d_state == DiaState.OFF)
            EventCenter.GetInstance().EventTriggered("DialoguePlay.Start", graph);
    }

    void DialoguePaused()
    {
        if(DiaPlay.n_state == NodeState.Dialogue)
            StartCoroutine(WaitAndPlay());
        else if (DiaPlay.n_state == NodeState.Option)
        {

        }
    }

    IEnumerator WaitAndPlay()
    {
        yield return new WaitForSeconds(1);
        HistoryText += DiaPlay.WholeText;
        EventCenter.GetInstance().EventTriggered("DialoguePlay.Next", 0);
    }

    void DialogueFinish()
    {

    }

    void DialogueOptionShowUp(List<OptionClass> opts)
    {

    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TMPText.text = HistoryText + DiaPlay.WholeText;
        TMPText.maxVisibleCharacters = HistoryText.Length + DiaPlay.MaxVisible;
        switch (gmState)
        {
            case GameManagerState.OFF:
                if(justEnter)
                {
                    justEnter = false;
                    Debug.Log("Next");
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
                                            else if (NPC[b].npc_so.npcName == evt.NPCTalking[a].MoveToClassB.Name)
                                            {
                                                NPC[b].npc_so.toDoList.Add(evt);
                                            }
                                        }
                                        EventCenter.GetInstance().EventTriggered("GM.DialoguePlay.Start", evt.NPCTalking[a].Graph);
                                    }
                                    break;
                                case DoingWithNPC.MoveTo:
                                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                                    {
                                        for (int b = 0; b < NPC.Count; b++)
                                        {
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
