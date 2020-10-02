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

    public Vector3 newDestination()
    {
        float x = Random.Range(transform.position.x - patrolRange.x, transform.position.x + patrolRange.x);
        float z = Random.Range(transform.position.z - patrolRange.z, transform.position.z - patrolRange.z);

        Vector3 tempPos = new Vector3(x, transform.position.y , z);
        return tempPos;
    }

    NavMeshAgent navAgent;
    NavMeshPath path;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }

    private void Update()
    {
        if (!navAgent.CalculatePath(newDestination(), path))
        {
            generateNewDestination();
        }
    }

    private void generateNewDestination()
    {
        navAgent.SetDestination(newDestination());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.x, 0, patrolRange.z));
    }
}
