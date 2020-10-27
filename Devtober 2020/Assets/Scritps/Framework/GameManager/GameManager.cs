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
using UnityEngine.Playables;
using UnityEngine.Timeline;

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
    CONDITIONING,
    PROGRESSING,
    PAUSED
}

public class GameManager : SingletonBase<GameManager>
{
    [Header("Input")]
    public string MoveLeft;
    public string MoveRight;

    [Header("Event")]
    public EventGraphScene eventGraph;
    public GameManagerState gmState;
    bool justEnter = true;

    [Header("EventTriggerCache")]
    List<EventScriptInterface> EventScripts = new List<EventScriptInterface>();
    EventNode TriggeringEventNode = null;
    bool justEnterEventTrigger = true;

    [Header("EventConditionalCache")]
    Dictionary<string, List<EventTrigger>> WaitingEvent = new Dictionary<string, List<EventTrigger>>();
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

    [Header("Camera Button List")]
    public Camera MainCamera;
    public Texture VHSTexture;
    public Transform CameraButtonListPanel;
    public GameObject CameraButton;

    [Header("Timeline Playing")]
    public PlayableDirector Director;
    public TimelineAsset timeline;

    void Awake()
    {
        SetupScene();
        eventGraph = GetComponent<EventGraphScene>();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
        EventCenter.GetInstance().AddEventListener<RoomTracker>("GM.Room.Add", RoomAdd);
        EventCenter.GetInstance().AddEventListener<EnemyController>("GM.Enemy.Add", EnemyAdd);
    }

    void Start()
    {
        RoomSwitch("Main Hall", 0);
        SetupCameraButton();
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
                Rooms[i].cameraLists[CameraIndex].roomCamera.gameObject.SetActive(true);
                Rooms[i].CurrentCameraIndex = CameraIndex;
                CurrentRoom = Rooms[i];
            }
        }
        SetupOption();
    }

    void SetupCameraButton()
    {
        for (int i = 0; i < Rooms.Count; i++)
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
        #region Input
        if (Input.GetKey(MoveLeft))
        {
            CurrentRoom.Rotate(-1, CurrentRoom.CurrentCameraIndex);
        }
        else if (Input.GetKey(MoveRight))
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
            EventCenter.GetInstance().EventTriggered("TU_TurnLeftCheck", "TU_TurnLeftCheck");
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            EventCenter.GetInstance().EventTriggered("TU_TurnRightCheck", "TU_TurnRightCheck");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Director.Play(timeline);
        }
        #endregion

        #region NavMeshBuilding
        //NavMesh Building
        if (Time.frameCount % 10 == 0)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (Rooms[i].isEnemyDetected() && Rooms[i].NPC().Count > 0)
                {
                    for (int a = 0; a < Rooms[i].navSurface.Count; a++)
                    {
                        Rooms[i].navSurface[a].BuildNavMesh();
                    }
                }
            }
        }
        #endregion

        #region UpdateText
        StartCoroutine(UpdateText());
        #endregion

        #region UpdateNPCList()
        //----------------------
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
                    justEnter = false;
                    for (int i = 0; i < eventGraph.graph.currentList.Count; i++)
                    {
                        EventNode evt = eventGraph.graph.currentList[i] as EventNode;
                        if (evt != null)
                        {
                            ConditionalWaitingNode.Add(evt);
                        }
                    }
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
                }
                if(!TriggerEvent())
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
        Vector3 MousePos = Input.mousePosition;
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

        //Right Click Menu
        if (Input.GetMouseButtonDown(1) && !IsWaitingForClickObj)
        {
            ClearRightClickButton();
            Ray ray = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ViewportPointToRay(MousePos);
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
                            if(!based.IsInteracting)
                            {
                                SetupRightClickMenu(based.rightClickMenus);
                            }
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
            //UI 位置适配
            RightClickMenuPanel.position = new Vector3(MousePos.x, MousePos.y, 0);
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

        //HighLight
        Ray ray_outline = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ViewportPointToRay(MousePos);
        if (Physics.Raycast(ray_outline, out RaycastHit hitInfomation, Mathf.Infinity, DoWithLayer))
        {
            Debug.DrawLine(ray_outline.origin, hitInfomation.point);
            //HighLight
            ControllerBased curCB = hitInfomation.collider.GetComponent<ControllerBased>();
            if (LastCB != null && curCB != null)
            {
                if (curCB != LastCB)
                {
                    LastCB.SetOutline(false);
                    if (curCB.gameObject.layer != LayerMask.NameToLayer("Room"))
                        curCB.SetOutline(true);
                    LastCB = curCB;
                }
            }
            else
            {
                if(curCB != null)
                {
                    if (curCB.gameObject.layer != LayerMask.NameToLayer("Room"))
                        curCB.SetOutline(true);
                    LastCB = curCB;
                }
            }
        }
        else
        {
            //HighLight
            if(LastCB != null)
            {
                LastCB.SetOutline(false);
                LastCB = null;
            }
        }
        
        if(IsWaitingForClickObj)
        {
            //ChangeWaitingCursor
            Ray ray = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ViewportPointToRay(MousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, RightClickMs.InteractLayer))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);
                
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
        #endregion
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
                eventGraph.graph.Next(eventGraph.graph.SetNode(new List<string>() {"StartNode"})[0]);
                GoToState(GameManagerState.CONDITIONING);
                //Conditioning();
                //TriggerEvent();
                break;
            case GameManagerState.CONDITIONING:
                break;
            case GameManagerState.PROGRESSING:
                break;
            case GameManagerState.PAUSED:
                eventGraph.graph.Next(TriggeringEventNode);
                TriggeringEventNode = null;
                GoToState(GameManagerState.CONDITIONING);
                //Conditioning();
                //TriggerEvent();
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
                                    if(!WaitingEvent.ContainsKey(cur.GUID))
                                    {
                                        WaitingEvent.Add(cur.GUID, new List<EventTrigger>() { new EventTrigger { IsTriggered = false, EventName = con.eventTriggers[i].EventName } });
                                    }
                                    else
                                    {
                                        WaitingEvent[cur.GUID].Add(new EventTrigger { IsTriggered = false, EventName = con.eventTriggers[i].EventName });
                                    }
                                }
                            }
                            foreach (var keys in WaitingEvent.Keys)
                            {
                                if(keys != cur.GUID)
                                {
                                    continue;
                                }
                                else
                                {
                                    for (int a = 0; a < WaitingEvent[keys].Count; a++)
                                    {
                                        if(!WaitingEvent[keys][a].IsTriggered)
                                        {
                                            HasNotAchieve = true;
                                            break;
                                        }
                                    }
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

    //TODO 执行时间
    bool TriggerEvent()
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
                                if(evt.CustomCode[a] != null)
                                {
                                    if(!evt.CustomCode[a].Instan)
                                    {
                                        EventScriptInterface com = gameObject.AddComponent(evt.CustomCode[a].GetType()) as EventScriptInterface;
                                        EventScripts.Add(com);
                                        com.DoEvent(null);
                                    }
                                    else
                                    {
                                        evt.CustomCode[a].DoEvent(null);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        return false;
    }

    void ClearConditionCache()
    {
        WaitingEvent.Clear();
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