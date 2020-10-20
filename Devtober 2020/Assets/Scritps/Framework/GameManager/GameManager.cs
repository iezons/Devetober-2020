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
using System;
using System.Linq;
using UnityEngine.AI;

public class DefaultValueWithGO
{
    public object DefaultValue;
    public GameObject GO;
}

public class WaitingNPCArrive
{
    public string RoomNum;
    public List<GameObject> NPC = new List<GameObject>();
    public DialogueGraph graph;
}

public enum GameManagerState
{
    OFF,
    PROGRESSING,
    PAUSED
}

public class GameManager : SingletonBase<GameManager>
{
    [Header("Event")]
    public EventGraphScene eventGraph;
    //public EventGraph eventGraph;
    public GameManagerState gmState;

    [Header("Dialogue")]
    public DialogueGraph TestGraph;
    [SerializeField]
    TMP_Text TMPText = null;
    [SerializeField]
    GameObject OptionButtonPrefab = null;
    [SerializeField]
    List<Button> Option = null;
    [SerializeField]
    Transform ButtonContent = null;
    //string HistoryText = string.Empty;

    [Header("Info Pool")]
    public List<RoomTracker> Rooms;
    public RoomTracker CurrentRoom;
    public List<NpcController> NPC;

    [Header("Click")]
    public RightClickMenus RightClickMs;
    public bool IsWaitingForClickObj = false;

    public LayerMask RightClickLayermask = 0;
    public LayerMask LeftClickLayermask = 0;
    public LayerMask FloorLayermask = 0;
    public LayerMask NotFloorLayermask = 0;

    [SerializeField]
    RectTransform RightClickMenuPanel = null;
    [SerializeField]
    GameObject RCButton = null;
    [SerializeField]
    List<GameObject> RightClickButton = new List<GameObject>();
    [SerializeField]
    RectTransform Canvas = null;

    [Header("NPC Panel")]
    [SerializeField]
    RectTransform NPCListPanel = null;
    [SerializeField]
    GameObject NPCListBtn = null;
    List<GameObject> NPCListButtons = new List<GameObject>();

    bool justEnter = true;
    //DialogueGraph graph;
    //Dictionary<string, bool> NPCAgentList = new Dictionary<string, bool>();

    //public NavMeshSurface nav;

    // Start is called before the first frame update
    void Awake()
    {
        SetupScene();
        eventGraph = GetComponent<EventGraphScene>();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
        EventCenter.GetInstance().AddEventListener<RoomTracker>("GM.Room.Add", RoomAdd);
    }

    void Start()
    {
        RoomSwitch("Room 9");
    }

    public void RoomSwitch(string roomName)
    {
        for (int i = 0; i < Rooms.Count; i++)
        {
            Rooms[i].RoomCamera.gameObject.SetActive(false);
            if (Rooms[i].gameObject.name == roomName)
            {
                Rooms[i].RoomCamera.gameObject.SetActive(true);
                CurrentRoom = Rooms[i];
            }
        }
        SetupOption();
    }

    public void OptionSelect(int index)
    {
        for (int i = 0; i < Option.Count; i++)
        {
            Destroy(Option[i].gameObject);
        }
        Option.Clear();
        CurrentRoom.OptionSelect(index);
    }

    public void SetupOption()
    {
        for (int i = 0; i < Option.Count; i++)
        {
            Destroy(Option[i].gameObject);
        }
        Option.Clear();
        for (int i = 0; i < CurrentRoom.OptionList.Count; i++)
        {
            Option.Add(Instantiate(OptionButtonPrefab).GetComponent<Button>());
            Option[i].transform.SetParent(ButtonContent, false);
            Option[i].transform.GetComponentInChildren<Text>().text = CurrentRoom.OptionList[i].Text;
            Option[i].transform.name = i.ToString();
        }
    }

    void NPCAdd(NpcController NPC_obj)
    {
        NPC.Add(NPC_obj);
    }

