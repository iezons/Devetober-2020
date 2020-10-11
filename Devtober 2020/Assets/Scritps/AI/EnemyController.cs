using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    Collider[] hitObjects;
    [SerializeField]
    Collider[] attackable;

    [System.Serializable]
    public class PatrolRange
    {
        [Range(0f, 50f)]
        public float maxX = 0;
        [Range(0f, 50f)]
        public float minX = 0;
        [Range(0f, 50f)]
        public float maxZ = 0;
        [Range(0f, 50f)]
        public float minZ = 0;
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
        if (Vector3.Distance(currentPos, transform.position) < 1 || !navAgent.CalculatePath(currentPos, path)
            //|| currentPos.x > transform.position.x - patrolRange.minX / 2
            //|| currentPos.x < transform.position.x + patrolRange.minX / 2
            //|| currentPos.z > transform.position.z - patrolRange.minZ / 2
            //|| currentPos.z < transform.position.x + patrolRange.minZ / 2 
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
        //List<float> distanceBetweenNPC = new List<float>();

        //for (int i = 0; i < hitObjects.Length; i++)
        //{
        //    float distance = Vector3.Distance(transform.position, hitObjects[i].transform.position);
        //    distanceBetweenNPC.Add(distance);
        //}

        navAgent.SetDestination(hitObjects[hitObjects.Length-1].transform.position);
        Attacking();
    }

    void Attacking()
    {
        attackable = Physics.OverlapSphere(transform.position, attackRadius, canChased);
        if (attackable.Length != 0 && hitObjects[hitObjects.Length - 1].gameObject == attackable[attackable.Length - 1].gameObject)
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
        hitObjects = Physics.OverlapSphere(transform.position, discoverRadius, canChased);

        if (hitObjects.Length != 0 && !hasAttacked )
        {
            m_fsm.ChangeState("Chase");
        }
    }
    void lossTarget()
    {
        if (hitObjects.Length == 0)
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.minX, 0, patrolRange.minZ));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentPos, 1);
    }

    #endregion
}
