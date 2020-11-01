using EvtGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay;
using DiaGraph;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
public class NpcController : ControllerBased
{
    [Header("Dialogue")]
    public DialogueGraph NoThingTalk;
    public DialogueGraph MedicalKit;
    public DialogueGraph JoinIn;
    public DialogueGraph HealPriDia;
    public int Stage = 0;
    bool isEnemyEnter = false;

    public bool inAnimState = false;
    public bool IsPrisoner = false;
    public bool isGuard = false;
    public bool isGrabbingGuardName = false;
    public bool isPrristNeedHeal = false;
    public bool NeedRemoveMenu = true;
    public bool Flashing;
    public float FlashingSpeed;
    bool IsIncreaseing;

    public string AnimStateName = string.Empty;
    public bool SendEventWhenGetOutLocker = false;

    #region Inspector View
    [System.Serializable]
    public class Status
    {
        public string npcName;
        public string description;

        public float maxHealth = 100;
        public float currentHealth = 0;
        public float getUpHealth = 50;

        public float maxStamina = 100;
        public float minStamina = 1;
        public float currentStamina = 0;

        public float maxCredit = 0;
        public float currentCredit = 0;

        public float fixRate = 5;

        public List<EventSO> toDoList;

        public Item_SO.ItemType CarryItem = Item_SO.ItemType.None;
        public string code;

        public float healAmount = 0;

        public bool isStruggling = false;
    }

    public Status status = null;
    [SerializeField]
    float boostSpeed = 0;
    [SerializeField]
    float limpingLimit = 0;
    [SerializeField]
    [Tooltip("The rest distance before reach destination. ")]
    float restDistance = 0.2f;

    [Header("Visual Setting")]
    [SerializeField]
    [Tooltip("The Ray to detect current Room")]
    [Range(0f, 50f)]
    float detectRay = 0;

    [SerializeField]
    [Range(0f, 100f)]
    float alertRadius = 0;
    [SerializeField]
    [Range(0f, 100f)]
    float bannedRadius = 0;

    [Header("Dodging Setting")]
    [SerializeField]
    LayerMask needDodged = 0;
    [SerializeField]
    LayerMask canBlocked = 0;

    [SerializeField]
    float dodgeAngle = 0;

    [Header("Rescuing Setting")]
    [SerializeField]
    float discoverAngle = 0;

    [Header("Idle Setting")]
    [SerializeField]
    float restTime = 0;
    [SerializeField]
    float recoverTime = 0;

    bool IsRandomTalking = false;
    Vector3 TalkingPos = Vector3.zero;

    public NpcController HighP;
    public DialogueGraph Graph1;
    public DialogueGraph Graph1_5;
    public EventSO PuEvt;
    #endregion


    #region Fields
    [HideInInspector]
    public StringRestrictedFiniteStateMachine m_fsm;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public NavMeshAgent navAgent;
    RoomTracker currentRoomTracker;
    NavMeshPath path;
    #endregion


    #region Value
    [HideInInspector]
    public Vector3 currentTerminalPos;
    [HideInInspector]
    public Transform fixTargetTransform;
    Vector3 recordColliderSize;

    public bool isSafe = false;

    public bool isEnemyChasing = false;
    public bool isRoomCalled = false;
    float recordRestTimer, recordRecoverTimer, recordSpeed;
    bool MoveAcrossNavMeshesStarted;
    bool inAngle, isBlocked;

    List<RoomTracker> roomScripts = new List<RoomTracker>();

    [HideInInspector]
    public GameObject fixTarget;
    GameObject RescuingTarget, HealingTarget;

    RaycastHit hit;
    Collider[] hitObjects = null;
    BoxCollider boxCollider;

    bool justEnterEvent = true;
    public AudioSource source;
    public float audioTimer = 0.5f;
    RoomTracker recordRoom;
    #endregion

    #region InteractWithItem
    [Header("Interact Item")]
    public float DampPosSpeed = 0.2f;
    public float DampRotSpeed = 0.2f;
    [HideInInspector]
    public Interact_SO CurrentInteractObject;
    [HideInInspector]
    public LocatorList locatorList = null;
    [HideInInspector]
    public Locators locators = null;
    int GrabOutIndex;
    bool IsGrabbing = true, isSitting = false;

    [HideInInspector]
    public Item_SO CurrentInteractItem;
    [HideInInspector]
    public bool HasInteract = false;

    public float resetTimer;
    float VelocityPosX;
    float VelocityPosZ;
    float timer;
    List<Transform> escWayponits = new List<Transform>();
    #endregion