    void RoomAdd(RoomTracker Room_obj)
    {
        Rooms.Add(Room_obj);
    }

    IEnumerator UpdateText()
    {
        TMPText.maxVisibleCharacters = CurrentRoom.HistoryText.Length + CurrentRoom.DiaPlay.MaxVisible;
        yield return null;
        TMPText.text = CurrentRoom.HistoryText + CurrentRoom.DiaPlay.WholeText;
    }

    void InstanceNPCListBtn(string npcName)
    {
        GameObject obj = Instantiate(NPCListBtn);
        NPCListButtons.Add(obj);
        obj.transform.SetParent(NPCListPanel, false);
        obj.name = npcName;
        obj.GetComponentInChildren<Text>().text = npcName;
    }

    void UpdateNPCList()
    {
        List<GameObject> tempObjs = CurrentRoom.NPC();
        List<int> RemoveIndex = new List<int>();

        //Check is there a npc enter the room
        foreach (var temp in tempObjs)
        {
            string NPCName = temp.GetComponent<NpcController>().status.npcName;
            bool IsContains = false;
            for (int i = 0; i < NPCListButtons.Count; i++)
            {
                if(NPCListButtons[i].name == NPCName)
                {
                    IsContains = true;
                    break;
                }
            }
            if(!IsContains)
            {
                InstanceNPCListBtn(NPCName);
            }
        }

        //Check is there a npc leave the room
        for (int a = 0; a < NPCListButtons.Count; a++)
        {
            bool IsContains = false;
            for (int i = 0; i < tempObjs.Count; i++)
            {
                if (NPCListButtons[a].name == tempObjs[i].GetComponent<NpcController>().status.npcName)
                {
                    IsContains = true;
                    break;
                }
            }

            if(!IsContains)
            {
                RemoveIndex.Add(a);
            }
        }

        for (int i = 0; i < RemoveIndex.Count; i++)
        {
            GameObject gameObj = NPCListButtons[RemoveIndex[i]];
            NPCListButtons.RemoveAt(RemoveIndex[i]);
            Destroy(gameObj);
        }

        RemoveIndex.Clear();
    }

