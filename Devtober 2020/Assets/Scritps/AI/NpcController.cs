using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcController : MonoBehaviour
{
    [System.Serializable]
    public class PatrolRange
    {
        [Range(0f, 50f)]
        public float x = 0;
        [Range(0f, 50f)]
        public float z = 0;
    }

    [SerializeField]
    PatrolRange patrolRange = null;

    StringRestrictedFiniteStateMachine m_fsm;

    Vector3 currentPos;

    public Vector3 newDestination()
    {
        float x = Random.Range(transform.position.x - patrolRange.x / 2, transform.position.x + patrolRange.x / 2);
        float z = Random.Range(transform.position.z - patrolRange.z / 2, transform.position.z + patrolRange.z / 2);

        Vector3 tempPos = new Vector3(x, transform.position.y , z);
        return tempPos;
    }

    NavMeshAgent navAgent;
    NavMeshPath path;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();

        #region StringRestrictedFiniteStateMachine
        Dictionary<string, List<string>> stateDictionary = new Dictionary<string, List<string>>()
        {
            { "Patrol", new List<string> { "Running" } },
            { "FindNewPath", new List<string> { "Idle" } },
        };

        m_fsm = new StringRestrictedFiniteStateMachine(stateDictionary, "Idle");
        #endregion

    }

    private void Update()
    {
        navAgent.SetDestination(currentPos);

        #region StringRestrictedFiniteStateMachine Update
        switch (m_fsm.GetCurrentState())
        {
            case "Idle":
                break;
            case "Running":
                break;
            default:
                break;
        }
        #endregion
    }

    public Vector3 generateNewDestination()
    {
        if(Vector3.Distance(currentPos, transform.position) <1 || !navAgent.CalculatePath(newDestination(), path))
        {
            currentPos = newDestination();
        }
        return currentPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.x, 0, patrolRange.z));
    }
}
