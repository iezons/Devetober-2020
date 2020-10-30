using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvtGraph;
using DiaGraph;
using TMPro;
using UnityEngine.UI;
using GamePlay;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;
using UnityEngine.AI;
using GraphBase;

public class DefaultValueWithGO
{
    public object DefaultValue;
    public GameObject GO;
}

public class WaitingNPCArrive
{
    public RoomTracker RoomNum;
    public List<GameObject> NPC = new List<GameObject>();
    public DialogueGraph graph;
}

[Serializable]
public class DirectorsClass
{
    public PlayableDirector Director;
    public bool IsPlaying;
}

public enum GameManagerState
{
    OFF,
    CONDITIONING,
    PROGRESSING,
    PAUSED
}

public class GameManager : SingletonBase<GameManager>
{
    [Header("navMesh")]
    public NavMeshSurface surface;
    public LocalNavMeshBuilder builder;

    [Header("Input")]
    public string MoveLeft;
    public string MoveRight;

    [Header("Event")]
    public EventGraphScene eventGraph;
    public GameManagerState gmState;
    bool justEnter = true;

    [Header("EventTriggerCache")]
    List<EventScriptInterface> EventScripts = new List<EventScriptInterface>();
    public EventNode TriggeringEventNode = null;
    bool justEnterEventTrigger = true;

    [Header("EventConditionalCache")]
    public List<EvtGraph.EventTrigger> WaitingEvent = new List<EvtGraph.EventTrigger>();
    Dictionary<string, List<CustomCondition>> customConditions = new Dictionary<string, List<CustomCondition>>();
    List<EventNode> ConditionalWaitingNode = new List<EventNode>();
    bool justEnterCondition = true;

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

    [Header("Info Pool")]
    public List<RoomTracker> Rooms;
    public RoomTracker CurrentRoom;
    public List<NpcController> NPC;
    public List<EnemyController> Enemy;

    [Header("Click")]
    public RectTransform Screen;
    public RightClickMenus RightClickMs;
    public bool IsWaitingForClickObj = false;
    public ControllerBased LastCB = null;
    public LayerMask RightClickLayermask = 0;
    public Texture2D DefaultCursor;
    public Texture2D InteractCursor;
    public TMP_Text InteractText;
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

    [Header("Camera")]
    public Camera MainCamera;
    public GameObject CameraScreen;
    public Texture VHSTexture;
    public Transform CameraButtonListPanel;
    public GameObject CameraButton;
    public TMP_Text CameraName;
    public TMP_Text TimeText;
    [Tooltip("x: hours  y: minutes z: seconds")]
    public Vector3 StartTime;
    public bool CanCameraTurnLeft = true;
    public bool CanCameraTurnRight= true;
    float currentSeconds;

    [Header("Timeline Playing")]
    public List<DirectorsClass> Directors = new List<DirectorsClass>();

    //public PlayableDirector Director;
    //public TimelineAsset timeline;

    [Header("Game Stages")]
    public int Stage = 0; // 0-Tutorial 1-Stage01 2-Stage-02

    [Header("Room group")]
    public GameObject TutorialLevel;
    public GameObject MainLevelGroup;

    [Header("Zombie")]
    public GameObject ZombieObject;
    public NpcController testnpc;

    [Header("Audio")]
    public AudioSource Audio2D;
    bool AllowAudio = false;

    public void SetCurEventNode(string name, EventSO so)
    {
        EventNode evt = eventGraph.graph.currentList[0] as EventNode;
        evt.eventSO[0] = so;
    }

    void Awake()
    {
        AudioMgr.GetInstance();
        SetupScene();
        eventGraph = GetComponent<EventGraphScene>();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
        EventCenter.GetInstance().AddEventListener<RoomTracker>("GM.Room.Add", RoomAdd);
        EventCenter.GetInstance().AddEventListener<EnemyController>("GM.Enemy.Add", EnemyAdd);
        RightClickMenuPanel.gameObject.SetActive(false);
        currentSeconds = StartTime.x * 3600 + StartTime.y * 60 + StartTime.z;
    }

    void Start()
    {
        SetupStage(Stage);
        if (Stage == 0)
            RoomSwitch("A7 Server Room", 0);
        else if (Stage > 0)
            RoomSwitch("BedRoom_A", 0);
        else
            RoomSwitch("TestRoom", 0);
        AllowAudio = true;
        StartCoroutine(BuildMesh());
    }

    public void RoomSwitch(string RoomName, int CameraIndex)
    {
        for (int i = 0; i < Rooms.Count; i++)
        {
            for (int a = 0; a < Rooms[i].cameraLists.Count; a++)
            {
                Rooms[i].cameraLists[a].roomCamera.gameObject.SetActive(false);
            }
            
            if (Rooms[i].gameObject.name == RoomName)
            {
                foreach (var item in NPCListButtons)
                {
                    Destroy(item);
                }
                NPCListButtons.Clear();
                Rooms[i].cameraLists[CameraIndex].roomCamera.gameObject.SetActive(true);
                if(Rooms[i].cameraLists.Count > 1)
                {
                    CameraName.text = Rooms[i].RoomName() + " " + (CameraIndex + 1).ToString();
                }
                else
                {
                    CameraName.text = Rooms[i].RoomName();
                }
                if(AllowAudio)
                    AudioMgr.GetInstance().PlayAudio(Audio2D, "camera_switch", 0.3f, false, null);
                SetupOption(Rooms[i]);
                Rooms[i].CurrentCameraIndex = CameraIndex;
                CurrentRoom = Rooms[i];
            }
        }
    }

