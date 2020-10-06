using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvtGraph;
using DiaGraph;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay;
using UnityEngine.EventSystems;

public enum GameManagerState
{
    OFF,
    PROGRESSING,
    PAUSED
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
    public RectTransform RightClickMenuPanel;
    public RectTransform Canvas;

    bool justEnter = true;
    DialogueGraph graph;
    Dictionary<string, bool> NPCAgentList = new Dictionary<string, bool>();

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
        
        //Check is it the time to play dialogue graph
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
                NPCAgentList.Clear();
            }
        }

        //Process Event Graph
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

        //Right Click Menu
        if (Input.GetMouseButtonDown(1))
        {
            RightClickMenuPanel.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            if (Canvas.rect.width - (RightClickMenuPanel.position.x + RightClickMenuPanel.rect.width) >= 0)
            {
                RightClickMenuPanel.pivot = new Vector2(0, RightClickMenuPanel.pivot.y);
            }
            else
            {
                RightClickMenuPanel.pivot = new Vector2(1, RightClickMenuPanel.pivot.y);
            }

            if (Canvas.rect.height - ((Canvas.rect.height - RightClickMenuPanel.position.y) + RightClickMenuPanel.rect.height) >= 0)
            {
                RightClickMenuPanel.pivot = new Vector2(RightClickMenuPanel.pivot.x, 1);
            }
            else
            {
                RightClickMenuPanel.pivot = new Vector2(RightClickMenuPanel.pivot.x, 0);
            }
            RightClickMenuPanel.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            Debug.Log(RightClickMenuPanel.position.y);
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
