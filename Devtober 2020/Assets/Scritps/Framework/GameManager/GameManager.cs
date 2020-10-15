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

public enum GameManagerState
{
    OFF,
    PROGRESSING,
    PAUSED
}

public class GameManager : SingletonBase<GameManager>
{
    [Header("Event")]
    public EventGraph eventGraph;
    public GameManagerState gmState;

    [Header("Dialogue")]
    public DialoguePlay DiaPlay;
    [SerializeField]
    TMP_Text TMPText;
    [SerializeField]
    GameObject OptionButtonPrefab;
    [SerializeField]
    List<Button> Option;
    [SerializeField]
    Transform ButtonContent;
    string HistoryText = string.Empty;

    [Header("Info Pool")]
    public List<RoomTracker> Rooms;
    public List<GameObject> NPCList;
    public List<GameObject> LastNPCList;
    RoomTracker CurrentRoom;
    public List<NpcController> NPC;

    [Header("Click")]
    public GameObject ClickAObj;
    public GameObject ClickBObj;
    public GameObject CursorOnGround;
    public LayerMask RightClickLayermask = 0;
    public LayerMask LeftClickLayermask = 0;
    public LayerMask FloorLayermask = 0;
    public LayerMask NotFloorLayermask = 0;

    [HideInInspector]
    public bool IsWaitingForMovePoint = false;
    public RightClickMenus MovePointFunction = null;

    [SerializeField]
    RectTransform RightClickMenuPanel;
    [SerializeField]
    GameObject RCButton;
    [SerializeField]
    List<GameObject> RightClickButton = new List<GameObject>();
    [SerializeField]
    RectTransform Canvas;

    [Header("NPC Panel")]
    [SerializeField]
    RectTransform NPCListPanel;
    [SerializeField]
    GameObject NPCListBtn;
    List<GameObject> NPCListButtons = new List<GameObject>();

    [Header("Camera")]
    List<Camera> cameraList = new List<Camera>();
    public Camera CurrentCamera;

    bool justEnter = true;
    DialogueGraph graph;
    Dictionary<string, bool> NPCAgentList = new Dictionary<string, bool>();

    public NavMeshSurface nav;

    // Start is called before the first frame update
    void Awake()
    {
        SetupScene();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
        EventCenter.GetInstance().AddEventListener<RoomTracker>("GM.Room.Add", RoomAdd);
        EventCenter.GetInstance().AddEventListener<DialogueGraph>("GM.DialoguePlay.Start", PlayingDialogue);
        EventCenter.GetInstance().AddEventListener<string>("GM.AllNPCArrive", NPCArrive);
        EventCenter.GetInstance().AddEventListener("DialoguePlay.PAUSED", DialoguePaused);
        EventCenter.GetInstance().AddEventListener("DialoguePlay.OFF", DialogueOFF);
        EventCenter.GetInstance().AddEventListener<int>("DialoguePlay.Next", Next);
        EventCenter.GetInstance().AddEventListener<List<OptionClass>>("DialoguePlay.OptionShowUP", DialogueOptionShowUp);
    }

    void Start()
    {
        //Add Camera
        for (int i = 0; i < Camera.allCameras.Length; i++)
        {
            cameraList.Add(Camera.allCameras[i]);
        }
        CameraSwtich("Camera 9");
    }

    //TODO 用List index
    //TODO Room Changing
    public void CameraSwtich(string camName)
    {
        for (int i = 0; i < cameraList.Count; i++)
        {
            cameraList[i].gameObject.SetActive(false);
            if (cameraList[i].gameObject.name == camName)
            {
                cameraList[i].gameObject.SetActive(true);
                CurrentCamera = cameraList[i];
            }
        }
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

    void RoomAdd(RoomTracker Room_obj)
    {
        Rooms.Add(Room_obj);
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

    // Update is called once per frame
    void Update()
    {
        nav.BuildNavMesh();
        if (Rooms != null)
        {
            if (Rooms.Count >= 1)
            {
                CurrentRoom = Rooms[0];

                NPCList = CurrentRoom.NPC();

                if(NPCList != LastNPCList)
                {
                    for (int i = 0; i < NPCListButtons.Count; i++)
                    {
                        Destroy(NPCListButtons[i]);
                    }
                    NPCListButtons.Clear();

                    for (int i = 0; i < NPCList.Count; i++)
                    {
                        GameObject obj = Instantiate(NPCListBtn);
                        NPCListButtons.Add(obj);
                        obj.transform.SetParent(NPCListPanel, false);
                        obj.name = NPCList[i].GetComponent<NpcController>().status.npcName;
                        obj.GetComponentInChildren<Text>().text = NPCList[i].GetComponent<NpcController>().status.npcName;
                    }
                    LastNPCList = NPCList;
                }
            }
        }

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
            ClearRightClickButton();
            Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity,RightClickLayermask))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);
                GameObject gameObj = hitInfo.collider.gameObject;
                List<DoorController> doorController;
                gameObj.TryGetComponent(out NpcController npcCTRL);
                //gameObj.TryGetComponent(out doorController);
                doorController = gameObj.GetComponentsInParent<DoorController>().ToList();
                Debug.Log(gameObj.name);
                if (npcCTRL != null)
                {
                    SetupRightClickMenu(npcCTRL.rightClickMenus);
                }
                else if (doorController != null)
                {
                    if (doorController.Count >= 1)
                    {
                        SetupRightClickMenu(doorController[0].rightClickMenus);
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

        if(IsWaitingForMovePoint)
        {
            Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, FloorLayermask) && !Physics.Raycast(ray, Mathf.Infinity, NotFloorLayermask))
            {
                //CursorOnGround.SetActive(true);
                if (Input.GetMouseButtonDown(0))
                {
                    MovePointFunction.DoFunction(hitInfo.point);
                    IsWaitingForMovePoint = false;
                }
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
                                            if(NPC[b].status.npcName == evt.NPCTalking[a].MoveToClassA.Name)
                                            {
                                                NPC[b].status.toDoList.Add(evt);
                                                NPCAgentList.Add(NPC[b].status.npcName, false);
                                            }
                                            else if (NPC[b].status.npcName == evt.NPCTalking[a].MoveToClassB.Name)
                                            {
                                                NPC[b].status.toDoList.Add(evt);
                                                NPCAgentList.Add(NPC[b].status.npcName, false);
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
                                            if (NPC[b].status.npcName == evt.NPCWayPoint[a].Name)
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
                        default:
                            break;
                    }
                }
            }
        }
        GoToState(GameManagerState.PAUSED);
    }
}
