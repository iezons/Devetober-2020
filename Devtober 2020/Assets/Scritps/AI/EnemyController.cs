using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GamePlay;
using UnityEngine.SceneManagement;
using EvtGraph;
using System;
using DiaGraph;

[RequireComponent(typeof(NavMeshAgent))]

public class EnemyController : ControllerBased
{
    #region Inspector View
    [System.Serializable]
    public class PatrolRange
    {
        [Range(0f, 50f)]
        public float maxX = 0;
        [Range(0f, 50f)]
        public float maxZ = 0;
    }

    public DialogueGraph PriDead;

    public string enemyName = Guid.NewGuid().ToString();

    [SerializeField]
    PatrolRange patrolRange = null;

    [Header("Visual Setting")]
    [SerializeField]
    [Range(0f, 100f)]
    float discoverRadius = 0;

    [SerializeField]
    float discoverAngle = 0;

    [Header("Chasing&Attacking Setting")]
    [SerializeField]
    LayerMask canChased = 0;
    [SerializeField]
    LayerMask canBlocked = 0;

    [SerializeField]
    float chaseSpeed = 0;

    [SerializeField]
    [Range(0f, 100f)]
    float attackRadius = 0;
    [SerializeField]
    float attackTime = 0;

    [SerializeField]
    float executingRate = 0;
    [SerializeField]
    float executeHealth = 0;

    public float restDistance = 0.2f;

    public List<EventSO> toDoList;

    #endregion

    #region Fields
    StringRestrictedFiniteStateMachine m_fsm;
    NavMeshAgent navAgent;
    NavMeshPath path;
    Animator animator;
    NpcController npc;
    ZombieAttackCollider resetAttack;
    #endregion

    #region Value
    Collider[] hitNPCs;
    public Collider[] attackable;

    [HideInInspector]
    public Vector3 currentTerminalPos;

    bool hasAttacked = false;
    bool inAngle, isBlocked;
    bool MoveAcrossNavMeshesStarted;

    float recordAttackTime, recordSpeed;

    List<RoomTracker> roomScripts = new List<RoomTracker>();

    Transform finalPos;

    public GameObject target;
    float timer = 4;

    bool isJustEnterEvent = false;
    bool isReachDestination = false;
    public float audioTimer = 8;
    public AudioSource source;
    #endregion

    void PlayAudio(string str)
    {
        AudioMgr.GetInstance().PlayAudio(source, str, 1f, false, null);
    }

    private void Awake()
    {
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        if (sources.Length > 0)
            source = sources[0];
        outline = GetComponent<Outline>();
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        resetAttack = GetComponent<ZombieAttackCollider>();
        path = new NavMeshPath();
        IsInteracting = true;
        recordSpeed = navAgent.speed;

        #region StringRestrictedFiniteStateMachine
        Dictionary<string, List<string>> EnemyDictionary = new Dictionary<string, List<string>>()
        {
            { "Patrol", new List<string> { "Chase", "Rest", "Dispatch", "Executing", "Event" } },
            { "Chase", new List<string> { "Patrol", "Rest", "Dispatch", "Executing", "Event" } },
            { "Rest", new List<string> { "Patrol", "Chase", "Dispatch", "Executing", "Event" } },
            { "Dispatch", new List<string> { "Patrol", "Chase", "Rest", "Executing", "Event" } },
            { "Executing", new List<string> { "Patrol", "Chase", "Rest", "Dispatch", "Event" } },
            { "Event", new List<string> { "Patrol", "Chase", "Rest", "Dispatch", "Executing" } },
        };

        m_fsm = new StringRestrictedFiniteStateMachine(EnemyDictionary, "Patrol");
        #endregion
        
    }
    private void Start()
    {
        EventCenter.GetInstance().EventTriggered("GM.Enemy.Add", this);
        currentTerminalPos = NewDestination();
        recordAttackTime = attackTime;

        Invoke("GenerateList", 0.00001f);
    }

    void GenerateList()
    {
        foreach (RoomTracker temp in GameManager.GetInstance().Rooms)
        {
            roomScripts.Add(temp);
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
                VisionCone();
                Discover();
                if(navAgent.velocity.magnitude <= 0.1)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        currentTerminalPos = NewDestination();
                        timer = 4;
                    }
                }

