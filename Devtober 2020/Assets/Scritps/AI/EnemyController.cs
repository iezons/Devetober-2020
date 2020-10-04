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
    LayerMask canChased = 0;

    [SerializeField]
    Collider[] hitObjects;

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
    #endregion





    public Vector3 NewDestination()
    {
        float x = Random.Range(transform.position.x - patrolRange.maxX / 2, transform.position.x + patrolRange.maxX / 2);
        float z = Random.Range(transform.position.z - patrolRange.maxZ / 2, transform.position.z + patrolRange.maxZ / 2);

        Vector3 tempPos = new Vector3(x, transform.position.y, z);
        return tempPos;
    }

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
    }


    private void Update()
    {
        #region StringRestrictedFiniteStateMachine Update
        switch (m_fsm.GetCurrentState())
        {
            case "Patrol":
                GenerateNewDestination();
                FindNPC();
                break;
            case "Chase":
                Chasing();
                FindNPC();
                break;
            case "Rest":
                break;
            case "Dispatch":
                FindNPC();
                break;
            default:
                break;
        }
        #endregion

        Discover();
    }

    private void GenerateNewDestination()
    {
        navAgent.SetDestination(currentPos);

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

    private void Discover()
    {
        hitObjects = Physics.OverlapSphere(transform.position, discoverRadius, canChased);
    }

    public void readyForDispatch()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Dispatch");
    }

    public void Dispatch(Vector3 newPos)
    {
        navAgent.SetDestination(newPos);
    }

    private void FindNPC()
    {
        if(Physics.CheckSphere(transform.position, discoverRadius, canChased))
        {
            m_fsm.ChangeState("Chase");
        }
        else
        {
            m_fsm.ChangeState("Patrol");
        }
    }

    public void Chasing()
    {
        //List<float> Distance = new List<float>();

        //for(int i =0; i < hitObjects.Length; i++)
        //{
        //    float distance = Vector3.Distance(transform.position, hitObjects[i].transform.position);
        //    Distance.Add(distance);
        //}

        navAgent.SetDestination(hitObjects[0].transform.position);
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, discoverRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.maxX, 0, patrolRange.maxZ));
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.minX, 0, patrolRange.minZ));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentPos, 1);
    }
}
