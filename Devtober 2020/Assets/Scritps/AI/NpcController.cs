using EvtGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcController : ControllerBased
{
    #region Inspector View
    [System.Serializable]
    public class Status
    {
        public string npcName;
        public string description;

        public float maxHealth = 0;
        public float currentHealth = 0;

        public float maxStamina = 0;
        public float currentStamina = 0;

        public float maxCredit = 0;
        public float currentCredit = 0;

        public List<EventSO> toDoList;

        public string CarryItem;
    }

    public Status status = null;

    [SerializeField]
    [Range(0f, 50f)]
    public float detectRay = 0;

    [SerializeField]
    [Range(0f, 100f)]
    float alertRadius = 0;

    [SerializeField]
    [Range(0f, 100f)]
    float bannedRadius = 0;

    [SerializeField]
    LayerMask needDodged = 0;

    [SerializeField]
    Collider[] hitObjects = null;

    [SerializeField]
    float dodgeAngle = 0;

    [SerializeField]
    float dodgeSpeed = 0;

    [SerializeField]
    float restTime = 0;

    [SerializeField]
    float recoverTime = 0;

    [SerializeField]
    [Tooltip("The rest distance before reach destination. ")]
    float restDistance = 0.2f;

    #endregion


    #region Fields
    StringRestrictedFiniteStateMachine m_fsm;
    Animator animator;
    NavMeshAgent navAgent;
    NavMeshPath path;
    #endregion


    #region Value
    RaycastHit hit;
    RoomTracker currentRoomTracker;
    float recordRestTimer, recordRecoverTimer, recordSpeed;

    [HideInInspector]
    public Vector3 currentTerminalPos;

    Transform finalHidingPos;
    Transform finalEscapingPos;

    List<RoomTracker> roomScripts = new List<RoomTracker>();
    List<GameObject> hiddenSpots = new List<GameObject>();
    List<GameObject> rooms = new List<GameObject>();
    public List<Transform> wayPoints = new List<Transform>();

    public bool isSafe = false;
    bool MoveAcrossNavMeshesStarted;

    GameObject hideIn = null;
    HiddenPos hiddenPos;
    #endregion

    #region InteractWithItem
    [Header("Interact Item")]
    public float DampPosSpeed = 0.2f;
    public float DampRotSpeed = 0.2f;
    public BoxCollider boxCollider;

    Interact_SO CurrentInteractObject;
    LocatorList locatorList;
    int GrabOutIndex;
    bool IsGrabbing = false;

    Item_SO CurrentInteractItem;

    bool HasInteract = false;

    float VelocityPosX;
    float VelocityPosZ;
    #endregion

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        navAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        animator = GetComponent<Animator>();
        status.toDoList.Clear();
        recordRestTimer = restTime;
        recordRecoverTimer = recoverTime;
        recordSpeed = navAgent.speed;

        #region StringRestrictedFiniteStateMachine
        Dictionary<string, List<string>> NPCDictionary = new Dictionary<string, List<string>>()
        {
            { "Patrol", new List<string> { "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem" } },
            { "Rest", new List<string> { "Patrol", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem" } },
            { "Event", new List<string> { "Patrol", "Rest", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem" } },
            { "Dispatch", new List<string> { "Patrol", "Rest", "Event", "Dodging", "Hiding", "Escaping", "InteractWithItem" } },
            { "Dodging", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Hiding", "Escaping", "InteractWithItem" } },
            { "Hiding", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Escaping", "InteractWithItem" } },
            { "Escaping", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "InteractWithItem" } },
            { "InteractWithItem", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping" } }
        };

        m_fsm = new StringRestrictedFiniteStateMachine(NPCDictionary, "Patrol");
        #endregion

        #region RightClickMenu
        //AddMenu("Move", "Move", false, ReadyForDispatch);
        //AddMenu("HideAll", "Hide All", false, TriggerHiding); //TODO NPC集体躲进去。Call一个方法，这个方法给GM发消息，带上自己在的房间，然后GM就会识别你带的房间，然后给本房间内所有的NPC发消息，让他们躲起来
        AddMenu("Interact", "Interact", true, ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos") 
            | 1 << LayerMask.NameToLayer("RestingPos") 
            | 1 << LayerMask.NameToLayer("TerminalPos"));
        #endregion

        DetectRoom();
    }

    private void Start()
    {
        currentTerminalPos = NewDestination();
        Debug.Log(currentRoomTracker.gameObject.name);
        EventCenter.GetInstance().EventTriggered("GM.NPC.Add", this);

        Invoke("GenerateList", 0.00001f);
    }

    void GenerateList()
    {
        foreach (RoomTracker temp in GameManager.GetInstance().Rooms)
        {
            roomScripts.Add(temp);
        }

        for (int i = 0; i < roomScripts.Count; i++)
        {
            rooms.Add(roomScripts[i].Room());
        }


        foreach (GameObject temp in currentRoomTracker.HiddenPos())
        {
            if (!hiddenSpots.Contains(temp))
                hiddenSpots.Add(temp);
        }
    }

    private void Update()
    {
        //if(currentRoomTracker != null) {
        
        #region StringRestrictedFiniteStateMachine Update
        switch (m_fsm.GetCurrentState())
        {
            case "Patrol":
                if (!currentRoomTracker.isEnemyDetected())
                {
                    restTime -= Time.deltaTime;
                }
                if(restTime > 0)
                {
                    DetectRoom();
                    Dispatch(currentTerminalPos);
                    GenerateNewDestination();
                    TriggerDodging();
                }
                else
                {
                    navAgent.ResetPath();
                    recoverTime = recordRecoverTimer;
                    m_fsm.ChangeState("Rest");
                }
                break;
            case "Rest":
                if(m_fsm.GetPreviousState() == "Patrol")
                {
                    TriggerDodging();
                    animator.Play("Idle", 0);
                    recoverTime -= Time.deltaTime * status.currentStamina / 100;
                    if (recoverTime <= 0)
                    {
                        restTime = recordRestTimer;
                        BackToPatrol();
                    }
                }
                break;
            case "Dispatch":
                CompleteDispatching();
                break;
            case "Event":
                Event();
                ReachDestination();
                 break;
            case "Dodging":
                Dodging();
                break;
            case "Hiding":
                Hiding();
                Dispatch(finalHidingPos.position);
                break;
            case "Escaping":
                Dispatch(finalEscapingPos.position);
                CompleteEscaping();
                break;
            case "InteractWithItem":
                PlayGetInAnim();
                break;
            default:
                break;
        }
        #endregion
        //CheckEvent();
        if (navAgent.isOnOffMeshLink && !MoveAcrossNavMeshesStarted)
        {
            StartCoroutine(MoveAcrossNavMeshLink());
            MoveAcrossNavMeshesStarted = true;
        }

        //}
    }

    #region Move
    public void BackToPatrol(object obj = null)
    {
        navAgent.speed = recordSpeed;
        currentTerminalPos = NewDestination();
        m_fsm.ChangeState("Patrol");
        ResetHiddenPos();
    }

    public float Distance()
    {
        float a = navAgent.destination.x - transform.position.x;
        float b = navAgent.destination.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        return Mathf.Abs(c);
    }

    public Vector3 NewDestination()
    {
        Vector3 tempPos = Vector3.zero;
        currentRoomTracker = hit.collider.gameObject.GetComponent<RoomTracker>();
        if (currentRoomTracker != null)
        {
            int tempInt = Random.Range(0, currentRoomTracker.tempWayPoints.Count);

            float x = Random.Range(currentRoomTracker.tempWayPoints[tempInt].position.x, transform.position.x);
            float z = Random.Range(currentRoomTracker.tempWayPoints[tempInt].position.z, transform.position.z);
            tempPos = new Vector3(x, transform.position.y, z); 
        }
        return tempPos;
    }

    private void GenerateNewDestination()
    {
        float a = currentTerminalPos.x - transform.position.x;
        float b = currentTerminalPos.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        
        if (Mathf.Abs(c) < restDistance || !navAgent.CalculatePath(currentTerminalPos,path))
        {
            currentTerminalPos = NewDestination();
        }
    }

    IEnumerator MoveAcrossNavMeshLink()
    {
        OffMeshLinkData data = navAgent.currentOffMeshLinkData;
   
        Vector3 startPos = navAgent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * navAgent.baseOffset;
        float duration = (endPos - startPos).magnitude / navAgent.velocity.magnitude;
        float t = 0.0f;
        float tStep = 1.0f / duration;
        while (t < 1.0f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t);
            t += tStep * Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        navAgent.CompleteOffMeshLink();
        MoveAcrossNavMeshesStarted = false;
    }

    void DetectRoom()
    {
        if(Physics.Raycast(transform.position, -transform.up * detectRay, out hit, 1 << LayerMask.NameToLayer("Room")))
        {
            //currentRoomTracker = hit.collider.GetComponent<RoomTracker>();
        }
    }

    public void Dispatch(object newPos)
    {
        navAgent.SetDestination((Vector3)newPos);

        if(m_fsm.GetCurrentState() != "InteractWithItem")
        {
            if(navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
            {
                animator.Play("Walk", 0);
            }
            else if(!navAgent.isOnOffMeshLink)
            {
                animator.Play("Idle", 0);
            }
        }
    }

    public void ReadyForDispatch(object newPos)
    {
        Debug.Log("Ready for Dispatch");
        navAgent.SetDestination((Vector3)newPos);
        m_fsm.ChangeState("Dispatch");
    }

    public void CompleteDispatching()
    {
        if (Distance() < restDistance)
        {
            navAgent.ResetPath();
            BackToPatrol();
        }
    }
    #endregion

    #region Hiding
    public void TriggerHiding(object obj = null)
    {
        RemoveAndInsertMenu("Interact", "BackToPatrol", "Leave", false, BackToPatrol);
        navAgent.ResetPath();
        m_fsm.ChangeState("Hiding");
    }

    void Hiding()
    {
        float minDistance = Mathf.Infinity;
        foreach (GameObject temp in hiddenSpots)
        {
            HiddenPos hpos = temp.GetComponent<HiddenPos>();
            bool isTaken = false;
            for (int i = 0; i < hpos.Locators.Count; i++)
            {
                if (hpos.Locators[i].npc != null)
                {
                    isTaken = true;
                    break;
                }
            }
            if (isTaken)
                continue;
            foreach (var item in hpos.Locators)
            {
                Transform tempTrans = item.Locator;
                float a = tempTrans.position.x - transform.position.x;
                float b = tempTrans.position.z - transform.position.z;
                float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
                float distance = Mathf.Abs(c);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    hideIn = temp;
                    hiddenPos = temp.GetComponent<HiddenPos>();
                    finalHidingPos = tempTrans;
                }
            }
        }
    }

    void ResetHiddenPos()
    {
        if (hideIn != null && hiddenPos != null)
        {
            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = null;
            hideIn = null;
            hiddenPos = null;
        }
    }
    #endregion

    #region Event
    public void TriggerEvent()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Event");
    }

    private void Event()
    {
        for (int i = 0; i < status.toDoList.Count; i++)
        {
            EventSO evt = status.toDoList[0];
            switch (evt.doingWithNPC)
            {
                case DoingWithNPC.Talking:
                    for (int a = 0; a < evt.NPCTalking.Count; a++)
                    {
                        for (int b = 0; b < evt.NPCTalking[a].moveToClasses.Count; b++)
                        {
                            if(evt.NPCTalking[a].moveToClasses[b].NPC == gameObject)
                            {
                                Dispatch(evt.NPCTalking[a].moveToClasses[b].MoveTO.position);
                            }
                        }
                    }
                    break;
                case DoingWithNPC.MoveTo:
                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                    {
                        if (evt.NPCWayPoint[a].NPC == gameObject)
                        {
                            Dispatch(evt.NPCWayPoint[a].MoveTO.position);
                        }
                    }
                    break;
                case DoingWithNPC.Patrol:
                    break;
                default:
                    break;
            }
            status.toDoList.Remove(evt);
        }
    }

    public void CheckEvent()
    {
        if (status.toDoList != null)
        {
            if (status.toDoList.Count != 0)
            {
                m_fsm.ChangeState("Event");
            }
        }
    }

    public void ReachDestination()
    {
        if (Distance() <= restDistance)
        {
            EventCenter.GetInstance().EventTriggered("GM.AllNPCArrive", status.npcName);
            //TODO 修改NPC Arrive call的方法
            navAgent.ResetPath();
        }
    }
    #endregion

    #region Dodging
    public void TriggerDodging()
    {
        hitObjects = Physics.OverlapSphere(transform.position, alertRadius, needDodged);
        if (hitObjects.Length != 0)
        {
            navAgent.speed *= (dodgeSpeed * status.currentStamina) / 100;
            m_fsm.ChangeState("Dodging");
        }
    }

    private void Dodging()
    {
        hitObjects = Physics.OverlapSphere(transform.position, alertRadius, needDodged);

        for (int i = 0; i < hitObjects.Length; i++)
        {
            Vector3 enemyDirection = (transform.position - hitObjects[i].gameObject.transform.position).normalized;
            Vector3 movingDirection = (currentTerminalPos - transform.position).normalized;

            float a = currentTerminalPos.x - transform.position.x;
            float b = currentTerminalPos.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));

            if (Vector3.Angle(enemyDirection, movingDirection) > dodgeAngle / 2 || Mathf.Abs(c) < bannedRadius)
            {
                currentTerminalPos = NewDestination();
            }
        }
        Dispatch(currentTerminalPos);

        if (hitObjects.Length == 0)
        {
            BackToPatrol();
        }
    }
    #endregion

    #region Escaping
    public void TriggerEscaping()
    {
        navAgent.ResetPath();

        RaycastHit hitroom;
        Physics.Raycast(transform.position, -transform.up, out hitroom, detectRay, (int)Mathf.Pow(2, 9));

        foreach (GameObject temp in rooms)
        {
            if (!temp.GetComponent<RoomTracker>().isEnemyDetected())
            {
                finalEscapingPos = temp.transform;
                break;
            }
        }

        m_fsm.ChangeState("Escaping");
    }

    public void CompleteEscaping()
    {
        if (Distance() < restDistance)
        {
            navAgent.ResetPath();
            BackToPatrol();
        }
    }
    #endregion

    #region Receive Call
    public void ReceiveInteractCall(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        Interact_SO item = gameObj.GetComponent<Interact_SO>();
        StoragePos storge = item as StoragePos;
        if(item != null)
        {
            Debug.Log("Receive Interact Call");
            
            Vector3 Pos = Vector3.zero;
            float minDistance = Mathf.Infinity;
            bool isEmpty = false;
            for (int i = 0; i < item.Locators.Count; i++)
            {
                if (item.Locators[i].npc != null)
                    continue;
                isEmpty = true;
                float a = item.Locators[i].Locator.position.x - transform.position.x;
                float b = item.Locators[i].Locator.position.z - transform.position.z;
                float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
                float distance = Mathf.Abs(c);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    Pos = item.Locators[i].Locator.position;
                    locatorList = item.Locators[i];
                }
            }
            if (!isEmpty)
                return;

            if (storge != null)
            {
                IsGrabbing = false;
            }

            CurrentInteractObject = item;
            HasInteract = false;
            Dispatch(Pos);
            navAgent.speed *= (dodgeSpeed * status.currentStamina) / 100;
            m_fsm.ChangeState("InteractWithItem");
        }
    }

    public void ReceiveItemCall(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        Item_SO item = gameObj.GetComponent<Item_SO>();

        if(item != null)
        {
            Debug.Log("Receive Item Call");
        }

        CurrentInteractItem = item;
        HasInteract = false;
        Dispatch(gameObj.transform.position);
        navAgent.speed *= (dodgeSpeed * status.currentStamina) / 100;
        m_fsm.ChangeState("InteractWithItem");
        locatorList.Locator = gameObj.transform;
        locatorList.npc = null;
    }

    public void CallGrabOut(object obj)
    {

    }

    public void ReceiveGrabOut(StoragePos storgePos, int Index, bool Grabbing)
    {
        Vector3 Pos = Vector3.zero;
        float minDistance = Mathf.Infinity;
        bool isEmpty = false;
        for (int i = 0; i < storgePos.Locators.Count; i++)
        {
            if (storgePos.Locators[i].npc != null)
                continue;
            isEmpty = true;
            float a = storgePos.Locators[i].Locator.position.x - transform.position.x;
            float b = storgePos.Locators[i].Locator.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            float distance = Mathf.Abs(c);

            if (distance < minDistance)
            {
                minDistance = distance;
                Pos = storgePos.Locators[i].Locator.position;
                locatorList = storgePos.Locators[i];
            }
        }

        if (!isEmpty)
            return;

        IsGrabbing = Grabbing;
        CurrentInteractObject = storgePos;
        if (Grabbing)
        {
            GrabOutIndex = Index;
        }
        else
        {
            if(status.CarryItem == string.Empty)
            {
                Debug.Log(status.npcName + ": I don't have Item to store");
                return;
            }
        }
        HasInteract = false;
        Dispatch(Pos);
        navAgent.speed *= (dodgeSpeed * status.currentStamina) / 100;
        m_fsm.ChangeState("InteractWithItem");
    }
    #endregion

    #region Play Animation
    void PlayGetInAnim()
    {
        if (Distance() < restDistance || !navAgent.enabled)
        {
            boxCollider.enabled = false;
            bool Damping = false;

            Vector3 TraPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 IntPos = new Vector3(locatorList.Locator.position.x, 0, locatorList.Locator.position.z);

            if(TraPos.magnitude - IntPos.magnitude >= 0.0001)
            {
                transform.position = new Vector3(Mathf.SmoothDamp(transform.position.x, locatorList.Locator.position.x, ref VelocityPosX, DampPosSpeed)
                , transform.position.y
                , Mathf.SmoothDamp(transform.position.z, locatorList.Locator.position.z, ref VelocityPosZ, DampPosSpeed));
                Damping = true;
            }

            if(Quaternion.Angle(transform.rotation, locatorList.Locator.rotation) >= 0.2)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, locatorList.Locator.rotation, DampRotSpeed);
                Damping = true;
            }

            if (Damping)
            {
                Debug.Log("Damping");
            }
            else if (!HasInteract)
            {
                if(CurrentInteractObject != null)
                {
                    switch (CurrentInteractObject.type)
                    {
                        case Interact_SO.InteractType.Locker:
                            CurrentInteractObject.NPCInteract(0);
                            animator.Play("GetInLocker", 0);
                            isSafe = true;
                            HasInteract = true;
                            navAgent.enabled = false;
                            break;
                        case Interact_SO.InteractType.Box:
                            isSafe = true;
                            HasInteract = true;
                            navAgent.enabled = false;
                            break;
                        case Interact_SO.InteractType.Bed:
                            animator.Play("GetOnBed", 0);
                            HasInteract = true;
                            navAgent.enabled = false;
                            break;
                        case Interact_SO.InteractType.Chair:
                            HasInteract = true;
                            navAgent.enabled = false;
                            break;
                        case Interact_SO.InteractType.Terminal:
                            CurrentInteractObject.NPCInteract(0);
                            animator.Play("OperateTerminal", 0);
                            HasInteract = true;
                            navAgent.enabled = false;
                            break;
                        case Interact_SO.InteractType.Switch:
                            CurrentInteractObject.NPCInteract(0);
                            animator.Play("OperateSwitch", 0);
                            HasInteract = true;
                            navAgent.enabled = false;
                            break;
                        case Interact_SO.InteractType.Storge:
                            HasInteract = true;
                            StoragePos sto = CurrentInteractObject.GetComponent<StoragePos>();
                            //取东西
                            if (IsGrabbing)
                            {
                                animator.Play("GrabOutItem");
                                if (status.CarryItem != string.Empty)//如果NPC身上带着东西
                                {
                                    //Debug.Log(status.npcName + ": I cannot grab this item out because I have no place to put the item that I already carried on. ");
                                    string GrabOutItem = sto.StorageItem[GrabOutIndex];
                                    string PutInItem = status.CarryItem;

                                    CurrentInteractObject.NPCInteract(GrabOutIndex); // 删掉箱子内的物品
                                    sto.Store(PutInItem);// 放入NPC身上的物品
                                    status.CarryItem = GrabOutItem;//NPC 身上的东西等于要取出的东西
                                }
                                else//如果NPC身上没带东西
                                {
                                    status.CarryItem = sto.StorageItem[GrabOutIndex];
                                    CurrentInteractObject.NPCInteract(GrabOutIndex);
                                }
                            }
                            else //存东西
                            {
                                animator.Play("GrabOutItem");
                                if(sto.StorageItem.Count + 1 <= sto.MaxStorage)
                                {
                                    sto.StorageItem.Add(status.CarryItem);
                                    status.CarryItem = string.Empty;
                                }
                                else
                                {
                                    Debug.Log(status.npcName + ": It doesn't have place to store.");
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if(CurrentInteractItem != null)
                {
                    switch (CurrentInteractItem.type)
                    {
                        case Item_SO.ItemType.MedicalKit:
                            animator.Play("GrabItem", 0);
                            status.CarryItem = CurrentInteractItem.type.ToString();
                            CurrentInteractItem.NPCInteract(0);
                            break;
                        default:
                            break;
                    }
                }
                
            }
        }
        else if (navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
        {
            animator.Play("Walk", 0);
        }
        else if (!navAgent.isOnOffMeshLink)
        {
            animator.Play("Idle", 0);
        }
    }

    public void PlayGetOutAnim(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        Interact_SO item = gameObj.GetComponent<Interact_SO>();
        switch (item.type)
        {
            case Interact_SO.InteractType.Locker:
                transform.eulerAngles += new Vector3(0, 180, 0);
                animator.Play("GetOutLocker", 0);
                isSafe = false;
                break;
            case Interact_SO.InteractType.Box:
                break;
            case Interact_SO.InteractType.Bed:
                break;
            case Interact_SO.InteractType.Chair:
                break;
            case Interact_SO.InteractType.Terminal:
                animator.Play("GetOutTerminal", 0);
                break;
            default:
                break;
        }
        CurrentInteractObject = item;

    }

    public void CompleteGetInItemAction()
    {
        CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
        m_fsm.ChangeState("Rest");
    }

    public void CompleteGetOutItemAction()
    {
        boxCollider.enabled = true;
        navAgent.enabled = true;
        switch (CurrentInteractObject.type)
        {
            case Interact_SO.InteractType.Locker:
                CurrentInteractObject.RemoveAndInsertMenu("Leave", "Hide In", "Hide In", true, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                break;
            case Interact_SO.InteractType.Box:
                break;
            case Interact_SO.InteractType.Bed:
                CurrentInteractObject.RemoveAndInsertMenu("Leave", "RestIn", "RestIn", true, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                break;
            case Interact_SO.InteractType.Chair:
                CurrentInteractObject.RemoveAndInsertMenu("Leave", "RestIn", "RestIn", true, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                break;
            case Interact_SO.InteractType.Terminal:
                CurrentInteractObject.RemoveAndInsertMenu("Leave", "Operate", "Operate", false, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                break;
            default:
                break;
        }
        BackToPatrol();
    }
    #endregion

    #region Status Change
    public void ApplyHealth(float healthAmount)
    {
        status.currentHealth = status.currentHealth + healthAmount > status.maxHealth ? status.maxHealth : status.currentHealth += healthAmount;
    }

    public void ApplyStamina(float staminaAmount)
    {
        status.currentStamina = status.currentStamina + staminaAmount > status.maxStamina ? status.maxStamina : status.currentStamina += staminaAmount;
    }

    public void ApplyCredit(float creditAmount)
    {
        status.currentCredit = status.currentCredit + creditAmount > status.maxCredit ? status.maxCredit : status.currentCredit += creditAmount;
    }

    public void RecoverStamina(float rate)
    {
        status.currentStamina = status.currentStamina + rate * Time.deltaTime > status.maxStamina ? status.maxStamina : status.currentStamina += rate * Time.deltaTime;
    }

    public void TakeDamage(float damageAmount)
    {
        status.currentHealth -= damageAmount;

        if (status.currentHealth <= 0)
        {
            //Death();
        }
    }

    public void ConsumeStamina(float staminaAmount)
    {
        status.currentStamina = status.currentStamina - staminaAmount <= 0 ? 0 : status.currentStamina -= staminaAmount;
    }

    public void ReduceCredit(float creditAmount)
    {
        status.currentCredit = status.currentCredit - creditAmount <= 0 ? 0 : status.currentCredit -= creditAmount;
    }
    #endregion


    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentTerminalPos, 1);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, bannedRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -transform.up * detectRay);
    }
    #endregion
}