    public void Awake()
    {
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        if (sources.Length > 0)
            source = sources[0];
        outline = GetComponent<Outline>();
        boxCollider = GetComponent<BoxCollider>();
        recordColliderSize = boxCollider.size;
        navAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        animator = GetComponent<Animator>();
        status.toDoList.Clear();
        recordRestTimer = restTime;
        recordRecoverTimer = recoverTime;
        recordSpeed = navAgent.speed;
        timer = resetTimer;
        #region StringRestrictedFiniteStateMachine
        Dictionary<string, List<string>> NPCDictionary = new Dictionary<string, List<string>>()
        {
            { "Patrol", new List<string> { "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Rest", new List<string> { "Patrol", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Event", new List<string> { "Patrol", "Rest", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Dispatch", new List<string> { "Patrol", "Rest", "Event", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Dodging", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Hiding", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Escaping", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "InteractWithItem", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Healing", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "GotAttacked", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Rescuing", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Idle", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Idle", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Fixing", "Anim", "OnFloor", "Death" } },
            { "Fixing", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Anim", "OnFloor", "Death" } },
            { "Anim", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "OnFloor", "Death" } },
            { "OnFloor", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "Death" } },
            { "Death", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping", "InteractWithItem", "Healing", "GotAttacked", "Rescuing", "Idle", "Fixing", "Anim", "OnFloor" } }
        };
        
        #endregion

        #region RightClickMenu
        //AddMenu("Move", "Move", false, ReadyForDispatch);
        //AddMenu("HideAll", "Hide All", false, TriggerHiding); //TODO NPC集体躲进去。Call一个方法，这个方法给GM发消息，带上自己在的房间，然后GM就会识别你带的房间，然后给本房间内所有的NPC发消息，让他们躲起来
        AddMenu("Interact", "Interact", true, ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos")
            | 1 << LayerMask.NameToLayer("RestingPos")
            | 1 << LayerMask.NameToLayer("TerminalPos")
            | 1 << LayerMask.NameToLayer("SwitchPos")
            | 1 << LayerMask.NameToLayer("Item")
            | 1 << LayerMask.NameToLayer("CBord")
            | 1 << LayerMask.NameToLayer("StoragePos"));
        #endregion

        m_fsm = new StringRestrictedFiniteStateMachine(NPCDictionary, "Patrol");

        if (inAnimState)
        {
            SwitchAnimState(true, AnimStateName);
        }
    }

    public void Start()
    {
        DetectRoom();
        navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f);
        currentTerminalPos = NewDestination();
        EventCenter.GetInstance().EventTriggered("GM.NPC.Add", this);
        Invoke("GenerateList", 0.00001f);
    }

    public void PlayAudio(string str)
    {
        AudioMgr.GetInstance().PlayAudio(source, str, 1f, false, null);
    }

    void GenerateList()
    {
        foreach (RoomTracker temp in GameManager.GetInstance().Rooms)
        {
            roomScripts.Add(temp);
        }
    }

    public void Update()
    {
        #region GameplayEvent
        if(IsPrisoner)
        {
            DetectRoom();
            if (currentRoomTracker.isEnemyDetected())
            {
                isEnemyEnter = true;
            }
            if (!currentRoomTracker.isEnemyDetected() && isEnemyEnter)
            {
                if(CurrentInteractObject != null)
                    CurrentInteractObject.IsInteracting = false;
                EventCenter.GetInstance().DiaEventTrigger("01_PrisonerSafe");
                IsPrisoner = false;
            }
        }

        if (Flashing)
        {
            AlwaysOutline = true;
            SetOutline(true);
            if (outline.OutlineWidth - Time.deltaTime <= 0f)
                IsIncreaseing = true;
            else if (outline.OutlineWidth + Time.deltaTime >= OutlineWidth)
                IsIncreaseing = false;

            if (IsIncreaseing)
            {
                outline.OutlineWidth += Time.deltaTime * FlashingSpeed;
            }
            else
            {
                outline.OutlineWidth -= Time.deltaTime * FlashingSpeed;
            }
        }
        else
        {
            AlwaysOutline = false;
        }
        #endregion
        if (m_fsm.GetCurrentState() != "Anim")
        {
            audioTimer -= Time.deltaTime;
            CheckEvent();
            AddStopMenu();
            if (navAgent.isOnOffMeshLink && !MoveAcrossNavMeshesStarted)
            {
                StartCoroutine(MoveAcrossNavMeshLink());
                MoveAcrossNavMeshesStarted = true;
            }
        }       
        #region StringRestrictedFiniteStateMachine Update
        switch (m_fsm.GetCurrentState())
        {
            case "Patrol":
                DetectRoom();
                if (currentRoomTracker != null)
                {
                    if (!currentRoomTracker.isEnemyDetected())
                    {
                        restTime -= Time.deltaTime;
                    }
                }
                if (restTime > 0)
                {                
                    Dispatch(currentTerminalPos);
                    GenerateNewDestination();
                    TriggerDodging();
                }
                else
                {
                    if (navAgent.enabled)
                        navAgent.ResetPath();
                    recoverTime = recordRecoverTimer;
                    m_fsm.ChangeState("Idle");
                }
                break;
            case "Idle":
                recoverTime -= Time.deltaTime * status.currentStamina / 100;
                TriggerDodging();
                animator.Play("Idle", 0);
                if (recoverTime <= 0)
                {
                    BackToPatrol();
                }
                break;
            case "Rest":
                if (CurrentInteractObject != null)
                {
                    switch (CurrentInteractObject.type)
                    {
                        case Interact_SO.InteractType.Bed:
                            if(status.currentHealth >= status.getUpHealth && status.currentStamina == status.maxStamina)
                            {
                                CurrentInteractObject.NPCInteractFinish(CurrentInteractObject);
                            }                          
                            break;
                        default:
                            break;
                    }
                }
                break;
            case "Dispatch":
                CompleteDispatching();
                break;
            case "Anim":
                break;
            case "Event":
                Event();
                ReachDestination();
                FacingEachOther();
                break;
            case "Dodging":
                Dodging();
                break;
            case "Hiding":
                Hiding();
                break;
            case "Escaping":
                Escaping();
                CompleteEscaping();
                break;
            case "InteractWithItem":
                PlayGetInAnim();
                break;
            case "Healing":
                if (HealingTarget != null)
                {
                    HealOther();
                }
                break;
            case "GotAttacked":
                if (navAgent.enabled)
                    navAgent.ResetPath();
                break;
            case "Rescuing":
                RescuingProcess();
                break;
            case "Fixing":
                Fixing();
                break;
            case "OnFloor":
                if (Distance() < restDistance && !isSitting)
                {
                    animator.Play("Sitting On Floor", 0);
                    if (MenuContains("Interact") >= 0)
                    {
                        RemoveAndInsertMenu("Interact", "Leave", "Leave", false, LeaveFloor);
                    }
                    isSitting = true;
                }
                hitObjects = Physics.OverlapSphere(transform.position, alertRadius, needDodged);
                if (hitObjects.Length != 0 && isSitting)
                {
                    animator.Play("Sitting Off Floor", 0);
                    isSitting = false;
                }
                break;
            default:
                break;
        }
        #endregion
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

    void AddStopMenu()
    {
        if (m_fsm.GetCurrentState() == "Hiding"
    || m_fsm.GetCurrentState() == "Escaping"
    || m_fsm.GetCurrentState() == "InteractWithItem"
    || m_fsm.GetCurrentState() == "Healing"
    || m_fsm.GetCurrentState() == "Fixing")
        {
            if (Distance() > restDistance + 2)
            {
                if (MenuContains("Stop") >= 0)
                    return;
                else
                {
                    InsertMenu(rightClickMenus.Count, "Stop", "Stop", false, Stop);
                }
            }
        }
        else if (MenuContains("Stop") >= 0)
        {
            RemoveMenu("Stop");
        }
    }

    public void Stop(object obj)
    {
        if (navAgent.enabled)
        {
            if(locatorList != null)
                locatorList.npc = null;
            CurrentInteractObject = null;
            RescuingTarget = null;
            HealingTarget = null;
            CurrentInteractItem = null;
            fixTarget = null;
            fixTargetTransform = null;
            locatorList = null;
            boxCollider.isTrigger = false;
            IsInteracting = false;
            switch (status.CarryItem)
            {
                case Item_SO.ItemType.None:
                    break;
                case Item_SO.ItemType.MedicalKit:
                    break;
                default:
                    break;
            }
            RemoveMenu("Stop");
            BackToPatrol();
                    if (MenuContains("Interact") >= 0)
            return;
        else
            AddMenu("Interact", "Interact", true, ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos")
            | 1 << LayerMask.NameToLayer("RestingPos")
            | 1 << LayerMask.NameToLayer("TerminalPos")
            | 1 << LayerMask.NameToLayer("SwitchPos")
            | 1 << LayerMask.NameToLayer("Item")
            | 1 << LayerMask.NameToLayer("CBord")
            | 1 << LayerMask.NameToLayer("StoragePos"));
        }
    }

    public void SwitchAnimState(bool inState, string animName = "")
    {
        inAnimState = inState;
        AnimStateName = animName;
        if (inState)
        {
            if(navAgent.isOnNavMesh)
                navAgent.ResetPath();
            CurrentInteractObject = null;
            CurrentInteractItem = null;
            RescuingTarget = null;
            HealingTarget = null;
            fixTarget = null;
            fixTargetTransform = null;
            locatorList = null;
            if(NeedRemoveMenu)
                RemoveAllMenu();
            m_fsm.ChangeState("Anim");
            animator.Play(animName, 0);
        }
        else
        {
            navAgent.enabled = true;
            boxCollider.isTrigger = false;
            HasInteract = false;
            DetectRoom();
            BackToPatrol();
            RemoveAllMenu();
            AddMenu("Interact", "Interact", true, ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos")
    | 1 << LayerMask.NameToLayer("RestingPos")
    | 1 << LayerMask.NameToLayer("TerminalPos")
    | 1 << LayerMask.NameToLayer("SwitchPos")
    | 1 << LayerMask.NameToLayer("Item")
    | 1 << LayerMask.NameToLayer("CBord")
    | 1 << LayerMask.NameToLayer("StoragePos"));
            switch (status.CarryItem)
            {
                case Item_SO.ItemType.None:
                    break;
                case Item_SO.ItemType.MedicalKit:
                    InsertMenu(rightClickMenus.Count, "Heal", "Heal", true, Heal, 1 << LayerMask.NameToLayer("NPC"));
                    break;
                default:
                    break;
            }
        }
    }

    #region Move
    public void BackToPatrol(object obj = null)
    {
        TalkingStop();
        restTime = recordRestTimer;
        navAgent.speed = recordSpeed;
        currentTerminalPos = NewDestination();
        navAgent.speed *= (status.currentStamina / 100) * 0.4f + 0.6f;
        m_fsm.ChangeState("Patrol");
        IsRandomTalking = false;

        if (MenuContains("Leave") >= 0)
        {
            RemoveMenu("Leave");
        }

        if (MenuContains("Interact") >= 0)
            return;
        else
        {
            AddMenu("Interact", "Interact", true, ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos")
            | 1 << LayerMask.NameToLayer("RestingPos")
            | 1 << LayerMask.NameToLayer("TerminalPos")
            | 1 << LayerMask.NameToLayer("SwitchPos")
            | 1 << LayerMask.NameToLayer("Item")
            | 1 << LayerMask.NameToLayer("CBord")
            | 1 << LayerMask.NameToLayer("StoragePos"));
        }
    }

    public float Distance()
    {
        float a = navAgent.destination.x - transform.position.x;
        float b = navAgent.destination.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        return c;
    }

    public Vector3 NewDestination()
    {
        Vector3 tempPos = Vector3.zero;
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

        if (navAgent.autoRepath && Mathf.Abs(c) < restDistance || !navAgent.CalculatePath(currentTerminalPos, path) || !navAgent.hasPath)
        {
            currentTerminalPos = NewDestination();
        }
    }

    public void LimpingChange(string type)
    {
        if (type == "Walk")
        {
            if (status.currentHealth <= limpingLimit)
            {
                animator.Play("Limping Walk", 0);
                if (audioTimer <= 0)
                {
                    PlayAudio("Walk Hard " + Random.Range(1, 7).ToString());
                    audioTimer = 1.2f;
                }
            }
            else
            {
                animator.Play("Walk", 0);
                if (audioTimer <= 0)
                {
                    PlayAudio("Walk Hard " + Random.Range(1, 7).ToString());
                    audioTimer = 0.8f;
                }
            }
        }
        else if (type == "Run")
        {
            if (status.currentHealth <= limpingLimit)
            {
                animator.Play("Limping Run", 0);
                if (audioTimer <= 0)
                {
                    PlayAudio("Run Hard " + Random.Range(1, 7).ToString());
                    audioTimer = 1f;
                }
            }
            else
            {
                animator.Play("Run", 0);
                if (audioTimer <= 0)
                {
                    PlayAudio("Run Hard " + Random.Range(1, 7).ToString());
                    audioTimer = 0.6f;
                }
            }
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
        Physics.Raycast(transform.position + new Vector3 (0, 3, 0), -transform.up * detectRay, out hit, 1 << LayerMask.NameToLayer("Room"));
        if(hit.collider.gameObject != null)
        {
            currentRoomTracker = hit.collider.gameObject.GetComponent<RoomTracker>();
        }
    }

    public void Dispatch(object newPos)
    {
        navAgent.SetDestination((Vector3)newPos);

        if (m_fsm.GetCurrentState() != "InteractWithItem")
        {
            if (navAgent.velocity.magnitude >= 0.1f || navAgent.isOnOffMeshLink)
            {
                if (m_fsm.GetCurrentState() == "Patrol")
                {
                    LimpingChange("Walk");
                }
                else
                {
                    LimpingChange("Run");
                }
            }
            else if (!navAgent.isOnOffMeshLink)
            {
                animator.Play("Idle", 0);
            }
        }
    }

    public void ReadyForDispatch(object newPos)
    {
        LimpingChange("Run");
        Debug.Log("Ready for Dispatch");
        navAgent.SetDestination((Vector3)newPos);
        m_fsm.ChangeState("Dispatch");
    }

    public void CompleteDispatching()
    {
        if (Distance() < restDistance)
        {
            if(navAgent.enabled)
                navAgent.ResetPath();
            BackToPatrol();
        }
    }
    #endregion

    #region Random Resting
    public void TriggerBedResting(GameObject obj)
    {
        if (!isRoomCalled)
        {
            ReceiveInteractCall(obj);
            isRoomCalled = true;
        }
    }

    public void TriggerRandomResting()
    {
        if (!isRoomCalled)
        {
            switch (Random.Range(0, 3))
            {
                case (0):
                    //Sitting Chair
                    if (currentRoomTracker != null)
                    {
                        foreach (var item in currentRoomTracker.RestingPos())
                        {
                            Interact_SO interact_SO = item.GetComponent<Interact_SO>();
                            switch (interact_SO.type)
                            {
                                case Interact_SO.InteractType.Chair:
                                    ReceiveInteractCall(item.gameObject);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No CurrentRoomTracker" + gameObject);
                    }
                    break;
                case (1):
                    //Sitting on the floor
                    Dispatch(NewDestination());                   
                    m_fsm.ChangeState("OnFloor");
                    break;
                case (2):
                    //Patrol
                    BackToPatrol();
                    break;
                default:
                    break;
            }
            isRoomCalled = true;
        }
    }

    void LeaveFloor(object obj)
    {
        animator.Play("Sitting Off Floor", 0);
        RemoveMenu("Leave");
    }

    void SittingOffFloor()
    {
        restTime = recordRestTimer;
        navAgent.speed = recordSpeed;
        currentTerminalPos = NewDestination();
        navAgent.speed *= (status.currentStamina / 100) * 0.4f + 0.6f;
        if (MenuContains("Interact") < 0)
        {
            AddMenu("Interact", "Interact", true, ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos")
            | 1 << LayerMask.NameToLayer("RestingPos")
            | 1 << LayerMask.NameToLayer("TerminalPos")
            | 1 << LayerMask.NameToLayer("SwitchPos")
            | 1 << LayerMask.NameToLayer("Item")
            | 1 << LayerMask.NameToLayer("CBord")
            | 1 << LayerMask.NameToLayer("StoragePos"));
        }
        if (MenuContains("Leave") >= 0)
        {
            RemoveMenu("Leave");
        }
        IsRandomTalking = false;
        m_fsm.ChangeState("Patrol");
    }
    #endregion

    #region Hiding
    public void TriggerHiding(object obj = null)
    {
        if(navAgent.enabled != false)
        {
            if (navAgent.enabled)
                navAgent.ResetPath();
            navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
            m_fsm.ChangeState("Hiding");
        }
    }

    void Hiding()
    {       
        bool isEmpty = false;
        float minDistance = Mathf.Infinity;
        foreach (GameObject temp in currentRoomTracker.HiddenPos())
        {
            Interact_SO hpos = temp.GetComponent<Interact_SO>();
            for (int i = 0; i < hpos.Locators.Count; i++)
            {
                if (hpos.Locators[i].npc != null)
                    continue;
                isEmpty = true;
                float a = hpos.Locators[i].Locator.position.x - transform.position.x;
                float b = hpos.Locators[i].Locator.position.z - transform.position.z;
                float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
                float distance = Mathf.Abs(c);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    CurrentInteractObject = hpos;
                    currentTerminalPos = hpos.Locators[i].Locator.position;
                    locatorList = hpos.Locators[i];
                }
            }
        }
        if (!isEmpty)
        {
            CurrentInteractObject = null;
            locatorList = null;
            BackToPatrol();
        }
        Dispatch(currentTerminalPos);
        if (Distance() < restDistance || !navAgent.enabled)
        {
            HasInteract = false;
            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
            m_fsm.ChangeState("InteractWithItem");
        }
    }
    #endregion

    #region Event
    public void TriggerEvent()
    {
        if (navAgent.enabled)
            navAgent.ResetPath();
        m_fsm.ChangeState("Event");
        justEnterEvent = true;
    }

    public void RandomTalk()
    {
        if (status.toDoList.Count > 0)
        {
            EventSO evt = status.toDoList[0];
            for (int i = 0; i < evt.NPCTalking.Count; i++)
            {
                for (int a = 0; a < evt.NPCTalking[i].moveToClasses.Count; a++)
                {
                    if (evt.NPCTalking[i].moveToClasses[a].Obj == gameObject)
                    {
                        int index = 0;
                        int b = 0;
                        do
                        {
                            index++;
                            b = Random.Range(0, evt.NPCTalking[i].moveToClasses.Count);
                        } while (b == a && index < 50);
                        if (index > 50)
                        {
                            TalkingPos = currentRoomTracker.cameraLists[currentRoomTracker.CurrentCameraIndex].roomCamera.gameObject.transform.position;
                        }
                        else
                        {
                            TalkingPos = evt.NPCTalking[i].moveToClasses[b].Obj.transform.position;
                        }
                    }
                }
            }
        }
        IsRandomTalking = true;
        switch (Random.Range(0, 3))
        {
            case (0):
                animator.Play("Talking1", 0);
                break;
            case (1):
                animator.Play("Talking2", 0);
                
                break;
            case (2):
                animator.Play("Talking3", 0);
                break;
            default:
                break;
        }
    }

    public void Talking1()
    {
        AudioMgr.GetInstance().PlayAudio(source, "Talking long", 0.5f, true, null);
    }

    public void Talking2()
    {
        AudioMgr.GetInstance().PlayAudio(source, "Talking_long_VG", 0.5f, true, null);
    }

    public void Talking3()
    {
        AudioMgr.GetInstance().PlayAudio(source, "Talking_long_VG", 0.5f, true, null);
    }

    public void TalkingStop()
    {
        source.Stop();
    }

    private void Event()
    {
        if(status.toDoList.Count > 0)
        {
            EventSO evt = status.toDoList[0];
            if (justEnterEvent)
            {
                justEnterEvent = false;
                switch (evt.doingWithNPC)
                {
                    case DoingWithNPC.Talking:
                        for (int a = 0; a < evt.NPCTalking.Count; a++)
                        {
                            for (int b = 0; b < evt.NPCTalking[a].moveToClasses.Count; b++)
                            {
                                if (evt.NPCTalking[a].moveToClasses[b].Obj == gameObject)
                                {
                                    Dispatch(evt.NPCTalking[a].moveToClasses[b].MoveTO.position);
                                    LimpingChange("Run");
                                    IsInteracting = true;
                                }
                            }
                        }
                        break;
                    case DoingWithNPC.MoveTo:
                        for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                        {
                            if (evt.NPCWayPoint[a].Obj == gameObject)
                            {
                                Dispatch(evt.NPCWayPoint[a].MoveTO.position);
                                LimpingChange("Run");
                                IsInteracting = true;
                            }
                        }
                        break;
                    case DoingWithNPC.Patrol:
                        break;
                    case DoingWithNPC.Interact:
                        break;
                    case DoingWithNPC.AnimState:
                    default:
                        break;
                }
            }
            else
            {
                switch (evt.doingWithNPC)
                {
                    case DoingWithNPC.Talking:
                        for (int a = 0; a < evt.NPCTalking.Count; a++)
                        {
                            if(evt.NPCTalking[a].room.DiaPlay.currentGraph != evt.NPCTalking[a].Graph && evt.NPCTalking[a].room.WaitingGraph != evt.NPCTalking[a].Graph)
                            {
                                IsInteracting = false;
                                status.toDoList.Remove(evt);
                            }
                        }
                        break;
                    case DoingWithNPC.MoveTo:
                        break;
                    case DoingWithNPC.Patrol:
                        break;
                    case DoingWithNPC.Interact:
                        break;
                    case DoingWithNPC.AnimState:
                        break;
                    default:
                        break;
                }
            }
        }
        if (status.toDoList.Count <= 0)
        {
            IsInteracting = false;
            BackToPatrol();
        }
    }

    public void ReachDestination()
    {
        if (Distance() <= restDistance)
        {
            EventCenter.GetInstance().EventTriggered("GM.NPCArrive", status.npcName);
            if (navAgent.enabled)
                navAgent.ResetPath();
        }
        else if (navAgent.autoRepath)
        {
            //TODO: Cant Reach
            Debug.Log("I cant go there");
        }
    }
    #endregion

    #region Dodging
    public void TriggerDodging()
    {
        hitObjects = Physics.OverlapSphere(transform.position, alertRadius, needDodged);
        if (hitObjects.Length != 0)
        {
            navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
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
        if (navAgent.enabled != false)
        {
            if (navAgent.enabled)
                navAgent.ResetPath();
            navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
            escWayponits.Clear();
            m_fsm.ChangeState("Escaping");
        }
    }

    void Escaping()
    {
        Vector3 pos = Vector3.zero;
        foreach (var item in roomScripts)
        {
            if (item.isEnemyDetected())
                continue;
            foreach (var point in item.tempWayPoints)
            {
                if(!escWayponits.Contains(point))
                    escWayponits.Add(point);
            }
        }
        float minDistance = Mathf.Infinity;
        foreach (var wayPoint in escWayponits)
        {
            float a = wayPoint.position.x - transform.position.x;
            float b = wayPoint.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            float distance = Mathf.Abs(c);
            if (distance < minDistance)
            {
                minDistance = distance;
                pos = wayPoint.position;
            }
        }
        Dispatch(pos);
    }

    public void CompleteEscaping()
    {
        if (Distance() < restDistance)
        {
            if (navAgent.enabled)
                navAgent.ResetPath();
            BackToPatrol();
        }
    }
    #endregion

    #region Healing
    public void Heal(object obj)
    {
        if (navAgent.enabled)
            navAgent.ResetPath();
        GameObject gameObj = (GameObject)obj;
        NpcController NPC = gameObj.GetComponent<NpcController>();
        m_fsm.ChangeState("Healing");
        if (NPC != this)
        {
            //Heal Other
            HealingTarget = gameObj;
            navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
            Dispatch(HealingTarget.transform.position);

        }
        else
        {
            //Heal Self
            switch (Random.Range(0, 2))
            {
                case (0):
                    animator.Play("Stand Heal Self", 0);
                    break;
                case (1):
                    animator.Play("Squat Heal Self", 0);
                    break;
                default:
                    break;
            }
        }
    }

    void HealOther()
    {
        float a = HealingTarget.transform.position.x - transform.position.x;
        float b = HealingTarget.transform.position.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        if (c < restDistance && HealingTarget != null)
        {
            bool Damping = false;
            Vector3 dir = (HealingTarget.transform.position - transform.position).normalized;
            dir.y = 0;
            Quaternion rotation = Quaternion.LookRotation(dir);

            if (Quaternion.Angle(transform.rotation, rotation) >= 1)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, DampRotSpeed);
                Damping = true;
            }
            if (Damping)
            {
                Debug.Log("Damping");
            }
            else if (HealingTarget.GetComponent<NpcController>().m_fsm.GetCurrentState() == "Rest")
            {
                animator.Play("Squat Heal Other", 0);
                if (isPrristNeedHeal)
                {
                    HealingTarget.GetComponent<NpcController>().navAgent.ResetPath();
                    HealingTarget.GetComponent<NpcController>().AnimStateName = "Talking1";
                    HealingTarget.GetComponent<NpcController>().inAnimState = true;
                    HealingTarget.GetComponent<NpcController>().IsInteracting = true;
                    HealingTarget.GetComponent<NpcController>().FacingEachOtherCoro(transform);
                    navAgent.ResetPath();
                    AnimStateName = "Talking2";
                    inAnimState = true;
                    IsInteracting = true;
                    FacingEachOtherCoro(HealingTarget.transform);
                    isPrristNeedHeal = false;
                    currentRoomTracker.PlayingDialogue(HealPriDia);
                }
            }
            else
            {
                if (HealingTarget.GetComponent<NpcController>().navAgent.enabled)
                    HealingTarget.GetComponent<NpcController>().navAgent.ResetPath();
                animator.Play("Stand Heal Other", 0);
            }
        }
        else if (navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
        {
            LimpingChange("Run");
            Dispatch(HealingTarget.transform.position);
        }
        else if (!navAgent.isOnOffMeshLink && !HasInteract)
        {
            animator.Play("Idle", 0);
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                HealingTarget = null;
                timer = resetTimer;
                //TODO: Cant Reach Destination
                Debug.Log("Cant Reach Destination");
                BackToPatrol();
            }
        }
    }

    #endregion

    #region Got Attacked
    public void GotHurt()
    {
        SetOutline(false);
        animator.Play("Got Hurt", 0);
        PlayAudio("Hurt Sound " + Random.Range(1, 7).ToString());
    }

    void Death()
    {
        Flashing = false;
        if(outline != null)
        {
            outline.OutlineWidth = 0f;
            SetOutline(false);
        }
        boxCollider.enabled = false;
        animator.Play("Death", 0);
        if(navAgent.enabled)
            navAgent.ResetPath();   
        status.isStruggling = false;
        gameObject.layer = LayerMask.NameToLayer("Dead");
        m_fsm.ChangeState("Death");

        ResMgr.GetInstance().LoadAsync<GameObject>("DeadBox", (x) => {
            x.transform.position = transform.position;
            DeadBox deadBox = x.GetComponent<DeadBox>();
            deadBox.type = status.CarryItem;
            switch (status.CarryItem)
            {
                case Item_SO.ItemType.None:
                    Destroy(x);
                    break;
                case Item_SO.ItemType.MedicalKit:
                    deadBox.HPRecovery = status.healAmount;
                    break;
                case Item_SO.ItemType.RepairedPart:
                    break;
                case Item_SO.ItemType.Key:                 
                    deadBox.code = status.code;
                    break;
                default:
                    break;
            }
        });
    }

    public void CurrentObjectAnimPlay(object obj)
    {
        if(CurrentInteractObject != null)
        {
            CurrentInteractObject.NPCInteractFinish(obj);
        }
    }
    #endregion

    #region Rescuing
    public void TriggerRescuing(object obj)
    {
        RescuingTarget = (GameObject)obj;
        if (RescuingTarget != null)
        {
            if (!navAgent.enabled)
                navAgent.enabled = true;            
            Debug.Log("I am coming!");
            Dispatch(RescuingTarget.transform.position);
            navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
            m_fsm.ChangeState("Rescuing");
        }
    }

    public void RescuingProcess()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, alertRadius, 1 << LayerMask.NameToLayer("NPC") | 1 << LayerMask.NameToLayer("Dead"));

        foreach (var item in hits)
        {
            if (item.gameObject == RescuingTarget)
            {
                isBlocked = Physics.Linecast(transform.position, item.transform.position, canBlocked);
                Vector3 direction = (item.transform.position - transform.position).normalized;
                float targetAngle = Vector3.Angle(transform.forward, direction);
                inAngle = targetAngle <= discoverAngle / 2 ? true : false;

                NpcController target = item.GetComponent<NpcController>();

                if (!navAgent.autoRepath && inAngle && !isBlocked)
                {
                    if (target.status.currentHealth <= 0)
                    {
                        Debug.Log("Too Late");
                        RescuingTarget = null;
                        BackToPatrol();
                    }
                    else if (Distance() <= restDistance + 0.5f)
                    {
                        Debug.Log("Got U");
                        target.status.isStruggling = false;
                        RescuingTarget = null;
                        BackToPatrol();
                    }
                }
                else if (navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
                {
                    LimpingChange("Run");
                }
                else if (!navAgent.isOnOffMeshLink && !HasInteract)
                {
                    animator.Play("Idle", 0);
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        RescuingTarget = null;
                        timer = resetTimer;
                        //TODO: Cant Reach Destination
                        Debug.Log("Cant Reach Destination");
                        BackToPatrol();
                    }
                }
            }
        }
    }

    public void Alive()
    {
        restTime = recordRestTimer;
        navAgent.speed = recordSpeed;
        currentTerminalPos = NewDestination();
        IsRandomTalking = false;
        m_fsm.ChangeState("Patrol");
    }
    #endregion

    #region Fixing
    public void TriggerFixing()
    {
        HasInteract = false;
        navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
        m_fsm.ChangeState("Fixing");
    }

    void Fixing()
    {
        if (fixTarget != null && fixTargetTransform != null)
        {
            float a = fixTargetTransform.position.x - transform.position.x;
            float b = fixTargetTransform.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            if (c < restDistance + 1 || !navAgent.enabled)
            {
                IsInteracting = true;
                bool Damping = false;

                Vector3 TraPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 IntPos = new Vector3(fixTargetTransform.position.x, 0, fixTargetTransform.position.z);

                if ((TraPos - IntPos).sqrMagnitude >= 0.001)
                {
                    transform.position = new Vector3(Mathf.SmoothDamp(transform.position.x, fixTargetTransform.position.x, ref VelocityPosX, DampPosSpeed)
                    , transform.position.y
                    , Mathf.SmoothDamp(transform.position.z, fixTargetTransform.position.z, ref VelocityPosZ, DampPosSpeed));
                    Damping = true;
                }

                if (Quaternion.Angle(transform.rotation, fixTargetTransform.rotation) >= 0.2)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, fixTargetTransform.rotation, DampRotSpeed);
                    Damping = true;
                }

                if (Damping)
                {
                    Debug.Log("Damping");
                }
                else if (!HasInteract)
                {
                    if (fixTarget.GetComponent<DoorController>() != null)
                    {
                        Debug.Log("Fixing Door");
                        DoorController door = fixTarget.GetComponent<DoorController>();
                        door.isFixing = true;
                        door.currentHealth += status.fixRate * Time.deltaTime;
                        foreach (var item in door.Locators)
                        {
                            if (item.npc != null)
                                continue;
                            item.npc = this;
                        }
                        animator.Play("Squat Terminal", 0);

                        if (door.currentHealth >= door.maxHealth)
                        {
                            if (door.cBord != null)
                            {
                                if (door.cBord.GetComponent<CBordPos>().isPowerOff)
                                {
                                    Debug.Log("Need Fix CBord ASAP");
                                    fixTarget = door.cBord.gameObject;
                                    fixTargetTransform = door.cBord.Locators[0].Locator;
                                    IsInteracting = false;
                                    foreach (var item in door.Locators)
                                    {
                                        item.npc = null;
                                    }
                                    RemoveMenu("Interact");
                                    AddStopMenu();
                                    Dispatch(door.cBord.Locators[0].Locator.position);
                                }
                                else
                                {
                                    Debug.Log("Fixed Door");
                                    foreach (var item in door.Locators)
                                    {
                                        item.npc = null;
                                    }
                                    door.RemoveAndInsertMenu("Repair", "SwitchStates", "Lock", false, door.SwtichStates, 1 << LayerMask.GetMask("Door"));
                                    IsInteracting = false;
                                    fixTarget = null;
                                    fixTargetTransform = null;
                                    HasInteract = true;
                                    BackToPatrol();
                                }
                            }                         
                            else
                            {
                                Debug.Log("Fixed Door");
                                foreach (var item in door.Locators)
                                {
                                    item.npc = null;
                                }
                                door.RemoveAndInsertMenu("Repair", "SwitchStates", "Lock", false, door.SwtichStates, 1 << LayerMask.GetMask("Door"));
                                IsInteracting = false;
                                fixTarget = null;
                                fixTargetTransform = null;
                                HasInteract = true;
                                BackToPatrol();
                            }
                        }
                    }
                    else if (fixTarget.GetComponent<CBordPos>() != null)
                    {
                        Debug.Log("Fixing CBord");
                        CBordPos cBord = fixTarget.GetComponent<CBordPos>();
                        cBord.isFixing = true;
                        cBord.currentHealth += status.fixRate * Time.deltaTime;
                        cBord.Locators[0].npc = this;
                        animator.Play("Squat Terminal", 0);
                        if (!cBord.isPowerOff)
                        {
                            Debug.Log("Fixed CBord");
                            cBord.RemoveAndInsertMenu("Repair", "Operate", "Operate", true,cBord.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                            if (!cBord.isLocked)
                            {
                                if(cBord.door != null)
                                {
                                    DoorController door = cBord.door.GetComponent<DoorController>();
                                    door.RemoveAndInsertMenu("Repair", "SwitchStates", "Lock", false, door.SwtichStates, 1 << LayerMask.GetMask("Door"));
                                }                             
                            }
                            cBord.Locators[0].npc = null;                          
                            fixTarget = null;
                            fixTargetTransform = null;
                            cBord.isFixing = false;
                            HasInteract = true;
                            IsInteracting = false;
                            if (!navAgent.enabled)
                            {
                                navAgent.enabled = true;
                            }
                            BackToPatrol();
                        }
                    }
                }         
            }
            else if (navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
            {
                LimpingChange("Run");
                if (fixTarget.GetComponent<CBordPos>() != null)
                {
                    if(fixTarget.GetComponent<CBordPos>().Locators[0].npc != null)
                    {
                        BackToPatrol();
                    }
                }
                else if (fixTarget.GetComponent<DoorController>() != null)
                {
                    DoorController door = fixTarget.GetComponent<DoorController>();
                    foreach (var item in door.Locators)
                    {
                        if (item.npc != null)
                        {
                            BackToPatrol();
                        }
                    }
                }

            }
            else if (!navAgent.isOnOffMeshLink && !HasInteract)
            {
                animator.Play("Idle", 0);
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    fixTarget = null;
                    fixTargetTransform = null;
                    timer = resetTimer;
                    //TODO: Cant Reach Destination
                    Debug.Log("Cant Reach Destination");
                    BackToPatrol();
                }
            }
        }
    }
    #endregion

