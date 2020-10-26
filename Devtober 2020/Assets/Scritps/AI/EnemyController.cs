using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GamePlay;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]

public class EnemyController : ControllerBased
{
    #region Inspector View
    [SerializeField]
    [Range(0f, 100f)]
    float discoverRadius = 0;

    [SerializeField]
    float discoverAngle = 0;

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

    [SerializeField]
    LayerMask canChased = 0;

    [SerializeField]
    LayerMask canBlocked = 0;

    [SerializeField]
    Collider[] hitNPCs;
    [SerializeField]
    Collider[] attackable;

    [System.Serializable]
    public class PatrolRange
    {
        [Range(0f, 50f)]
        public float maxX = 0;
        [Range(0f, 50f)]
        public float maxZ = 0;
    }
    [SerializeField]
    PatrolRange patrolRange = null;

    #endregion

    #region Fields
    public StringRestrictedFiniteStateMachine m_fsm;

    NavMeshAgent navAgent;
    NavMeshPath path;
    Animator animator;
    public NpcController npc;
    ZombieAttackCollider resetAttack;
    #endregion

    #region Value
    [HideInInspector]
    public Vector3 currentTerminalPos;

    public bool hasAttacked = false;
    float recordAttackTime, recordSpeed;
    List<RoomTracker> roomScripts = new List<RoomTracker>();

    Transform finalPos;
    GameObject target;
    bool inAngle, isBlocked;
    bool MoveAcrossNavMeshesStarted;
    #endregion


    private void Awake()
    {
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
            { "Patrol", new List<string> { "Chase", "Rest", "Dispatch", "Executing" } },
            { "Chase", new List<string> { "Patrol", "Rest", "Dispatch", "Executing" } },
            { "Rest", new List<string> { "Patrol", "Chase", "Dispatch", "Executing" } },
            { "Dispatch", new List<string> { "Patrol", "Chase", "Rest", "Executing" } },
            { "Executing", new List<string> { "Patrol", "Chase", "Rest", "Dispatch" } }
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
                Discover();
                break;
            case "Chase":
                Chasing();
                Discover();
                lossTarget();
                break;
            case "Rest":
                Resting();
                break;
            case "Dispatch":
                break;
            case "Executing":
                IsExecuting();
                break;
            default:
                break;
        }
        #endregion
        if (navAgent.isOnOffMeshLink && !MoveAcrossNavMeshesStarted)
        {
            StartCoroutine(MoveAcrossNavMeshLink());
            MoveAcrossNavMeshesStarted = true;
        }
    }

    #region Move
    public Vector3 NewDestination()
    {
        float x = Random.Range(transform.position.x - patrolRange.maxX / 2, transform.position.x + patrolRange.maxX / 2);
        float z = Random.Range(transform.position.z - patrolRange.maxZ / 2, transform.position.z + patrolRange.maxZ / 2);

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
            animator.Play("Idle", 0);
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

        if (hitNPCs.Length != 0 && !hasAttacked)
        {
            if (m_fsm.GetCurrentState() != "Chase")
            {
                navAgent.speed *= chaseSpeed;
                m_fsm.ChangeState("Chase");
            }

            if (target != null)
            {
                isBlocked = Physics.Linecast(transform.position, target.transform.position, canBlocked);
                Vector3 direction = (target.transform.position - transform.position).normalized;
                float targetAngle = Vector3.Angle(transform.forward, direction);
                inAngle = targetAngle <= discoverAngle / 2 ? true : false;
            }
        }
    }

    public void Chasing()
    {
        float minDistance = Mathf.Infinity;

        for(int i = 0; i<hitNPCs.Length; i++)
        {
            if (hitNPCs[i].GetComponent<NpcController>().isSafe)
                continue;
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

        if (target != null && finalPos != null && !npc.isSafe && inAngle && !isBlocked)
        {
            Dispatch(finalPos.position);
        }
        Attacking();
    }

    void lossTarget()
    {
        if (hitNPCs.Length == 0)
        {
            target = null;
            BackToPatrol();
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
            if(npc.status.currentHealth <= executeHealth)
            {
                npc.m_fsm.ChangeState("GotAttacked");
                animator.Play("Zombie_Hug", 0);
                npc.animator.Play("Got Bite", 0);
                npc.status.isStruggling = true;
                m_fsm.ChangeState("Executing");
            }
            else
            {
                animator.Play("Zombie_Attack", 0);
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
        }
    }
    #endregion

    #region Resting
    public void TriggerResting()
    {
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
            BackToPatrol();
        }
    }
    #endregion

    #region Dispatch
    public void readyForDispatch()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Dispatch");
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

        if(hitNPCs.Length != 0 && target != null)
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