    void Update()
    {
        //Test Code
        if(Input.GetKeyDown(KeyCode.Y))
        {
            CurrentRoom.PlayingDialogue(TestGraph);
        }

        //NavMesh Building
        if(Time.frameCount % 10 == 0)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (Rooms[i].isEnemyDetected() && Rooms[i].NPC().Count > 0)
                {
                    Debug.Log("Build");
                    Rooms[i].navSurface.BuildNavMesh();
                }
            }
        }

        StartCoroutine(UpdateText());

        //----------------------
        if (CurrentRoom != null)
        {
            UpdateNPCList();
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
        if (Input.GetMouseButtonDown(1) && !IsWaitingForClickObj)
        {
            ClearRightClickButton();
            Ray ray = CurrentRoom.RoomCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity,RightClickLayermask))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);
                GameObject gameObj = hitInfo.collider.gameObject;
                gameObj.TryGetComponent(out ControllerBased based);
                if(based != null)
                {
                    if (based.rightClickMenus != null)
                    {
                        if(based.rightClickMenus.Count >= 0)
                        {
                            SetupRightClickMenu(based.rightClickMenus);
                        }
                    }
                }
                else
                {
                    ControllerBased[] baseds = gameObj.GetComponentsInParent<ControllerBased>();
                    if(baseds != null)
                    {
                        if(baseds.Count() >= 1)
                        {
                            if (baseds[0].rightClickMenus != null)
                            {
                                if (baseds[0].rightClickMenus.Count >= 0)
                                {
                                    SetupRightClickMenu(baseds[0].rightClickMenus);
                                }
                            }
                        }
                    }
                }
            }
            //UI 位置适配
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
        }

        if (Input.GetMouseButtonDown(0))
        {
            bool ClickOnMenu = false;
            if (RightClickButton != null)
            {
                foreach (var item in RightClickButton)
                {
                    if (EventSystem.current.currentSelectedGameObject == item)
                    {
                        ClickOnMenu = true;
                        break;
                    }
                }
            }

            if (!ClickOnMenu)
            {
                RightClickMenuPanel.gameObject.SetActive(false);
            }
        }

        if(IsWaitingForClickObj)
        {
            //ChangeWaitingCursor
            Ray ray = CurrentRoom.RoomCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, RightClickMs.InteractLayer))
            {
                Debug.Log(hitInfo.collider.name);
                Debug.DrawLine(ray.origin, hitInfo.point);
                //HighLight
                //ChangeDefaultCursor
                if (Input.GetMouseButtonDown(0))
                {
                    if(RightClickMs.DefaultCallValue != null)
                    {
                        DefaultValueWithGO dwg = new DefaultValueWithGO
                        {
                            DefaultValue = RightClickMs.DefaultCallValue,
                            GO = hitInfo.collider.gameObject,
                        };
                        RightClickMs.DoFunction(dwg);
                    }
                    else
                    {
                        RightClickMs.DoFunction(hitInfo.collider.gameObject);
                    }
                    IsWaitingForClickObj = false;
                }
            }

            if(Input.anyKeyDown && !Input.GetMouseButtonDown(0))
            {
                //ChangeDefaultCursor
                IsWaitingForClickObj = false;
            }
            //CursorOnGround.transform.position = hitInfo.point;
        }
        else
        {
            //CursorOnGround.SetActive(false);
        }
    }

    void SetupRightClickMenu(List<RightClickMenus> menus)
    {
        RightClickMenuPanel.gameObject.SetActive(true);
        for (int i = 0; i < menus.Count; i++)
        {
            RightClickButton.Add(Instantiate(RCButton));
            GameObject obj = RightClickButton[RightClickButton.Count - 1];
            obj.transform.SetParent(RightClickMenuPanel, false);
            obj.GetComponent<RightClickButtonSC>().menu = menus[i];
            obj.GetComponent<RightClickButtonSC>().AfterInstantiate();
        }
    }

    void ClearRightClickButton()
    {
        for (int i = 0; i < RightClickButton.Count; i++)
        {
            Destroy(RightClickButton[i].gameObject);
        }
        RightClickButton.Clear();
        RightClickMs = null;
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
                eventGraph.graph.SetNode();
                eventGraph.graph.Next();
                TriggerEvent();
                break;
            case GameManagerState.PROGRESSING:
                break;
            case GameManagerState.PAUSED:
                eventGraph.graph.Next();
                TriggerEvent();
                break;
            default:
                break;
        }
    }

    void TriggerEvent()
    {
        GoToState(GameManagerState.PROGRESSING);
        EventNode cur =  eventGraph.graph.current as EventNode;
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
                                        for (int b = 0; b < evt.NPCTalking[a].moveToClasses.Count; b++)
                                        {
                                            for (int c = 0; c < NPC.Count; c++)
                                            {
                                                if(evt.NPCTalking[a].moveToClasses[b].NPC.gameObject == NPC[c])
                                                {
                                                    NPC[c].status.toDoList.Add(evt);
                                                    for (int t = 0; t < Rooms.Count; t++)
                                                    {
                                                        if(Rooms[t].NPC().Contains(NPC[c].gameObject))
                                                        {
                                                            Rooms[t].NPCAgentList.Add(NPC[c].status.npcName, false);
                                                            Rooms[t].WaitingGraph = evt.NPCTalking[a].Graph;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case DoingWithNPC.MoveTo:
                                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                                    {
                                        for (int b = 0; b < NPC.Count; b++)
                                        {
                                            if (NPC[b].gameObject == evt.NPCWayPoint[a].NPC)
                                            {
                                                NPC[b].status.toDoList.Add(evt);
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
                        case DoingWith.Custom:
                            for (int a = 0; a < evt.CustomCode.Count; a++)
                            {
                                evt.CustomCode[a].DoEvent(null);
                            }
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