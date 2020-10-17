using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GamePlay;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]

public class EnemyController : MonoBehaviour
{
    #region Inspector View
    [SerializeField]
    [Range(0f, 100f)]
    float discoverRadius = 0;

    [SerializeField]
    [Range(0f, 100f)]
    float attackRadius = 0;

    [SerializeField]
    int attackDamage = 0;

    [SerializeField]
    float attackTime = 0;

    [SerializeField]
    LayerMask canChased = 0;

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
    StringRestrictedFiniteStateMachine m_fsm;

    NavMeshAgent navAgent;
    NavMeshPath path;
    #endregion

    #region Value
    [HideInInspector]
    public Vector3 currentPos;

    public bool hasAttacked = false;
    float recordAttackTime;
    List<RoomTracker> roomScripts = new List<RoomTracker>();

    Transform finalPos;
    GameObject target;
    #endregion


    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();

        #region StringRestrictedFiniteStateMachine
        Dictionary<string, List<string>> EnemyDictionary = new Dictionary<string, List<string>>()
        {
            { "Patrol", new List<string> { "Chase", "Rest", "Dispatch" } },
            { "Chase", new List<string> { "Patrol", "Rest", "Dispatch" } },
            { "Rest", new List<string> { "Patrol", "Chase", "Dispatch" } },
            { "Dispatch", new List<string> { "Patrol", "Chase", "Rest" } },
        };

        m_fsm = new StringRestrictedFiniteStateMachine(EnemyDictionary, "Patrol");
        #endregion
        
    }
    private void Start()
    {
        currentPos = NewDestination();
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
                Dispatch(currentPos);
                GenerateNewDestination();
                Discover();
                break;
            case "Chase":
                Discover();
                Chasing();
                lossTarget();
                break;
            case "Rest":
                Resting();
                break;
            case "Dispatch":
                break;
            default:
                break;
        }
        #endregion
    }

    #region Move
    public Vector3 NewDestination()
    {
        float x = Random.Range(transform.position.x - patrolRange.maxX / 2, transform.position.x + patrolRange.maxX / 2);
        float z = Random.Range(transform.position.z - patrolRange.maxZ / 2, transform.position.z + patrolRange.maxZ / 2);

        Vector3 tempPos = new Vector3(x, transform.position.y, z);
        return tempPos;
    }
    private void GenerateNewDestination()
    {
        float a = currentPos.x - transform.position.x;
        float b = currentPos.z - transform.position.z;
        float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        if (Mathf.Abs(c) < 1 || !navAgent.CalculatePath(currentPos, path)
            )
        {
            currentPos = NewDestination();
        }
    }
    public void Dispatch(Vector3 newPos)
    {
        navAgent.SetDestination(newPos);
    }

    #endregion

    #region Special Action
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
                finalPos = tempTrans;
            }
        }
        if (finalPos != null && !target.GetComponent<NpcController>().isSafe)
        {
            Dispatch(finalPos.position);
        }
        else
        {
            m_fsm.ChangeState("Patrol");
        }
        Attacking();
    }

    void Attacking()
    {
        attackable = Physics.OverlapSphere(transform.position, attackRadius, canChased);
        if (attackable.Length != 0 && hitNPCs[hitNPCs.Length - 1].gameObject == attackable[attackable.Length - 1].gameObject)
        {
            hasAttacked = true;
            navAgent.ResetPath();
            NpcController attackedNPC = attackable[attackable.Length - 1].gameObject.GetComponent<NpcController>();
            attackedNPC.TakeDamage(attackDamage);
            
            m_fsm.ChangeState("Rest");
        }
    }

    void Resting()
    {
        attackTime -= Time.deltaTime;
        if (attackTime <= 0)
        {
            attackTime = recordAttackTime;
            hasAttacked = false;
            m_fsm.ChangeState("Patrol");
        }
        
    }

    #endregion

    #region Swtich State

    public void readyForDispatch()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Dispatch");
    }
    private void Discover()
    {
        hitNPCs = Physics.OverlapSphere(transform.position, discoverRadius, canChased);

        if (hitNPCs.Length != 0 && !hasAttacked)
        {
            m_fsm.ChangeState("Chase");
        }
    }
    void lossTarget()
    {
        if (hitNPCs.Length == 0)
        {
            m_fsm.ChangeState("Patrol");
        }
    }



    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, discoverRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.maxX, 0, patrolRange.maxZ));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentPos, 1);
    }

    #endregion
}
