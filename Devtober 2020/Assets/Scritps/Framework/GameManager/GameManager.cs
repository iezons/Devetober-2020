using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvtGraph;
using DiaGraph;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay;

public enum GameManagerState
{
    OFF,
    PROGRESSING,
    PAUSED
}

public delegate void MenuHandler(object obj);

public class TestRightClick
{
    public string FunctionName;
    public event MenuHandler Function;
    public void DoFunction(object obj)
    {
        Function(obj);
    }
}

public class GameManager : SingletonBase<GameManager>
{
    public EventGraph eventGraph;
    public DialoguePlay DiaPlay;
    public List<RoomTracker> Rooms;
    public List<NpcController> NPC;
    public GameManagerState gmState;
    public string HistoryText;
    public TMP_Text TMPText;
    public GameObject OptionButtonPrefab;
    public List<Button> Option;
    public Transform ButtonContent;

    public event MenuHandler ATT;

    bool justEnter = true;
    DialogueGraph graph;
    Dictionary<string, bool> NPCAgentList = new Dictionary<string, bool>();

    List<TestRightClick> RC = new List<TestRightClick>();

    // Start is called before the first frame update
    void Awake()
    {
        SetupScene();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
        EventCenter.GetInstance().AddEventListener<DialogueGraph>("GM.DialoguePlay.Start", PlayingDialogue);
        EventCenter.GetInstance().AddEventListener<string>("GM.AllNPCArrive", NPCArrive);
        EventCenter.GetInstance().AddEventListener("DialoguePlay.PAUSED", DialoguePaused);
        EventCenter.GetInstance().AddEventListener("DialoguePlay.OFF", DialogueOFF);
        EventCenter.GetInstance().AddEventListener<int>("DialoguePlay.Next", Next);
        EventCenter.GetInstance().AddEventListener<List<OptionClass>>("DialoguePlay.OptionShowUP", DialogueOptionShowUp);
        
    }

    public void AddMenu(string name, MenuHandler action)
    {
        RC.Add(new TestRightClick { FunctionName = name});
        RC[RC.Count - 1].Function += action;
    }

    void TurnON(object obj)
    {

    }

    void Next(int index)
    {
        for (int i = 0; i < Option.Count; i++)
        {
            Destroy(Option[i].gameObject);
        }
        Option.Clear();
    }

    void NPCArrive(string NPCName)
    {
        if(NPCAgentList.ContainsKey(NPCName))
        {
            NPCAgentList[NPCName] = true;
        }
    }

    void NPCAdd(NpcController NPC_obj)
    {
        NPC.Add(NPC_obj);
    }

    void PlayingDialogue(DialogueGraph graph)
    {
        if (DiaPlay.d_state == DiaState.OFF)
            EventCenter.GetInstance().EventTriggered("DialoguePlay.Start", graph);
    }

    void DialoguePaused()
    {
        if(DiaPlay.n_state == NodeState.Dialogue && DiaPlay.d_state != DiaState.OFF)
            StartCoroutine(WaitAndPlay());
        else if (DiaPlay.n_state == NodeState.Option)
        {

        }
    }

    IEnumerator WaitAndPlay()
    {
        yield return new WaitForSeconds(0.7f);
        HistoryText += DiaPlay.WholeText + System.Environment.NewLine;
        EventCenter.GetInstance().EventTriggered("DialoguePlay.Next", 0);
    }

    void DialogueOFF()
    {
        //HistoryText += DiaPlay.WholeText + System.Environment.NewLine;
    }

    void DialogueOptionShowUp(List<OptionClass> opts)
    {
        for (int i = 0; i < Option.Count; i++)
        {
            Destroy(Option[i].gameObject);
        }
        Option.Clear();
        for (int i = 0; i < opts.Count; i++)
        {
            Option.Add(Instantiate(OptionButtonPrefab).GetComponent<Button>());
            Option[i].transform.SetParent(ButtonContent, false);
            Option[i].transform.GetComponentInChildren<Text>().text = opts[i].Text;
            Option[i].transform.name = i.ToString();
        }
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(UpdateText());
        
        if(graph != null)
        {
            bool tempBool = false;
            foreach (bool value in NPCAgentList.Values)
            {
                if(value == false)
                {
                    tempBool = false;
                    break;
                }
                else
                {
                    tempBool = true;
                }
            }
            if(tempBool)
            {
                EventCenter.GetInstance().EventTriggered("GM.DialoguePlay.Start", graph);
                graph = null;
            }
        }

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

    IEnumerator UpdateText()
    {
        TMPText.maxVisibleCharacters = HistoryText.Length + DiaPlay.MaxVisible;
        yield return new WaitForEndOfFrame();
        TMPText.text = HistoryText + DiaPlay.WholeText;
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
                                                NPCAgentList.Add(NPC[b].npc_so.npcName, false);
                                            }
                                            else if (NPC[b].npc_so.npcName == evt.NPCTalking[a].MoveToClassB.Name)
                                            {
                                                NPC[b].npc_so.toDoList.Add(evt);
                                                NPCAgentList.Add(NPC[b].npc_so.npcName, false);
                                            }
                                        }
                                        graph = evt.NPCTalking[a].Graph;
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