                audioTimer -= Time.deltaTime;
                if (audioTimer <= 0)
                {
                    PlayAudio("Enemy Idel " + UnityEngine.Random.Range(1, 4).ToString());
                    audioTimer = 8;
                }              
                break;
            case "Chase":
                audioTimer -= Time.deltaTime;
                if (audioTimer <= 0)
                {
                    PlayAudio("Enemy Moans " + UnityEngine.Random.Range(1, 4).ToString());
                    audioTimer = 6;
                }
                VisionCone();
                Chasing();
                break;
            case "Rest":
                Resting();
                break;
            case "Dispatch":
                CompleteDispatching();
                break;
            case "Executing":
                IsExecuting();
                break;
            case "Event":
                Event();
                ReachDestination();
                break;
            default:
                break;
        }
        #endregion

        CheckEvent();

        if (navAgent.isOnOffMeshLink && !MoveAcrossNavMeshesStarted)
        {
            StartCoroutine(MoveAcrossNavMeshLink());
            MoveAcrossNavMeshesStarted = true;
        }
    }

    #region Move
    public Vector3 NewDestination()
    {
        float x = UnityEngine.Random.Range(transform.position.x - patrolRange.maxX / 2, transform.position.x + patrolRange.maxX / 2);
        float z = UnityEngine.Random.Range(transform.position.z - patrolRange.maxZ / 2, transform.position.z + patrolRange.maxZ / 2);

        Vector3 tempPos = new Vector3(x, transform.position.y, z);
        return tempPos;
    }

    public float Distance()
    {
        float a = navAgent.destination.x - transform.position.x;
        float b = navAgent.destination.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        return Mathf.Abs(c);
    }

    private void GenerateNewDestination()
    {
        float a = currentTerminalPos.x - transform.position.x;
        float b = currentTerminalPos.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        if (Mathf.Abs(c) < 1 || !navAgent.CalculatePath(currentTerminalPos, path)
            )
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

    public void Dispatch(Vector3 newPos)
    {
        navAgent.SetDestination(newPos);

        if (m_fsm.GetCurrentState() == "Chase")
        {
            animator.Play("Zombie_Chase", 0);
        }
        else if (navAgent.velocity.magnitude >= 0.1 || navAgent.isOnOffMeshLink)
        {
            animator.Play("Zombie_Walk", 0); 
        }
        else if (!navAgent.isOnOffMeshLink)
        {
            animator.Play("Zombie_Idle", 0);
        }
    }

    public void BackToPatrol()
    {
        navAgent.speed = recordSpeed;
        currentTerminalPos = NewDestination();
        m_fsm.ChangeState("Patrol");
    }

    #endregion

    #region Chasing
    private void Discover()
    {
        hitNPCs = Physics.OverlapSphere(transform.position, discoverRadius, canChased);

        float minDistance = Mathf.Infinity;

        for (int i = 0; i < hitNPCs.Length; i++)
        {
            Transform tempTrans = hitNPCs[i].transform;
            float a = tempTrans.position.x - transform.position.x;
            float b = tempTrans.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            float distance = Mathf.Abs(c);

            if (distance < minDistance)
            {
                minDistance = distance;
                target = hitNPCs[i].gameObject;
                npc = target.GetComponent<NpcController>();
                finalPos = tempTrans;
            }
        }

        if (hitNPCs.Length != 0 && !hasAttacked && target != null && finalPos != null && inAngle && !isBlocked && !npc.isSafe)
        {
            navAgent.speed *= chaseSpeed;
            toDoList.Clear();
            m_fsm.ChangeState("Chase");
        }
    }

    public void Chasing()
    {
        hitNPCs = Physics.OverlapSphere(transform.position, discoverRadius, canChased);
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < hitNPCs.Length; i++)
        {
            Transform tempTrans = hitNPCs[i].transform;
            float a = tempTrans.position.x - transform.position.x;
            float b = tempTrans.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            float distance = Mathf.Abs(c);

            if (distance < minDistance)
            {
                minDistance = distance;
                target = hitNPCs[i].gameObject;
                npc = target.GetComponent<NpcController>();
                finalPos = tempTrans;
            }
        }

        if (target != null && finalPos != null && inAngle && !isBlocked)
        {
            npc.isEnemyChasing = true;
            Dispatch(finalPos.position);
            Attacking();
        }
        else if (hitNPCs.Length != 0 && !inAngle || isBlocked)
        {
            npc.isEnemyChasing = false;
            //Dispatch(currentTerminalPos);
            //GenerateNewDestination();
        }
        
        if(hitNPCs.Length == 0)
        {
            npc.isEnemyChasing = false;
            target = null;
            npc = null;
            BackToPatrol();
        }            
    }

    void VisionCone()
    {
        if (target != null)
        {
            isBlocked = Physics.Linecast(transform.position, target.transform.position, canBlocked);
            Vector3 direction = (target.transform.position - transform.position).normalized;
            float targetAngle = Vector3.Angle(transform.forward, direction);
            inAngle = targetAngle <= discoverAngle / 2 ? true : false;
        }
    }

    #endregion

    #region Attacking
    void Attacking()
    {
        attackable = Physics.OverlapSphere(transform.position + new Vector3(0, 3, 0), attackRadius, canChased);
        if (attackable.Length != 0 && target == attackable[attackable.Length - 1].gameObject)
        {
            hasAttacked = true;
            navAgent.ResetPath();
            if (npc.isSafe || npc.m_fsm.GetCurrentState() == "Rest")
            {
                npc.m_fsm.ChangeState("GotAttacked");
                animator.Play("Zombie_Hug", 0);
                npc.CurrentObjectAnimPlay(target);
                if(npc.CurrentInteractObject != null)
                {
                    switch (npc.CurrentInteractObject.type)
                    {
                        case Interact_SO.InteractType.Locker:
                            target.transform.eulerAngles += new Vector3(0, 180, 0);
                            break;
                        case Interact_SO.InteractType.Box:
                            target.transform.eulerAngles += new Vector3(0, 180, 0);
                            break;
                        case Interact_SO.InteractType.Bed:
                            float minDistance = Mathf.Infinity;
                            Vector3 teleportPos = Vector3.zero;
                            for (int i = 0; i < npc.CurrentInteractObject.Locators.Count; i++)
                            {
                                Transform tempTrans = npc.CurrentInteractObject.Locators[i].Locator;
                                float a = tempTrans.position.x - transform.position.x;
                                float b = tempTrans.position.z - transform.position.z;
                                float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
                                float distance = Mathf.Abs(c);

                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    teleportPos = tempTrans.position;
                                }
                            }
                            npc.transform.position = teleportPos;
                            break;
                        case Interact_SO.InteractType.Chair:
                            break;
                        case Interact_SO.InteractType.Terminal:
                            break;
                        case Interact_SO.InteractType.Switch:
                            break;
                        case Interact_SO.InteractType.Storage:
                            break;
                        case Interact_SO.InteractType.CBoard:
                            break;
                        case Interact_SO.InteractType.TU_Server:
                            break;
                        default:
                            break;
                    }
                }
                npc.CompleteGetOutItemAction();
                npc.animator.Play("Got Bite", 0);
                if (npc.IsPrisoner)
                {
                    EventCenter.GetInstance().DiaEventTrigger("01_Dead");
                    GameManager.GetInstance().CurrentRoom.DiaPlay.d_state = DiaState.OFF;
                    GameManager.GetInstance().CurrentRoom.DiaPlay.WholeText = "";
                    GameManager.GetInstance().CurrentRoom.PlayingDialogue(PriDead);
                }
                npc.status.isStruggling = true;
                npc.HasInteract = false;
                PlayAudio("Enemy Moans 4");
                npc.PlayAudio("Dead_Scream " + UnityEngine.Random.Range(1, 7).ToString());
                m_fsm.ChangeState("Executing");
            }
            else if (npc.status.currentHealth <= executeHealth)
            {
                npc.m_fsm.ChangeState("GotAttacked");
                animator.Play("Zombie_Hug", 0);
                npc.animator.Play("Got Bite", 0);
                if(npc.IsPrisoner)
                {
                    EventCenter.GetInstance().DiaEventTrigger("01_Dead");
                    GameManager.GetInstance().CurrentRoom.DiaPlay.d_state = DiaState.OFF;
                    GameManager.GetInstance().CurrentRoom.DiaPlay.WholeText = "";
                    GameManager.GetInstance().CurrentRoom.PlayingDialogue(PriDead);
                }
                npc.status.isStruggling = true;
                PlayAudio("Enemy Moans 4");
                npc.PlayAudio("Dead_Scream " + UnityEngine.Random.Range(1, 7).ToString());
                m_fsm.ChangeState("Executing");
            }
            else
            {
                animator.Play("Zombie_Attack", 0);
                npc.navAgent.enabled = true;
                npc.Stop(null);
                PlayAudio("Enemy Moans " + UnityEngine.Random.Range(5, 7).ToString());
                TriggerResting();
            }       
        }
    }

    void IsExecuting()
    {
        npc.IsStruggling(executingRate);
        if(npc.status.currentHealth <= 0)
        {
            animator.Play("Zombie_Idle", 0);
            TriggerResting();
        }
        else if (!npc.status.isStruggling && npc.status.currentHealth > 0)
        {
            npc.animator.Play("Escape", 0);
            animator.Play("FailExecuting", 0);
            TriggerResting();
            if (npc.MenuContains("Interact") >= 0)
            {
                return;
            }
            else
            {
                npc.AddMenu("Interact", "Interact", true, npc.ReceiveInteractCall, 1 << LayerMask.NameToLayer("HiddenPos")
            | 1 << LayerMask.NameToLayer("RestingPos")
            | 1 << LayerMask.NameToLayer("TerminalPos")
            | 1 << LayerMask.NameToLayer("SwitchPos")
            | 1 << LayerMask.NameToLayer("Item")
            | 1 << LayerMask.NameToLayer("CBord")
            | 1 << LayerMask.NameToLayer("StoragePos"));
            }
        }
    }
    #endregion

    #region Resting
    public void TriggerResting()
    {
        npc.isEnemyChasing = false;
        m_fsm.ChangeState("Rest");
    }

    void Resting()
    {
        attackTime -= Time.deltaTime;
        if (attackTime <= 0)
        {
            attackTime = recordAttackTime;
            hasAttacked = false;
            resetAttack.isHit = false;
            m_fsm.ChangeState("Patrol");
        }
    }
    #endregion

    #region Dispatch
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

    #region Event
    public void TriggerEvent()
    {
        navAgent.ResetPath();
        isJustEnterEvent = true;
        m_fsm.ChangeState("Event");
    }

    private void Event()
    {
        if (isJustEnterEvent)
        {
            Debug.Log("Event");
            isJustEnterEvent = false;
            if (toDoList.Count > 0)
            {
                EventSO evt = toDoList[0];
                switch (evt.doingWithEnemy)
                {
                    case DoingWithEnemy.MoveTo:
                        for (int i = 0; i < evt.EnemyWayPoint.Count; i++)
                        {
                            if (evt.EnemyWayPoint[i].Obj == gameObject)
                            {
                                isReachDestination = false;
                                Dispatch(evt.EnemyWayPoint[i].MoveTO.position);
                                Debug.Log("Dispatch");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            if (toDoList.Count > 0)
            {
                EventSO evt = toDoList[0];
                switch (evt.doingWithEnemy)
                {
                    case DoingWithEnemy.MoveTo:
                        VisionCone();
                        Discover();
                        if (isReachDestination)
                        {
                            toDoList.Remove(evt);
                            isJustEnterEvent = true;
                            Debug.Log("Reach");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        if (toDoList.Count <= 0)
        {
            BackToPatrol();
        }
    }

    public void CheckEvent()
    {
        if (toDoList != null)
        {
            if (toDoList.Count != 0)
            {
                m_fsm.ChangeState("Event");
            }
        }
    }

    public void ReachDestination()
    {
        if (Distance() <= restDistance)
        {
            EventCenter.GetInstance().EventTriggered("GM.EnemyArrive", enemyName);
            if(m_fsm.GetCurrentState() == "Event")
                isReachDestination = true;
            //TODO 修改NPC Arrive call的方法
            navAgent.ResetPath();
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 3, 0), attackRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, discoverRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.maxX, 0, patrolRange.maxZ));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentTerminalPos, 1);

        if(hitNPCs != null)
        {
            if (hitNPCs.Length != 0 && target != null)
            {
                if (isBlocked)
                {
                    Gizmos.color = Color.blue;
                }
                else
                {
                    Gizmos.color = inAngle ? Color.red : Color.green;
                }

                Gizmos.DrawLine(transform.position + new Vector3(0, 3, 0), target.transform.position + new Vector3(0, 3, 0));
            }
        }
        

        //float yAngle;
        //if (transform.eulerAngles.y > 180)
        //{
        //    yAngle = transform.eulerAngles.y - 360;
        //}
        //else
        //{
        //    yAngle = transform.eulerAngles.y;
        //}

        //Gizmos.color = Color.green;
        //float x = Mathf.Sin(yAngle - discoverAngle / 2);
        //float z = Mathf.Cos(yAngle + discoverAngle / 2);
        //Vector3 lineEnd1 = new Vector3(x, 0, -z);
        //Vector3 lineEnd2 = new Vector3(-x, 0, -z);
        //Gizmos.DrawLine(transform.position, transform.position + lineEnd1 * discoverRadius);
        //Gizmos.DrawLine(transform.position, transform.position + lineEnd2 * discoverRadius);
    }

    #endregion
}
