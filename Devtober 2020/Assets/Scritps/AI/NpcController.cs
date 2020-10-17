using EvtGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay;
using System;

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
    }

    public Status status = null;

    [System.Serializable]
    public class PatrolRange
    {
        [Range(0f, 50f)]
        public float maxX = 0;
        [Range(0f, 50f)]
        public float y = 0;
        [Range(0f, 50f)]
        public float maxZ = 0;
        [Range(0f, 50f)]
        public float banned = 0;
    }
    [SerializeField]
    PatrolRange patrolRange = null;

    [SerializeField]
    [Range(0f, 100f)]
    float alertRadius = 0;

    [SerializeField]
    LayerMask needDodged = 0;

    [SerializeField]
    Collider[] hitObjects = null;

    [SerializeField]
    float dodgeAngle = 0;

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
    [HideInInspector]
    public Vector3 currentTerminalPos;

    Transform finalHidingPos;
    Transform finalEscapingPos;

    public List<RoomTracker> roomScripts = new List<RoomTracker>();
    public List<GameObject> hiddenSpots = new List<GameObject>();
    public List<GameObject> rooms = new List<GameObject>();

    public bool isSafe = false;

    GameObject hideIn = null;
    HiddenPos hiddenPos;
    #endregion

    #region InteractWithItem
    [Header("Interact Item")]
    public float DampPosSpeed = 0.2f;
    public float DampRotSpeed = 0.2f;
    public BoxCollider boxCollider;

    Item_SO CurrentInteractItem;
    LocatorList locatorList;

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
        AddMenu("Hide", "Hide in", true, ReceiveItemCall, 1 << LayerMask.NameToLayer("HiddenPos") | 1 << LayerMask.NameToLayer("RestingPos"));
        #endregion
    }

    private void Start()
    {
        currentTerminalPos = NewDestination();
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

        for (int i = 0; i < roomScripts.Count; i++)
        {
            foreach (GameObject temp in roomScripts[i].HiddenPos())
            {
                if (!hiddenSpots.Contains(temp))
                    hiddenSpots.Add(temp);
            }
        }
    }

    private void Update()
    {
        #region StringRestrictedFiniteStateMachine Update
        switch (m_fsm.GetCurrentState())
        {
            case "Patrol":
                Dispatch(currentTerminalPos);
                GenerateNewDestination();
                TriggerDodging();
                break;
            case "Rest":
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
        normalSpeedOffMeshLink();
    }

    #region Move
    public void BackToPatrol(object obj = null)
    {
        currentTerminalPos = NewDestination();
        m_fsm.ChangeState("Patrol");
        ResetHiddenPos();
    }

    public float distance()
    {
        float a = navAgent.destination.x - transform.position.x;
        float b = navAgent.destination.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        return Mathf.Abs(c);
    }

    public Vector3 NewDestination()
    {
        float x = UnityEngine.Random.Range(transform.position.x - patrolRange.maxX / 2, transform.position.x + patrolRange.maxX / 2);
        float z = UnityEngine.Random.Range(transform.position.z - patrolRange.maxZ / 2, transform.position.z + patrolRange.maxZ / 2);

        Vector3 tempPos = new Vector3(x, transform.position.y, z);     
        return tempPos;
    }

    private void GenerateNewDestination()
    {
        float a = currentTerminalPos.x - transform.position.x;
        float b = currentTerminalPos.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        if (Mathf.Abs(c) < patrolRange.banned || !navAgent.CalculatePath(currentTerminalPos, path))
        {
            currentTerminalPos = NewDestination();
        }
    }

    void normalSpeedOffMeshLink()
    {
        OffMeshLinkData data = navAgent.currentOffMeshLinkData;
        Vector3 endPos = data.endPos + Vector3.up * navAgent.baseOffset;
        if(navAgent.isOnOffMeshLink && navAgent.transform.position != endPos)
        {
            navAgent.transform.position = Vector3.MoveTowards(navAgent.transform.position, endPos, navAgent.speed * Time.deltaTime);
        }
    }

    public void Dispatch(object newPos)
    {
        navAgent.SetDestination((Vector3)newPos);
        if(m_fsm.GetCurrentState() != "InteractWithItem")
        {
            if(navAgent.velocity.magnitude >= 0.1)
            {
                animator.Play("Walk", 0);
            }
            else
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
        if (distance() < restDistance)
        {
            navAgent.ResetPath();
            BackToPatrol();
        }
    }
    #endregion

    #region Hiding
    public void TriggerHiding(object obj = null)
    {
        RemoveAndInsertMenu("Hide", "BackToPatrol", "Leave", false, BackToPatrol);
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
            if (isTaken == true)
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
            CurrentInteractItem.Locators.Find((x) => (x == locatorList)).npc = null;
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
                        if (evt.NPCTalking[a].MoveToClassA.Name == status.npcName)
                        {
                            Dispatch(evt.NPCTalking[a].MoveToClassA.MoveTO);
                        }
                        else if (evt.NPCTalking[a].MoveToClassB.Name == status.npcName)
                        {
                            Dispatch(evt.NPCTalking[a].MoveToClassB.MoveTO);
                        }
                    }
                    break;
                case DoingWithNPC.MoveTo:
                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                    {
                        if (evt.NPCWayPoint[a].Name == status.npcName)
                        {
                            Dispatch(evt.NPCWayPoint[a].MoveTO);
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
        if (distance() <= restDistance)
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

            if (Vector3.Angle(enemyDirection, movingDirection) > dodgeAngle / 2
                || Mathf.Abs(c) < patrolRange.banned
                || !navAgent.CalculatePath(currentTerminalPos, path))
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
        Physics.Raycast(transform.position, -transform.up, out hitroom, patrolRange.y, (int)Mathf.Pow(2, 9));

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
        if (distance() < restDistance)
        {
            navAgent.ResetPath();
            BackToPatrol();
        }
    }
    #endregion

    #region Receive Call
    public void ReceiveItemCall(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        Item_SO item = gameObj.GetComponent<Item_SO>();

        if(item != null)
        {
            Debug.Log("Receive");
            Vector3 Pos = Vector3.zero;

            float minDistance = Mathf.Infinity;
            for (int i = 0; i < item.Locators.Count; i++)
            {
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

            CurrentInteractItem = item;
            HasInteract = false;
            Dispatch(Pos);
            m_fsm.ChangeState("InteractWithItem");
        }
    }
    #endregion

    #region Play Animation
    void PlayGetInAnim()
    {
        if (distance() < restDistance || !navAgent.enabled)
        {
            boxCollider.enabled = false;
            navAgent.enabled = false;

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
                switch (CurrentInteractItem.type)
                {
                    case Item_SO.ItemType.Locker:
                        CurrentInteractItem.NPCInteract(0);
                        animator.Play("GetInLocker", 0);
                        HasInteract = true;
                        isSafe = true;
                        break;
                    case Item_SO.ItemType.Box:
                        isSafe = true;
                        break;
                    case Item_SO.ItemType.Bed:
                        animator.Play("GetOnBed", 0);
                        HasInteract = true;
                        break;
                    case Item_SO.ItemType.Chair:
                        break;
                    case Item_SO.ItemType.Terminal:
                        break;
                    default:
                        break;
                }
            }
        }
        else if (navAgent.velocity.magnitude >= 0.1)
        {
            animator.Play("Walk", 0);
        }
        else
        {
            animator.Play("Idle", 0);
        }
    }

    public void PlayGetOutAnim(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        Item_SO item = gameObj.GetComponent<Item_SO>();
        switch (item.type)
        {
            case Item_SO.ItemType.Locker:
                animator.Play("GetOutLocker", 0);
                break;
            case Item_SO.ItemType.Box:
                break;
            case Item_SO.ItemType.Bed:
                break;
            case Item_SO.ItemType.Chair:
                break;
            case Item_SO.ItemType.Terminal:
                break;
            default:
                break;
        }
        CurrentInteractItem = item;

    }

    public void CompleteGetInItemAction()
    {
        //hiddenPos.isTaken = true;
        //gameObject.layer = LayerMask.NameToLayer("Safe");
        //RemoveAndInsertMenu("BackToPatrol", "Hide", "Hide", false, TriggerHiding);
        CurrentInteractItem.Locators.Find((x) => (x == locatorList)).npc = this;
        m_fsm.ChangeState("Rest");
    }

    public void CompleteGetOutItemAction()
    {
        boxCollider.enabled = true;
        navAgent.enabled = true;
        CurrentInteractItem.RemoveAndInsertMenu("Leave", "Hide In", "Hide In", true, CurrentInteractItem.CallNPC, 1 << LayerMask.NameToLayer("NPC"));
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.maxX, 0, patrolRange.maxZ));
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRange.banned);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentTerminalPos, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * patrolRange.y);
    }
    #endregion
}