﻿using System.Collections;
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
    bool justEnterEventTrigger = true;

    [Header("EventConditionalCache")]
    List<EventTrigger> WaitingEvent = new List<EventTrigger>();
    List<EventNode> WaitingNode = new List<EventNode>();
    List<CustomCondition> customConditions = new List<CustomCondition>();
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
    public Transform CameraButtonListPanel;
    public GameObject CameraButton;

    void Awake()
    {
        SetupScene();
        eventGraph = GetComponent<EventGraphScene>();
        EventCenter.GetInstance().AddEventListener<NpcController>("GM.NPC.Add", NPCAdd);
        EventCenter.GetInstance().AddEventListener<RoomTracker>("GM.Room.Add", RoomAdd);
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
        //switch (gmState)
        //{
        //    case GameManagerState.OFF:
        //        if(justEnter)
        //        {
        //            justEnter = false;
        //            Next();
        //        }
        //        break;
        //    case GameManagerState.CONDITIONING:
        //        if (justEnter)
        //        {
        //            justEnter = false;
        //        }
        //        if (Conditioning())
        //            GoToState(GameManagerState.PROGRESSING);
        //        break;
        //    case GameManagerState.PROGRESSING:
        //        if (justEnter)
        //        {
        //            justEnter = false;
        //        }
        //        TriggerEvent();
        //        break;
        //    case GameManagerState.PAUSED:
        //        if (justEnter)
        //        {
        //            justEnter = false;
        //        }
        //        break;
        //    default:
        //        break;
        //}
        #endregion

        #region Mouse Clicking
        //Mouse Position Correction
        Vector3 MousePos = Input.mousePosition;

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
            Ray ray = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ScreenPointToRay(MousePos);
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

        //HighLight
        Ray ray_outline = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ScreenPointToRay(MousePos);
        if (Physics.Raycast(ray_outline, out RaycastHit hitInfomation, Mathf.Infinity, DoWithLayer))
        {
            //HighLight
            ControllerBased curCB = hitInfomation.collider.GetComponent<ControllerBased>();
            if (LastCB != null)
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
                if (curCB.gameObject.layer != LayerMask.NameToLayer("Room"))
                    curCB.SetOutline(true);
                LastCB = curCB;
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
            Ray ray = CurrentRoom.cameraLists[CurrentRoom.CurrentCameraIndex].roomCamera.ScreenPointToRay(MousePos);
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
                eventGraph.graph.SetNode();
                eventGraph.graph.Next();
                GoToState(GameManagerState.CONDITIONING);
                //Conditioning();
                //TriggerEvent();
                break;
            case GameManagerState.CONDITIONING:
                break;
            case GameManagerState.PROGRESSING:
                break;
            case GameManagerState.PAUSED:
                eventGraph.graph.Next();
                GoToState(GameManagerState.CONDITIONING);
                //Conditioning();
                //TriggerEvent();
                break;
            default:
                break;
        }
    }

    bool Conditioning()
    {
        bool HasNotAchieve = false;
        EventNode cur = eventGraph.graph.current as EventNode;
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
                            if (justEnterCondition)
                            {
                                for (int a = 0; a < con.eventTriggers.Count; a++)
                                {
                                    WaitingEvent.Add(new EventTrigger { IsTriggered = false, EventName = con.eventTriggers[i].EventName });
                                }
                            }
                            for (int b = 0; b < WaitingEvent.Count; b++)
                            {
                                if(!WaitingEvent[b].IsTriggered)
                                {
                                    HasNotAchieve = true;
                                    break;
                                }
                            }
                            break;
                        case ConditionSO.ConditionWith.Custom:
                            if (justEnterCondition)
                            {
                                for (int a = 0; a < con.customConditions.Count; a++)
                                {
                                    if (!con.customConditions[a].Instan)
                                    {
                                        CustomCondition com = gameObject.AddComponent(con.customConditions[a].GetType()) as CustomCondition;
                                        customConditions.Add(com);
                                    }
                                    else
                                    {
                                        customConditions.Add(con.customConditions[a]);
                                    }
                                }
                            }
                            for (int a = 0; a < customConditions.Count; a++)
                            {
                                if(!customConditions[a].Conditional())
                                {
                                    HasNotAchieve = true;
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        justEnterCondition = false;

        if (HasNotAchieve)
            return false;
        else
            return true;
    }

    void TriggerEvent()
    {
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
        GoToState(GameManagerState.PAUSED);
    }

    void ClearConditionCache()
    {
        WaitingEvent.Clear();
        WaitingNode.Clear();
        justEnterCondition = true;
        for (int i = 0; i < customConditions.Count; i++)
        {
            Destroy(customConditions[i]);
        }
        customConditions.Clear();
    }

    void ClearEventTriggerCache()
    {
        justEnterEventTrigger = true;
    }
}