    void SetupCameraButton()
    {
        for (int i = 0; i < CameraButtonListPanel.childCount; i++)
        {
            Destroy(CameraButtonListPanel.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < Rooms.Count; i++)
        {
            if(Rooms[i].CanBeDetected)
            {
                for (int a = 0; a < Rooms[i].cameraLists.Count; a++)
                {
                    string RoomName = Rooms[i].RoomName();
                    int index = a;

                    GameObject gameobj = Instantiate(CameraButton);

                    gameobj.transform.SetParent(CameraButtonListPanel, false);
                    if(Rooms[i].cameraLists.Count > 1)
                        gameobj.GetComponentInChildren<TMP_Text>().text = RoomName + " " + (index + 1).ToString();
                    else
                        gameobj.GetComponentInChildren<TMP_Text>().text = RoomName;
                    SwitchCameraButton SCB = gameobj.GetComponent<SwitchCameraButton>();
                    SCB.RoomName = RoomName;
                    SCB.CameraIndex = index;
                }
            }
        }
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

    public void SetupOption(RoomTracker room)
    {
        for (int i = 0; i < Option.Count; i++)
        {
            Destroy(Option[i].gameObject);
        }
        Option.Clear();
        if (room.DiaPlay.n_state == NodeState.Option)
        {
            for (int i = 0; i < room.OptionList.Count; i++)
            {
                Option.Add(Instantiate(OptionButtonPrefab).GetComponent<Button>());
                Option[i].transform.SetParent(ButtonContent, false);
                Option[i].transform.GetComponentInChildren<Text>().text = room.OptionList[i].Text;
                Option[i].transform.name = i.ToString();
            }
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

    void EnemyAdd(EnemyController Enemy_obj)
    {
        Enemy.Add(Enemy_obj);
    }

    IEnumerator UpdateText()
    {
        TMPText.maxVisibleCharacters = CurrentRoom.WordCound + CurrentRoom.DiaPlay.MaxVisible;
        yield return null;
        TMPText.text = CurrentRoom.HistoryText + CurrentRoom.DiaPlay.WholeText;
    }

    void InstanceNPCListBtn(string npcName, NpcController npc)
    {
        GameObject obj = Instantiate(NPCListBtn);
        NPCListButtons.Add(obj);
        obj.transform.SetParent(NPCListPanel, false);
        obj.name = npcName;
        obj.GetComponent<NPCListCTRL>().npc = npc;
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
                InstanceNPCListBtn(NPCName, temp.GetComponent<NpcController>());
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

    public void EventForceNext()
    {
        GoToState(GameManagerState.PAUSED);
    }

    IEnumerator BuildMesh()
    {
        //while(true)
        //{
        //    for (int i = 0; i < Rooms.Count; i++)
        //    {
        //        if (Rooms[i].isEnemyDetected() && Rooms[i].NPC().Count > 0)
        //        {
        //            break;
        //        }
        //    }
            yield return new WaitForSeconds(0.5f);
        //}
    }

    void Update()
    {
        #region TimeLine
        TimelineStop();
        #endregion

        #region Input
        if (Input.GetKey(MoveLeft) && CanCameraTurnLeft)
        {
            CurrentRoom.Rotate(-1, CurrentRoom.CurrentCameraIndex);
        }
        else if (Input.GetKey(MoveRight) && CanCameraTurnRight)
        {
            CurrentRoom.Rotate(1, CurrentRoom.CurrentCameraIndex);
        }
        #endregion

        #region Test Code
        //Test Code
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CurrentRoom.PlayingDialogue(TestGraph);
        }

        if(Input.GetKeyDown(KeyCode.U))
        {
            EventCenter.GetInstance().DiaEventTrigger("01_CloseDoor");
            EventCenter.GetInstance().DiaEventTrigger("TU_TurnLeftCheck");
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            EventCenter.GetInstance().DiaEventTrigger("01_PrisonerSafe");
            EventCenter.GetInstance().DiaEventTrigger("TU_TurnRightCheck");
        }

        if(Input.GetKeyDown(KeyCode.L))
        {
            Resolution[] res = UnityEngine.Screen.resolutions;
            if (res.Length > 0)
            {
                if (res[0].width >= res[1].height)
                {
                    UnityEngine.Screen.SetResolution(res[0].width, res[0].width / 16 * 9, false);
                    Debug.Log(res[0].width.ToString() + "x" + (res[0].width / 16 * 9).ToString());
                }
                else
                {
                    UnityEngine.Screen.SetResolution(res[0].height / 9 * 16, res[0].height, false);
                    Debug.Log((res[0].height / 9 * 16).ToString() + "x" + res[0].height.ToString());
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.K))
        {
            Resolution[] res = UnityEngine.Screen.resolutions;
            if (res.Length > 0)
            {
                if (res[0].width >= res[1].height)
                {
                    UnityEngine.Screen.SetResolution(res[0].width, res[0].width / 16 * 9, true);
                }
                else
                {
                    UnityEngine.Screen.SetResolution(res[0].height / 9 * 16, res[0].height, true);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.F1))
        {
            testnpc.SwitchAnimState(false);
        }
        #endregion

        #region NavMeshBuilding
        //NavMesh Building
        //if (Time.frameCount % 10 == 0)
        //{
        //    for (int i = 0; i < Rooms.Count; i++)
        //    {
        //        if (Rooms[i].isEnemyDetected() && Rooms[i].NPC().Count > 0)
        //        {
        //            for (int a = 0; a < Rooms[i].navSurface.Count; a++)
        //            {
        //                Rooms[i].navSurface[a].BuildNavMesh();
        //            }
        //        }
        //    }
        //}
        if (Time.frameCount % 10 == 0)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (Rooms[i].isEnemyDetected() && Rooms[i].NPC().Count > 0)
                {
                    surface.BuildNavMesh();
                    break;
                }
            }
        }
        #endregion

        #region UpdateText
        StartCoroutine(UpdateText());
        #endregion

        #region UpdateNPCList
        if (CurrentRoom != null)
        {
            UpdateNPCList();
        }
        #endregion

        #region Process Event Graph
        switch (gmState)
        {
            case GameManagerState.OFF:
                if (justEnter)
                {
                    justEnter = false;
                    Next();
                }
                break;
            case GameManagerState.CONDITIONING:
                if (justEnter)
                {
                    for (int i = 0; i < eventGraph.graph.currentList.Count; i++)
                    {
                        EventNode evt = eventGraph.graph.currentList[i] as EventNode;
                        if (evt != null)
                        {
                            ConditionalWaitingNode.Add(evt);
                        }
                    }
                    justEnter = false;
                }

                for (int i = 0; i < ConditionalWaitingNode.Count; i++)
                {
                    if(Conditioning(ConditionalWaitingNode[i]))
                    {
                        TriggeringEventNode = ConditionalWaitingNode[i];
                        GoToState(GameManagerState.PROGRESSING);
                        break;
                    }
                }
                justEnterCondition = false;
                break;
            case GameManagerState.PROGRESSING:
                if (justEnter)
                {
                    justEnter = false;
                    EventDistribute();
                }
                if(IsEventFinish())
                {
                    GoToState(GameManagerState.PAUSED);
                }
                break;
            case GameManagerState.PAUSED:
                if (justEnter)
                {
                    justEnter = false;
                    Next();
                }
                break;
            default:
                break;
        }
        #endregion

        #region Mouse Clicking
        //Mouse Position Correction
        //Vector3 MousePos = new Vector3(Input.mousePosition.x * (VHSTexture.width / 1920f), Input.mousePosition.y * (VHSTexture.height / 1080f), Input.mousePosition.z);
        Vector3 MousePos = Input.mousePosition * (Canvas.rect.width / UnityEngine.Screen.width);
        
        Ray ray_MainCamera = MainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray_MainCamera, out RaycastHit hit_MainCamera))
        {
            MousePos = hit_MainCamera.textureCoord;
        }

        LayerMask DoWithLayer;
        if(IsWaitingForClickObj)
        {
            DoWithLayer = RightClickMs.InteractLayer;
        }
        else
        {
            DoWithLayer = RightClickLayermask;
        }

        bool IsSetCursor = false;

        if(Input.GetMouseButtonDown(0))
        {
            if (NPCListButtons.Contains(EventSystem.current.currentSelectedGameObject))
            {
                Debug.Log("899999999");
                //ControllerBased cbase = EventSystem.current.currentSelectedGameObject.GetComponent<NPCListCTRL>().npc;
                //if (cbase != null)
                //{
                //    if (cbase.rightClickMenus.Count > 0)
                //    {
                //        if (cbase.IsInteracting)
                //        {
                //            SetupRightClickMenu(cbase.rightClickMenus);
                //        }
                //    }
                }
            }

        //Right Click Menu
        if (Input.GetMouseButtonDown(1) && !IsWaitingForClickObj)
        {
            ClearRightClickButton();
            //UI
            if (NPCListButtons.Contains(EventSystem.current.currentSelectedGameObject))
            {
                ControllerBased cbase = EventSystem.current.currentSelectedGameObject.GetComponent<NPCListCTRL>().npc;
                if(cbase != null)
                {
                    if(cbase.rightClickMenus.Count > 0)
                    {
                        if(cbase.IsInteracting)
                        {
                            SetupRightClickMenu(cbase.rightClickMenus);
                        }
                    }
                }
            }
            //Raycast
            Ray ray = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ViewportPointToRay(MousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity,RightClickLayermask))
            {
                if(CurrentRoom.hitInformation.Contains(hitInfo.collider))
                {
                    Debug.DrawLine(ray.origin, hitInfo.point);
                    GameObject gameObj = hitInfo.collider.gameObject;
                    gameObj.TryGetComponent(out ControllerBased based);
                    if (based != null)
                    {
                        if (based.rightClickMenus != null)
                        {
                            if (based.rightClickMenus.Count > 0)
                            {
                                if (!based.IsInteracting)
                                {
                                    SetupRightClickMenu(based.rightClickMenus);
                                }
                            }
                        }
                    }
                    else
                    {
                        ControllerBased[] baseds = gameObj.GetComponentsInParent<ControllerBased>();
                        if (baseds != null)
                        {
                            if (baseds.Count() >= 1)
                            {
                                if (baseds[0].rightClickMenus != null)
                                {
                                    if (baseds[0].rightClickMenus.Count >= 0)
                                    {
                                        if (!based.IsInteracting)
                                        {
                                            SetupRightClickMenu(baseds[0].rightClickMenus);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Vector3 mousepos = Input.mousePosition * (Canvas.rect.width / UnityEngine.Screen.width);
                float RCx = mousepos.x - Canvas.rect.width / 2;
                float RCy = mousepos.y - Canvas.rect.height / 2 - RightClickMenuPanel.rect.height;
                if (RCx + RightClickMenuPanel.rect.width > Canvas.rect.width - Canvas.rect.width / 2)//Out of right bounds
                {
                    RCx -= RightClickMenuPanel.rect.width;
                }
                else if (RCy - RightClickMenuPanel.rect.height < Canvas.rect.height - Canvas.rect.height / 2)
                {
                    RCy += RightClickMenuPanel.rect.height;
                }
                RightClickMenuPanel.localPosition = new Vector3(RCx, RCy, 0);
            }
            else
            {
                RightClickMenuPanel.gameObject.SetActive(false);
            }
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

        //HighLight
        Ray ray_outline = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ViewportPointToRay(MousePos);
        if (Physics.Raycast(ray_outline, out RaycastHit hitInfomation, Mathf.Infinity, DoWithLayer))
        {
            if(CurrentRoom.hitInformation.Contains(hitInfomation.collider))
            {
                Debug.DrawLine(ray_outline.origin, hitInfomation.point);
                IsSetCursor = true;
                ControllerBased curCB = hitInfomation.collider.GetComponent<ControllerBased>();
                if (LastCB != null && curCB != null)
                {
                    if (curCB != LastCB)
                    {
                        LastCB.SetOutline(false);
                        if (curCB.gameObject.layer != LayerMask.NameToLayer("Room") && !curCB.IsInteracting)
                        {
                            Cursor.SetCursor(InteractCursor, new Vector2(0, 0), CursorMode.Auto);
                            IsSetCursor = true;
                            curCB.SetOutline(true);
                        }
                        LastCB = curCB;
                    }
                }
                else
                {
                    if (curCB != null)
                    {
                        if (curCB.gameObject.layer != LayerMask.NameToLayer("Room") && !curCB.IsInteracting)
                        {
                            Cursor.SetCursor(InteractCursor, new Vector2(0, 0), CursorMode.Auto);
                            IsSetCursor = true;
                            curCB.SetOutline(true);
                        }
                        LastCB = curCB;
                    }
                }
            }
        }
        else
        {
            if(LastCB != null)
            {
                LastCB.SetOutline(false);
                LastCB = null;
            }
        }
        
        if(!IsSetCursor)
        {
            Cursor.SetCursor(DefaultCursor, new Vector2(0, 0), CursorMode.Auto);
        }

        if(IsWaitingForClickObj)
        {
            InteractText.gameObject.SetActive(true);
            InteractText.text = "Waiting for: <color=#00FF00>" + RightClickMs.functionName + "</color>";
            //ChangeWaitingCursor
            Ray ray = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ViewportPointToRay(MousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, RightClickMs.InteractLayer))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);

                //ChangeDefaultCursor
                hitInfo.collider.TryGetComponent(out NpcController Npcc);
                if(Npcc != null)
                {
                    if (Input.GetMouseButtonDown(0) && CurrentRoom.hitInformation.Contains(hitInfo.collider) && !hitInfo.collider.GetComponent<ControllerBased>().IsInteracting && !Npcc.IsPrisoner)
                    {
                        if (RightClickMs.DefaultCallValue != null)
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
                else
                {
                    if (Input.GetMouseButtonDown(0) && CurrentRoom.hitInformation.Contains(hitInfo.collider) && !hitInfo.collider.GetComponent<ControllerBased>().IsInteracting)
                    {
                        if (RightClickMs.DefaultCallValue != null)
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
                
            }

            if(Input.anyKeyDown && !Input.GetKeyDown(MoveLeft) && !Input.GetKeyDown(MoveRight) && !Input.GetMouseButtonDown(0))
            {
                Debug.Log("Waiting disable");
                //ChangeDefaultCursor
                IsWaitingForClickObj = false;
            }
        }
        else
        {
            InteractText.gameObject.SetActive(false);
        }
        #endregion

        #region DisplayTimeText
        if (currentSeconds + Time.deltaTime >= 86400)
        {
            currentSeconds -= 86400;
        }
        currentSeconds += Time.deltaTime;
        int SecondTime = (int)Mathf.Floor(currentSeconds);
        int Hours = (int)Mathf.Floor(SecondTime / 3600);
        int Minutes = (int)Mathf.Floor((SecondTime - Hours * 3600) / 60);
        int Seconds = (int)Mathf.Floor((SecondTime - Hours * 3600 - Minutes * 60));
        string H = ReverseString((ReverseString(Hours.ToString()) + "00").Substring(0, 2));
        string M = ReverseString((ReverseString(Minutes.ToString()) + "00").Substring(0, 2));
        string S = ReverseString((ReverseString(Seconds.ToString()) + "00").Substring(0, 2));
        TimeText.text = H + ":" + M + ":" + S;
        #endregion
    }

    string ReverseString(string str)
    {
        char[] cha = str.ToCharArray();
        Array.Reverse(cha);
        return new string(cha);
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
        Resolution[] res = UnityEngine.Screen.resolutions;
        if(res.Length > 0)
        {
            if (res[0].width >= res[1].height)
            {
                UnityEngine.Screen.SetResolution(res[0].width, res[0].width / 16 * 9, false);
            }
            else
            {
                UnityEngine.Screen.SetResolution(res[0].height / 9 * 16, res[0].height, false);
            }
        }
    }

    public void SetupStage(int stage = 2)
    {
        Stage = stage;
        if(stage == 0)
        {
            CanCameraTurnLeft = false;
            CanCameraTurnRight = false;
            CameraScreen.SetActive(false);
            NPCListPanel.gameObject.SetActive(false);
            CameraButtonListPanel.gameObject.SetActive(false);
            CameraName.gameObject.SetActive(false);
            MainLevelGroup.SetActive(false);
            TutorialLevel.SetActive(true);
            RoomSwitch("A7 Server Room", 0);
            RoomTracker[] MainRooms = MainLevelGroup.GetComponentsInChildren<RoomTracker>();
            foreach (var item in MainRooms)
            {
                item.CanBeDetected = false;
            }
            TutorialLevel.GetComponent<RoomTracker>().CanBeDetected = true;
            SetupCameraButton();
        }
        else if(stage == 1)
        {
            CanCameraTurnLeft = true;
            CanCameraTurnRight = true;
            NPCListPanel.gameObject.SetActive(true);
            CameraButtonListPanel.gameObject.SetActive(true);
            CameraName.gameObject.SetActive(true);
            MainLevelGroup.SetActive(true);
            TutorialLevel.SetActive(false);
            for (int i = 0; i < Rooms.Count; i++)
            {
                if(Rooms[i].RoomName() == "BedRoom_A")
                {
                    Rooms[i].CanBeDetected = true;
                }
                else
                {
                    Rooms[i].CanBeDetected = false;
                }
            }
            if(TriggeringEventNode != null)
            {
                if(TriggeringEventNode.GUID == "a30bb683-8c34-4721-ad04-06c810241ebc")
                {
                    EventForceNext();
                }
            }
            RoomSwitch("BedRoom_A", 0);
            TutorialLevel.GetComponent<RoomTracker>().CanBeDetected = false;
            SetupCameraButton();
            //Destroy(TutorialLevel);
        }
        else if(stage == 2)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                if(Rooms[i].CBoard != null)
                {
                    if(!Rooms[i].CBoard.isLocked)
                    {
                        Rooms[i].CanBeDetected = true;
                    }
                    else
                    {
                        Rooms[i].CanBeDetected = false;
                    }
                }
                else if(Rooms[i].RoomName() != "A7 Server Room")
                {
                    Rooms[i].CanBeDetected = true;
                }
            }
            SetupCameraButton();
        }
        else
        {
            SetupCameraButton();
        }
    }

    void GoToState(GameManagerState next)
    {
        ClearConditionCache();
        ClearEventTriggerCache();
        justEnter = true;
        gmState = next;
    }

    void Next()
    {
        switch (gmState)
        {
            case GameManagerState.OFF:
                switch (Stage)
                {
                    case 0:
                        eventGraph.graph.Next(eventGraph.graph.SetNode(new List<string>() { "StartNode" })[0]);
                        break;
                    case 1:
                        //eventGraph.graph.SetNode(new List<string>() { "4c2d20a2-a0e5-4d26-ab1d-00ee2f69f237" });
                        eventGraph.graph.Next(eventGraph.graph.SetNode(new List<string>() { "StartNode" })[0]);
                        break;
                    default:
                        break;
                }
                GoToState(GameManagerState.CONDITIONING);
                break;
            case GameManagerState.CONDITIONING:
                break;
            case GameManagerState.PROGRESSING:
                break;
            case GameManagerState.PAUSED:
                eventGraph.graph.Next(TriggeringEventNode);
                TriggeringEventNode = null;
                GoToState(GameManagerState.CONDITIONING);
                break;
            default:
                break;
        }
    }

    bool Conditioning(EventNode cur)
    {
        bool HasNotAchieve = false;
        //EventNode cur = eventGraph.graph.current as EventNode;
        if(cur != null)
        {
            for (int i = 0; i < cur.conditionSOs.Count; i++)
            {
                ConditionSO con = cur.conditionSOs[i];
                if (con != null)
                {
                    switch (con.conditionWith)
                    {
                        case ConditionSO.ConditionWith.NPC:
                            switch (con.nPCConditinoWith)
                            {
                                case ConditionSO.NPCConditionWith.HP:
                                    for (int a = 0; a < con.NPC.Count; a++)
                                    {
                                        switch (con.nPCHPWith)
                                        {
                                            case ConditionSO.EqualType.Greater:
                                                if (con.NPC[a].status.currentHealth <= con.HP)
                                                {
                                                    HasNotAchieve = true;
                                                }
                                                break;
                                            case ConditionSO.EqualType.Less:
                                                if (con.NPC[a].status.currentHealth >= con.HP)
                                                {
                                                    HasNotAchieve = true;
                                                }
                                                break;
                                            case ConditionSO.EqualType.Equal:
                                                if (con.NPC[a].status.currentHealth != con.HP)
                                                {
                                                    HasNotAchieve = true;
                                                }
                                                break;
                                            case ConditionSO.EqualType.GEqual:
                                                if (con.NPC[a].status.currentHealth < con.HP)
                                                {
                                                    HasNotAchieve = true;
                                                }
                                                break;
                                            case ConditionSO.EqualType.LEqual:
                                                if (con.NPC[a].status.currentHealth > con.HP)
                                                {
                                                    HasNotAchieve = true;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        if (HasNotAchieve)
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case ConditionSO.ConditionWith.Room:
                            switch (con.roomConditionWith)
                            {
                                case ConditionSO.RoomConditionWith.Number_of_NPCs:
                                    for (int a = 0; a < con.roomTrackers.Count; a++)
                                    {
                                        switch (con.Room_NPC_Num)
                                        {
                                            case ConditionSO.EqualType.Greater:
                                                if(con.roomTrackers[a].NPC().Count <= con.NPC_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.Less:
                                                if (con.roomTrackers[a].NPC().Count >= con.NPC_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.Equal:
                                                if (con.roomTrackers[a].NPC().Count != con.NPC_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.GEqual:
                                                if (con.roomTrackers[a].NPC().Count < con.NPC_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.LEqual:
                                                if (con.roomTrackers[a].NPC().Count > con.NPC_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            default:
                                                break;
                                        }
                                        if (HasNotAchieve)
                                            break;
                                    }
                                    break;
                                case ConditionSO.RoomConditionWith.Specific_NPCs:
                                    for (int a = 0; a < con.SpecificRoomNPCs.Count; a++)
                                    {
                                        for (int b = 0; b < con.SpecificRoomNPCs[a].npcControllers.Count; b++)
                                        {
                                            if(!con.SpecificRoomNPCs[a].roomTracker.NPC().Contains(con.SpecificRoomNPCs[a].npcControllers[b].gameObject))
                                            {
                                                HasNotAchieve = true;
                                                break;
                                            }
                                        }
                                        if (HasNotAchieve)
                                            break;
                                    }
                                    break;
                                case ConditionSO.RoomConditionWith.Number_of_Enemys:
                                    for (int a = 0; a < con.roomTrackers.Count; a++)
                                    {
                                        switch (con.Room_Enemy_Num)
                                        {
                                            case ConditionSO.EqualType.Greater:
                                                if (con.roomTrackers[a].Enemy().Count <= con.Enemy_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.Less:
                                                if (con.roomTrackers[a].Enemy().Count >= con.Enemy_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.Equal:
                                                if (con.roomTrackers[a].Enemy().Count != con.Enemy_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.GEqual:
                                                if (con.roomTrackers[a].Enemy().Count < con.Enemy_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            case ConditionSO.EqualType.LEqual:
                                                if (con.roomTrackers[a].Enemy().Count > con.Enemy_Number)
                                                    HasNotAchieve = true;
                                                break;
                                            default:
                                                break;
                                        }
                                        if (HasNotAchieve)
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case ConditionSO.ConditionWith.Enemy:
                            switch (con.enemyConditionWith)
                            {
                                case ConditionSO.EnemyConditionWith.Overall_Numbers:
                                    switch (con.Enemy_Num)
                                    {
                                        case ConditionSO.EqualType.Greater:
                                            if (Enemy.Count <= con.OverallNumbers)
                                                HasNotAchieve = true;
                                            break;
                                        case ConditionSO.EqualType.Less:
                                            if (Enemy.Count >= con.OverallNumbers)
                                                HasNotAchieve = true;
                                            break;
                                        case ConditionSO.EqualType.Equal:
                                            if (Enemy.Count != con.OverallNumbers)
                                                HasNotAchieve = true;
                                            break;
                                        case ConditionSO.EqualType.GEqual:
                                            if (Enemy.Count < con.OverallNumbers)
                                                HasNotAchieve = true;
                                            break;
                                        case ConditionSO.EqualType.LEqual:
                                            if (Enemy.Count > con.OverallNumbers)
                                                HasNotAchieve = true;
                                            break;
                                        default:
                                            break;
                                    }
                                    if (HasNotAchieve)
                                        break;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case ConditionSO.ConditionWith.Event:
                            #region Event
                            if (justEnterCondition)
                            {
                                for (int a = 0; a < con.eventTriggers.Count; a++)
                                {
                                    EvtGraph.EventTrigger tri = new EvtGraph.EventTrigger { IsTriggered = false, EventName = con.eventTriggers[a].EventName };
                                    WaitingEvent.Add(tri);
                                    EventCenter.GetInstance().AddEventListener(tri.EventName, () => { 
                                        tri.IsTriggered = true; 
                                        EventCenter.GetInstance().RemoveEventListenerKeys(tri.EventName);
                                    });
                                }
                            }

                            for (int a = 0; a < WaitingEvent.Count; a++)
                            {
                                if (!WaitingEvent[a].IsTriggered)
                                {
                                    HasNotAchieve = true;
                                }
                            }
                            #endregion
                            break;
                        case ConditionSO.ConditionWith.Custom:
                            #region Custom
                            if (justEnterCondition)
                            {
                                for (int a = 0; a < con.customConditions.Count; a++)
                                {
                                    if (!con.customConditions[a].Instan)
                                    {
                                        CustomCondition com = gameObject.AddComponent(con.customConditions[a].GetType()) as CustomCondition;
                                        if(customConditions.ContainsKey(cur.GUID))
                                        {
                                            customConditions[cur.GUID].Add(com);
                                        }
                                        else
                                        {
                                            customConditions.Add(cur.GUID, new List<CustomCondition>(){ com });
                                        }
                                    }
                                    else
                                    {
                                        if (customConditions.ContainsKey(cur.GUID))
                                        {
                                            customConditions[cur.GUID].Add(con.customConditions[a]);
                                        }
                                        else
                                        {
                                            customConditions.Add(cur.GUID, new List<CustomCondition>() { con.customConditions[a] });
                                        }
                                    }
                                }
                            }
                            foreach (var keys in customConditions.Keys)
                            {
                                if(keys != cur.GUID)
                                {
                                    continue;
                                }
                                else
                                {
                                    for (int a = 0; a < customConditions[keys].Count; a++)
                                    {
                                        if(!customConditions[keys][a].Conditional())
                                        {
                                            HasNotAchieve = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            #endregion
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        if (HasNotAchieve)
            return false;
        else
            return true;
    }

    //TODO 事件完成检测Debug
    void TimelineStop()
    {
        foreach (var item in Directors)
        {
            if (item.Director.time == 0)
            {
                item.IsPlaying = true;
            }
        }
    }

    bool IsEventFinish()
    {
        bool HasNotFinish = false;
        EventNode cur = TriggeringEventNode;
        if(cur != null)
        {
            for (int z = 0; z < cur.eventSO.Count; z++)
            {
                EventSO evt = cur.eventSO[z];
                if(evt != null)
                {
                    switch (evt.doingWith)
                    {
                        case DoingWith.NPC:
                            switch (evt.doingWithNPC)
                            {
                                case DoingWithNPC.Talking:
                                    for (int i = 0; i < evt.NPCTalking.Count; i++)
                                    {
                                        if (evt.NPCTalking[i].room.DiaPlay.currentGraph == evt.NPCTalking[i].Graph || evt.NPCTalking[i].room.WaitingGraph == evt.NPCTalking[i].Graph)
                                        {
                                            HasNotFinish = true;
                                            break;
                                        }
                                    }
                                    break;
                                case DoingWithNPC.MoveTo:
                                    break;
                                case DoingWithNPC.Patrol:
                                    break;
                                case DoingWithNPC.AnimState:
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case DoingWith.Room:
                            break;
                        case DoingWith.Enemy:
                            switch (evt.doingWithEnemy)
                            {
                                case DoingWithEnemy.Spawn:
                                    break;
                                case DoingWithEnemy.MoveTo:
                                    for (int a = 0; a < evt.EnemyWayPoint.Count; a++)
                                    {
                                        if(evt.EnemyWayPoint[a].Obj.GetComponent<EnemyController>().toDoList.Count > 0)
                                        {
                                            HasNotFinish = true;
                                            break;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case DoingWith.Dialogue://√
                            if(evt.Dialogue_Room.DiaPlay.currentGraph == evt.Dialogue_Graph)
                            {
                                HasNotFinish = true;
                            }
                            break;
                        case DoingWith.Timeline://√
                            foreach (var timelines in evt.timelines)
                            {
                                foreach (var item in Directors)
                                {
                                    if (item.Director == timelines)
                                    {
                                        if (!item.IsPlaying)
                                        {
                                            HasNotFinish = true;
                                            break;
                                        }
                                    }
                                }
                                if (HasNotFinish)
                                    break;
                            }
                            break;
                        case DoingWith.Custom://√
                            for (int i = 0; i < evt.CustomCode.Count; i++)
                            {
                                if(!evt.CustomCode[i].IsEventFinish)
                                {
                                    HasNotFinish = true;
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                if(HasNotFinish)
                    break;
            }
        }
        if (HasNotFinish)
            return false;
        else
        {
            Debug.Log("Event Move Next");
            return true;
        }
    }
    
    void EventDistribute()
    {
        EventNode cur = TriggeringEventNode;
        if (cur != null)
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
                                case DoingWithNPC.Talking://√
                                    for (int a = 0; a < evt.NPCTalking.Count; a++)
                                    {
                                        Debug.Log("Clear NPC Agent List from GM. ", gameObject);
                                        evt.NPCTalking[a].room.NPCAgentList.Clear();
                                        for (int b = 0; b < evt.NPCTalking[a].moveToClasses.Count; b++)
                                        {
                                            for (int c = 0; c < NPC.Count; c++)
                                            {
                                                if(evt.NPCTalking[a].moveToClasses[b].Obj == NPC[c].gameObject)
                                                {
                                                    NPC[c].status.toDoList.Add(evt);
                                                    evt.NPCTalking[a].room.NPCAgentList.Add(NPC[c].status.npcName, false);
                                                    NPC[c].TriggerEvent();
                                                }
                                            }
                                        }
                                        if(evt.NPCTalking[a].room.WaitingGraph == null)
                                        {
                                            evt.NPCTalking[a].room.WaitingGraph = evt.NPCTalking[a].Graph;
                                        }
                                    }
                                    break;
                                case DoingWithNPC.MoveTo://√
                                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                                    {
                                        for (int b = 0; b < NPC.Count; b++)
                                        {
                                            if (NPC[b].gameObject == evt.NPCWayPoint[a].Obj)
                                            {
                                                NPC[b].status.toDoList.Add(evt);
                                            }
                                        }
                                    }
                                    break;
                                case DoingWithNPC.Patrol://√
                                    for (int a = 0; a < evt.NPC.Count; a++)
                                    {
                                        NPC[a].BackToPatrol();
                                    }
                                    break;
                                case DoingWithNPC.AnimState://√
                                    for (int a = 0; a < evt.NPC.Count; a++)
                                    {
                                        NPC[a].SwitchAnimState(evt.IsAnimState, evt.AnimStateName);
                                    }
                                    break;
                                case DoingWithNPC.Interact://√
                                    switch (evt.doingWithNPC_Interact)
                                    {
                                        case DoingWithNPC_Interact.InteractObject:
                                            evt.NPCInteract.ReceiveInteractCall(evt.InteractObject.gameObject);
                                            break;
                                        case DoingWithNPC_Interact.InteractItem:
                                            evt.NPCInteract.ReceiveInteractCall(evt.Item.gameObject);
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case DoingWith.Room:
                            //Nothing
                            break;
                        case DoingWith.Enemy://√
                            switch (evt.doingWithEnemy)
                            {
                                case DoingWithEnemy.Spawn:
                                    foreach (var item in evt.SpawnPoint)
                                    {
                                        GameObject obj = Instantiate(ZombieObject, evt.SpawnPoint[i]);
                                    }
                                    break;
                                case DoingWithEnemy.MoveTo:
                                    for (int a = 0; a < evt.EnemyWayPoint.Count; a++)
                                    {
                                        evt.EnemyWayPoint[a].Obj.GetComponent<EnemyController>().toDoList.Add(evt);
                                        evt.EnemyWayPoint[a].Obj.GetComponent<EnemyController>().TriggerEvent();
                                    }
                                    break;
                                default:
                                    break;
                            }
                            //TODO
                            break;
                        case DoingWith.Custom://√
                            for (int a = 0; a < evt.CustomCode.Count; a++)
                            {
                                evt.CustomCode[a].enabled = true;
                                evt.CustomCode[a].DoEvent(null);
                            }
                            break;
                        case DoingWith.Timeline://√
                            foreach (var item in evt.timelines)
                            {
                                Directors.Add(new DirectorsClass() {Director = item, IsPlaying = false });
                                item.Play();
                                item.time = 0.001f;
                            }
                            break;
                        case DoingWith.Dialogue://√
                            evt.Dialogue_Room.PlayingDialogue(evt.Dialogue_Graph);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    void ClearConditionCache()
    {
        //WaitingEvent.Clear();
        ConditionalWaitingNode.Clear();
        justEnterCondition = true;
        foreach (var keys in customConditions.Keys)
        {
            for (int i = 0; i < customConditions[keys].Count; i++)
            {
                Destroy(customConditions[keys][i]);
            }
        }
        customConditions.Clear();
    }

    void ClearEventTriggerCache()
    {
        justEnterEventTrigger = true;
        for (int i = 0; i < EventScripts.Count; i++)
        {
            Destroy(EventScripts[i]);
        }
        EventScripts.Clear();
    }
}