    #region Receive Call
    public void ReceiveInteractCall(object obj)
    {
        Flashing = false;
        outline.OutlineWidth = 0;
        SetOutline(false);
        if (navAgent.enabled)
        {
            GameObject gameObj = (GameObject)obj;
            if (gameObj.GetComponent<Item_SO>() != null)
            {
                ReceiveItemCall(gameObj);
            }
            else
            {          
                Interact_SO item = gameObj.GetComponent<Interact_SO>();
                StoragePos storge = item as StoragePos;
                if (item != null)
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
                    navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
                    RemoveMenu("Interact");
                    m_fsm.ChangeState("InteractWithItem");
                }
            }
        }   
    }

    public void ReceiveItemCall(object obj)
    {
        if (status.CarryItem == Item_SO.ItemType.None)
        {
            GameObject gameObj = (GameObject)obj;
            Item_SO item = gameObj.GetComponent<Item_SO>();

            if (item != null)
            {
                Debug.Log("Receive Item Call");
            }

            CurrentInteractObject = null;
            CurrentInteractItem = item;
            HasInteract = false;
            Dispatch(CurrentInteractItem.gameObject.transform.position);
            navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
            m_fsm.ChangeState("InteractWithItem");
        }
        else
        {
            Debug.Log("Leave it alone, dont be too greedy");
        }
    }

    public void InteractMoment()
    {
        if (CurrentInteractObject != null)
        {
            CurrentInteractObject.NPCInteract(0);
            PlayAudio("light_lever");
        }
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
            if (status.CarryItem == Item_SO.ItemType.None)
            {
                Debug.Log(status.npcName + ": I don't have Item to store");
                return;
            }
        }
        HasInteract = false;
        Dispatch(Pos);
        navAgent.speed *= ((status.currentStamina / 100) * 0.4f + 0.6f) * boostSpeed;
        m_fsm.ChangeState("InteractWithItem");
    }
    #endregion

    #region Play Animation
    void PlayGetInAnim()
    {
        if (CurrentInteractObject != null)
        {
            float a = locatorList.Locator.position.x - transform.position.x;
            float b = locatorList.Locator.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            if (c < restDistance || !navAgent.enabled)
            {
                boxCollider.isTrigger = true;
                IsInteracting = true;
                navAgent.enabled = false;
                bool Damping = false;

                Vector3 TraPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 IntPos = new Vector3(locatorList.Locator.position.x, 0, locatorList.Locator.position.z);

                if ((TraPos - IntPos).sqrMagnitude >= 0.01f)
                {
                    transform.position = new Vector3(Mathf.SmoothDamp(transform.position.x, locatorList.Locator.position.x, ref VelocityPosX, DampPosSpeed)
                    , transform.position.y
                    , Mathf.SmoothDamp(transform.position.z, locatorList.Locator.position.z, ref VelocityPosZ, DampPosSpeed));
                    Damping = true;
                }

                if (Quaternion.Angle(transform.rotation, locatorList.Locator.rotation) >= 0.2f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, locatorList.Locator.rotation, DampRotSpeed);
                    Damping = true;
                }

                if (Damping)
                {
                    //Debug.Log("Damping");
                }
                else if (!HasInteract)
                {
                    switch (CurrentInteractObject.type)
                    {
                        case Interact_SO.InteractType.ServerPos:
                            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
                            animator.Play("UnlockTerminal", 0);
                            HasInteract = true;
                            break;
                        case Interact_SO.InteractType.Locker:
                            if (IsPrisoner)
                            {
                                EventCenter.GetInstance().DiaEventTrigger("01_HideIn");
                                CurrentInteractObject.IsInteracting = true;
                            }
                            CurrentInteractObject.NPCInteract(0);
                            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
                            animator.Play("Get In Locker", 0);
                            HasInteract = true;
                            break;
                        case Interact_SO.InteractType.Box:
                            CurrentInteractObject.NPCInteract(0);
                            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
                            animator.Play("Get In Box", 0);
                            HasInteract = true;
                            break;
                        case Interact_SO.InteractType.Bed:
                            CurrentInteractObject.NPCInteract(0);
                            foreach (var item in CurrentInteractObject.GetComponent<Interact_SO>().Locators)
                            {
                                item.npc = this;
                            }
                            if (locatorList.Locator.name == "locatorL")
                                animator.Play("Bed Left On", 0);
                            else
                                animator.Play("Bed Right On", 0);
                            HasInteract = true;
                            break;
                        case Interact_SO.InteractType.Chair:
                            CurrentInteractObject.NPCInteract(0);
                            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
                            animator.Play("Sitting On Chair");
                            HasInteract = true;
                            break;
                        case Interact_SO.InteractType.Terminal:
                            CurrentInteractObject.NPCInteract(0);
                            animator.Play("UnlockTerminal", 0);
                            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
                            HasInteract = true;
                            break;
                        case Interact_SO.InteractType.Switch:
                            if (IsPrisoner)
                                EventCenter.GetInstance().DiaEventTrigger("01_CloseDoor");
                            animator.Play("Stand Switch", 0);
                            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
                            HasInteract = true;
                            break;
                        case Interact_SO.InteractType.Storage:
                            HasInteract = true;
                            StoragePos sto = CurrentInteractObject.GetComponent<StoragePos>();                           
                            if (IsGrabbing)
                            {
                                animator.Play("Grab Item");
                                if (status.CarryItem != Item_SO.ItemType.Key)
                                {
                                    if (status.CarryItem != Item_SO.ItemType.None)
                                    {
                                        Item_SO.ItemType GrabOutItem = sto.StorageItem[GrabOutIndex];
                                        Item_SO.ItemType PutInItem = status.CarryItem;
                                        RemoveAllMenu();
                                        AddMenu("Interact", "Interact", true, ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos")
                                                                                                        | 1 << LayerMask.NameToLayer("RestingPos")
                                                                                                        | 1 << LayerMask.NameToLayer("TerminalPos")
                                                                                                        | 1 << LayerMask.NameToLayer("SwitchPos")
                                                                                                        | 1 << LayerMask.NameToLayer("Item")
                                                                                                        | 1 << LayerMask.NameToLayer("CBord")
                                                                                                        | 1 << LayerMask.NameToLayer("StoragePos"));
                                        status.CarryItem = GrabOutItem;
                                        switch (GrabOutItem)
                                        {
                                            case Item_SO.ItemType.None:
                                                break;
                                            case Item_SO.ItemType.MedicalKit:
                                                InsertMenu(rightClickMenus.Count, "Heal", "Heal", true, Heal, 1 << LayerMask.NameToLayer("NPC"));
                                                status.healAmount = 70;
                                                Debug.Log("Got MedicalKit");
                                                break;
                                            case Item_SO.ItemType.RepairedPart:
                                                break;
                                            default:
                                                break;
                                        }
                                        sto.StorageItem.Add(PutInItem);
                                        sto.StorageItem.Remove(GrabOutItem);
                                        sto.UpdateMenu();
                                    }
                                    else
                                    {
                                        status.CarryItem = sto.StorageItem[GrabOutIndex];
                                        switch (sto.StorageItem[GrabOutIndex])
                                        {
                                            case Item_SO.ItemType.None:
                                                break;
                                            case Item_SO.ItemType.MedicalKit:
                                                InsertMenu(rightClickMenus.Count, "Heal", "Heal", true, Heal, 1 << LayerMask.NameToLayer("NPC"));
                                                status.healAmount = 70;
                                                Debug.Log("Got MedicalKit");
                                                break;
                                            case Item_SO.ItemType.RepairedPart:
                                                break;
                                            default:
                                                break;
                                        }
                                        sto.StorageItem.Remove(sto.StorageItem[GrabOutIndex]);
                                        sto.UpdateMenu();
                                    }
                                }
                                else
                                    Debug.Log("I cant give up this key");                               
                            }
                            else
                            {
                                animator.Play("Grab Item");
                                if (status.CarryItem != Item_SO.ItemType.None)
                                {
                                    if (status.CarryItem != Item_SO.ItemType.Key)
                                    {
                                        if (sto.StorageItem.Count + 1 <= sto.MaxStorage)
                                        {                                     
                                            sto.StorageItem.Add(status.CarryItem);
                                            sto.UpdateMenu();
                                            switch (status.CarryItem)
                                            {
                                                case Item_SO.ItemType.MedicalKit:
                                                    if (MenuContains("Heal") >= 0)
                                                    {
                                                        RemoveMenu("Heal");
                                                    }
                                                    status.healAmount = 0;
                                                    break;
                                                case Item_SO.ItemType.RepairedPart:
                                                    break;
                                                case Item_SO.ItemType.Key:
                                                    break;
                                                default:
                                                    break;
                                            }
                                            status.CarryItem = Item_SO.ItemType.None;
                                        }
                                        else
                                        {
                                            Debug.Log("It is full");
                                        }
                                    }
                                    else
                                        Debug.Log("Cant store this");
                                }
                                else
                                    Debug.Log("I dont have any to store");
                            }
                            break;
                        case Interact_SO.InteractType.CBoard:
                            CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
                            CBordPos cBord = CurrentInteractObject as CBordPos;
                            if (cBord != null)
                            {
                                if (cBord.isPowerOff)
                                {
                                    fixTarget = CurrentInteractObject.gameObject;
                                    fixTargetTransform = cBord.Locators[0].Locator;
                                    navAgent.enabled = true;
                                    boxCollider.isTrigger = false;
                                    Dispatch(cBord.Locators[0].Locator.position);
                                    CurrentInteractObject = null;
                                    m_fsm.ChangeState("Fixing");
                                }
                                else
                                {
                                    if (cBord.isLocked)
                                    {
                                        animator.Play("UnlockTerminal", 0);
                                        CurrentInteractObject.NPCInteract(0);
                                        HasInteract = true;
                                    }
                                    else
                                    {
                                        animator.Play("UnlockTerminal", 0);
                                        HasInteract = true;
                                    }
                                }
                            }
                            else
                                Debug.LogWarning("No CBord", gameObject);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
            {
                LimpingChange("Run");
                if (locatorList.npc != null)
                {                      
                    BackToPatrol();
                }
            }
            else if (!navAgent.isOnOffMeshLink)
            {
                animator.Play("Idle", 0);
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    CurrentInteractObject = null;
                    locatorList = null;
                    timer = resetTimer;
                    //TODO: Cant Reach Destination
                    Debug.Log("Cant Reach Destination");
                    BackToPatrol();
                }
            }
        }
        else if (CurrentInteractItem != null)
        {
            float a = CurrentInteractItem.transform.position.x - transform.position.x;
            float b = CurrentInteractItem.transform.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            if (c <= restDistance)
            {
                bool Damping = false;
                Vector3 dir = (CurrentInteractItem.transform.position - transform.position).normalized;
                dir.y = 0;
                Quaternion rotation = Quaternion.LookRotation(dir);

                if (Quaternion.Angle(transform.rotation, rotation) >= 1)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, DampRotSpeed);
                    Damping = true;
                }

                if (Damping)
                {
                    Debug.Log("Damping");
                }
                else if (!HasInteract)
                {
                    boxCollider.isTrigger = true;
                    IsInteracting = true;
                    navAgent.enabled = false;
                    HasInteract = true;
                    switch (CurrentInteractItem.type)
                    {
                        case Item_SO.ItemType.MedicalKit:
                            Debug.Log("Got MedicalKit");
                            animator.Play("Grab Item", 0);
                            break;
                        case Item_SO.ItemType.Key:
                            Debug.Log("Key");
                            animator.Play("Grab Item", 0);
                            break;
                        case Item_SO.ItemType.RepairedPart:
                            Debug.Log("RepairedPart");
                            animator.Play("Grab Item", 0);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
            {
                LimpingChange("Run");
            }
            else if (!navAgent.isOnOffMeshLink && !HasInteract)
            {
                animator.Play("Idle", 0);
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    CurrentInteractItem = null;
                    timer = resetTimer;
                    //TODO: Cant Reach Destination
                    Debug.Log("Cant Reach Destination");
                    BackToPatrol();
                }
            }
        }
    }

    public void PlayGetOutAnim(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        Interact_SO item = gameObj.GetComponent<Interact_SO>();
        switch (item.type)
        {
            case Interact_SO.InteractType.Locker:
                animator.Play("Get Out Locker", 0);
                transform.eulerAngles += new Vector3(0, 180, 0);              
                break;
            case Interact_SO.InteractType.Box:
                animator.Play("Get Out Box", 0);
                transform.eulerAngles += new Vector3(0, 180, 0);
                break;
            case Interact_SO.InteractType.Bed:
                if (locatorList.Locator.name == "locatorL")
                    animator.Play("Bed Left Off", 0);
                else
                    animator.Play("Bed Right Off", 0);
                break;
            case Interact_SO.InteractType.Chair:
                animator.Play("Sitting Off Chair");
                break;
            case Interact_SO.InteractType.Terminal:
                break;
            case Interact_SO.InteractType.Switch:
                break;
            default:
                break;
        }
        CurrentInteractObject = item;
    }

    public void CompleteGetInItemAction()
    {
        Debug.Log("Get In Animation Done");
        switch (CurrentInteractObject.type)
        {
            case Interact_SO.InteractType.Locker:
                boxCollider.size = new Vector3(0.001f, 0.001f, 0.001f);
                isSafe = true;
                if (IsPrisoner)
                    CurrentInteractObject.IsInteracting = true;
                break;
            case Interact_SO.InteractType.Box:
                isSafe = true;
                break;
            case Interact_SO.InteractType.Bed:
                break;
            case Interact_SO.InteractType.Chair:
                CurrentInteractObject.GetComponent<BoxCollider>().size = CurrentInteractObject.newColliderSize;
                CurrentInteractObject.GetComponent<BoxCollider>().center = CurrentInteractObject.newColliderCenter;
                boxCollider.size = new Vector3(0.001f, 0.001f, 0.001f);
                break;
            case Interact_SO.InteractType.Terminal:
                break;
            case Interact_SO.InteractType.Switch:
                break;
            case Interact_SO.InteractType.Storage:
                break;
            case Interact_SO.InteractType.CBoard:
                break;
            default:
                break;
        }
        CurrentInteractObject.Locators.Find((x) => (x == locatorList)).npc = this;
        m_fsm.ChangeState("Rest");
    }

    public void CompleteGetOutItemAction()
    {
        Debug.Log("Get Out Animation Done");
        boxCollider.isTrigger = false;
        IsInteracting = false;
        navAgent.enabled = true;
        if (CurrentInteractObject != null)
        {
            switch (CurrentInteractObject.type)
            {
                case Interact_SO.InteractType.ServerPos:
                    if(status.CarryItem == Item_SO.ItemType.RepairedPart)
                    {
                        ServerPos server = CurrentInteractObject as ServerPos;
                        server.isUnlocked = true;
                        if (server.RedLight != null && server.GreenLight != null)
                        {
                            foreach (var item in server.RedLight)
                            {
                                item.SetActive(false);
                            }
                            foreach (var item in server.GreenLight)
                            {
                                item.SetActive(true);
                            }
                        }
                        server.IsInteracting = true;
                        Debug.Log("Server unlocked");
                        status.CarryItem = Item_SO.ItemType.None;
                    }
                    else
                    {
                        Debug.Log("I dont have RepairedPart");
                    }
                    break;
                case Interact_SO.InteractType.Locker:
                    if(SendEventWhenGetOutLocker)
                    {
                        SendEventWhenGetOutLocker = false;
                        EventCenter.GetInstance().EventTriggered("01_03DiaPlay");
                    }
                    CurrentInteractObject.RemoveAndInsertMenu("Leave", "Hide In", "Hide In", true, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                    boxCollider.size = recordColliderSize;
                    CurrentInteractObject.IsInteracting = false;
                    isSafe = false;
                    break;
                case Interact_SO.InteractType.Box:
                    CurrentInteractObject.RemoveAndInsertMenu("Leave", "Hide In", "Hide In", true, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                    isSafe = false;
                    CurrentInteractObject.IsInteracting = false;
                    break;
                case Interact_SO.InteractType.Bed:
                    CurrentInteractObject.RemoveAndInsertMenu("Leave", "RestIn", "RestIn", true, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                    CurrentInteractObject.IsInteracting = false;
                    break;
                case Interact_SO.InteractType.Chair:
                    CurrentInteractObject.RemoveAndInsertMenu("Leave", "RestIn", "RestIn", true, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                    CurrentInteractObject.GetComponent<BoxCollider>().size = CurrentInteractObject.recordColliderSize;
                    CurrentInteractObject.GetComponent<BoxCollider>().center = CurrentInteractObject.recordColliderCenter;
                    boxCollider.size = recordColliderSize;
                    CurrentInteractObject.IsInteracting = false;
                    break;
                case Interact_SO.InteractType.Terminal:
                    CurrentInteractObject.RemoveAndInsertMenu("Leave", "Operate", "Operate", false, CurrentInteractObject.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                    CurrentInteractObject.IsInteracting = false;
                    if (status.CarryItem != Item_SO.ItemType.None)
                    {
                        Debug.Log("I dont have place to get this");                      
                        break;
                    }
                    else
                    {
                        TerminalPos terminal = CurrentInteractObject as TerminalPos;
                        if(terminal.code == "")
                        {
                            Debug.Log("There is no Key");
                        }
                        else
                        {
                            status.CarryItem = Item_SO.ItemType.Key;
                            status.code = terminal.code;
                            terminal.code = "";
                        }
                    }
                    break;
                case Interact_SO.InteractType.Switch:
                    CurrentInteractObject.IsInteracting = false;
                    break;
                case Interact_SO.InteractType.CBoard:
                    CurrentInteractObject.IsInteracting = false;
                    CBordPos cBord = CurrentInteractObject as CBordPos;
                    if (cBord != null)
                    {
                        if (!cBord.isPowerOff)
                        {
                            if (!cBord.isLocked)
                            {
                                Debug.Log("It is unlocked");
                            }
                            else
                            {
                                if (status.code != "")
                                {
                                    if (status.code == cBord.code)
                                    {
                                        Debug.Log("Right Key");
                                        status.CarryItem = Item_SO.ItemType.None;
                                        status.code = "";
                                        if(cBord.door != null)
                                        {
                                            DoorController door = cBord.door.GetComponent<DoorController>();
                                            door.currentHealth = door.maxHealth;
                                            door.isPowerOff = false;
                                            door.isLocked = false;
                                            door.AddMenu("SwitchStates", "Lock", false, door.SwtichStates, 1 << LayerMask.GetMask("Door"));
                                        }    
                                        if(cBord.swtich != null)
                                        {
                                            SwitchPos swtich = cBord.swtich.GetComponent<SwitchPos>();
                                            swtich.AddMenu("SwtichState", "Lock Door", true, swtich.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
                                        }                                       
                                        cBord.isLocked = false;
                                        GameManager.GetInstance().SetupStage(2);
                                        if (status.npcName == "Stephanus Lentinus")
                                        {
                                            PuEvt.NPCTalking[0].Graph = Graph1;
                                            PuEvt.NPCTalking[0].moveToClasses[1].Obj = gameObject;
                                            HighP.status.toDoList.Add(PuEvt);
                                            HighP.TriggerEvent();
                                            status.toDoList.Add(PuEvt);
                                            TriggerEvent();
                                        }
                                        else
                                        {
                                            PuEvt.NPCTalking[0].Graph = Graph1_5;
                                            PuEvt.NPCTalking[0].moveToClasses[1].Obj = gameObject;
                                            HighP.status.toDoList.Add(PuEvt);
                                            HighP.TriggerEvent();
                                            status.toDoList.Add(PuEvt);
                                            TriggerEvent();
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("Wrong Key");
                                        break;
                                    }
                                }
                                else
                                {
                                    Debug.Log("No Key");
                                    break;
                                }
                            }                          
                        }
                    }
                    else
                        Debug.LogWarning("No CBord", gameObject);
                    break;
                default:
                    break;
            }
            CurrentInteractObject = null;
            locatorList.npc = null;
            locatorList = null;
        }
        else if (CurrentInteractItem != null && status.CarryItem == Item_SO.ItemType.None)
        {
            Debug.Log("Grabing");
            switch (CurrentInteractItem.type)
            {
                case Item_SO.ItemType.None:
                    CurrentInteractItem = null;
                    break;
                case Item_SO.ItemType.MedicalKit:
                    status.CarryItem = CurrentInteractItem.type;
                    if (CurrentInteractItem.GetComponent<MedicalKit>() != null)
                        status.healAmount = CurrentInteractItem.GetComponent<MedicalKit>().HPRecovery;
                    else
                        status.healAmount = CurrentInteractItem.GetComponent<DeadBox>().HPRecovery;
                    InsertMenu(rightClickMenus.Count, "Heal", "Heal", true, Heal, 1 << LayerMask.NameToLayer("NPC"));
                    CurrentInteractItem.NPCInteract(0);
                    CurrentInteractItem = null;
                    break;
                case Item_SO.ItemType.Key:
                    status.CarryItem = CurrentInteractItem.type;
                    status.code = CurrentInteractItem.GetComponent<DeadBox>().code;
                    CurrentInteractItem.NPCInteract(0);
                    break;
                case Item_SO.ItemType.RepairedPart:
                    status.CarryItem = CurrentInteractItem.type;
                    CurrentInteractItem.NPCInteract(0);
                    break;
                default:
                    break;
            }
        }
        else if (CurrentInteractItem == null && status.CarryItem != Item_SO.ItemType.None)
        {
            switch (status.CarryItem)
            {
                case Item_SO.ItemType.None:
                    break;
                case Item_SO.ItemType.MedicalKit:
                    Debug.Log("Healing");
                    if (HealingTarget != null)
                    {
                        NpcController npc = HealingTarget.GetComponent<NpcController>();
                        if(npc.status.currentHealth< npc.status.maxHealth)
                        {
                            HealingTarget.GetComponent<NpcController>().ApplyHealth(status.healAmount);
                            HealingTarget = null;
                            status.CarryItem = Item_SO.ItemType.None;
                            status.healAmount = 0;
                            RemoveMenu("Heal");
                        }
                        else
                        {
                            Debug.Log("No need Heal");
                            HealingTarget = null;
                        }
                    }
                    else
                    {
                        ApplyHealth(status.healAmount); status.CarryItem = Item_SO.ItemType.None;
                        status.healAmount = 0;
                        RemoveMenu("Heal");
                    }             
                    break;
                default:
                    break;
            }
        }
        else if (CurrentInteractItem != null && status.CarryItem != Item_SO.ItemType.None)
        {
            CurrentInteractItem = null;
        }
        if (m_fsm.GetCurrentState() != "GotAttacked" && (!inAnimState || IsPrisoner))
            BackToPatrol();
        else
        {
            SwitchAnimState(true, AnimStateName);
            isGuard = false;
        }
    }

    public void FacingEachOther(bool IsFacingCamera = false)
    {
        Vector3 dir = Vector3.zero;
        if (!IsFacingCamera)
        {
            dir = (TalkingPos - transform.position).normalized;
        }
        else
        {
            dir = (currentRoomTracker.cameraLists[currentRoomTracker.CurrentCameraIndex].roomCamera.gameObject.transform.position - transform.position).normalized;
        }
        bool Damping = false;
        
        
        dir.y = 0;
        Quaternion rotation = Quaternion.LookRotation(dir);

        if (Quaternion.Angle(transform.rotation, rotation) >= 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, DampRotSpeed);
            Damping = true;
        }

        if (Damping)
        {
            Debug.Log("Damping");
        }
    }

    public void FacingEachOther(Transform tran)
    {
        Vector3 dir = (tran.position - transform.position).normalized;

        dir.y = 0;
        Quaternion rotation = Quaternion.LookRotation(dir);

        if (Quaternion.Angle(transform.rotation, rotation) >= 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, DampRotSpeed);
        }
    }

    public void FacingEachOtherCoro(Transform tran)
    {
        Vector3 dir = (tran.position - transform.position).normalized;
        StartCoroutine(FacingEachOtherCoro(dir));
    }

    public void FacingEachOtherCoro()
    {
        Vector3  dir = (currentRoomTracker.cameraLists[currentRoomTracker.CurrentCameraIndex].roomCamera.gameObject.transform.position - transform.position).normalized;
        StartCoroutine(FacingEachOtherCoro(dir));
    }

    IEnumerator FacingEachOtherCoro(Vector3 dir)
    {
        bool Damping = true;
        dir.y = 0;
        Quaternion rotation = Quaternion.LookRotation(dir);
        while (Damping)
        {
            if (Quaternion.Angle(transform.rotation, rotation) >= 1)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, DampRotSpeed);
                Damping = true;
            }
            else
            {
                Damping = false;
            }
            yield return null;
        }
        
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
            Death();
        }
    }

    public void IsStruggling(float rate)
    {
        status.currentHealth -= rate * Time.deltaTime;
        if (status.currentHealth <= 0)
        {
            PlayAudio("Dead_Simple " + Random.Range(1, 5).ToString());
            Death();
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
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentTerminalPos, 1);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, bannedRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + new Vector3(0, 3, 0), -transform.up * detectRay);
        if (RescuingTarget != null)
        {
            if (isBlocked)
            {
                Gizmos.color = Color.blue;
            }
            else
            {
                Gizmos.color = inAngle ? Color.red : Color.green;
            }
            Gizmos.DrawLine(transform.position + new Vector3(0, 3, 0), RescuingTarget.transform.position + new Vector3(0, 3, 0));
        }

        Gizmos.color = Color.white;
        if(navAgent != null)
            Gizmos.DrawSphere(navAgent.destination, 1);
    }
    #endregion

    #region SpecialTalking
    public void SpecialTalking(object obj)
    {
        switch (Stage)
        {
            case 0:
                GameManager.GetInstance().CurrentRoom.PlayingDialogue(NoThingTalk);
                break;
            case 1:
                GameManager.GetInstance().CurrentRoom.PlayingDialogue(MedicalKit);
                break;
            case 2:
                GameManager.GetInstance().CurrentRoom.PlayingDialogue(JoinIn);
                break;
            default:
                break;
        }
    }
    #endregion
}