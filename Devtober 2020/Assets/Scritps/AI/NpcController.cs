using DiaGraph;
using EvtGraph;
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

    StringRestrictedFiniteStateMachine m_fsm;

    public NPC_SO npc_so;

    [HideInInspector]
    public Vector3 currentPos;

    public Vector3 NewDestination()
    {
        float x = Random.Range(transform.position.x - patrolRange.maxX / 2, transform.position.x + patrolRange.maxX / 2);
        float z = Random.Range(transform.position.z - patrolRange.maxZ / 2, transform.position.z + patrolRange.maxZ / 2);

        Vector3 tempPos = new Vector3(x, transform.position.y, z);     
        return tempPos;
    }

    NavMeshAgent navAgent;
    NavMeshPath path;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        npc_so.toDoList.Clear();
        #region StringRestrictedFiniteStateMachine
        Dictionary<string, List<string>> NPCDictionary = new Dictionary<string, List<string>>()
        {
            { "Patrol", new List<string> { "Rest", "Event1", "Dispatch" } },
            { "Rest", new List<string> { "Patrol", "Event1", "Dispatch" } },
            { "Event1", new List<string> { "Patrol", "Rest", "Dispatch" } },
            { "Dispatch", new List<string> { "Patrol", "Rest", "Event1" } },
        };

        m_fsm = new StringRestrictedFiniteStateMachine(NPCDictionary, "Patrol");
        #endregion
    }

    private void Start()
    {
        currentPos = NewDestination();
        EventCenter.GetInstance().EventTriggered("GM.NPC.Add", this);
        TriggerEvent1();
    }

    private void Update()
    {
        #region StringRestrictedFiniteStateMachine Update
        switch (m_fsm.GetCurrentState())
        {
            case "Patrol":
                GenerateNewDestination();
                break;
            case "Rest":
                Rest();
                break;
            case "Event1":
                print("Start Event");
                 break;
            default:
                break;
        }
        #endregion
    }

    private void GenerateNewDestination()
    {
        navAgent.SetDestination(currentPos);
        npc_so.ConsumeStamina();

        if (Vector3.Distance(currentPos, transform.position) < 1 || !navAgent.CalculatePath(currentPos, path) 
            //|| currentPos.x > transform.position.x - patrolRange.minX / 2
            //|| currentPos.x < transform.position.x + patrolRange.minX / 2
            //|| currentPos.z > transform.position.z - patrolRange.minZ / 2
            //|| currentPos.z < transform.position.x + patrolRange.minZ / 2 
            )
        {
            currentPos = NewDestination();
        }

        if(npc_so.currentStamina <= 0)
        {
            m_fsm.ChangeState("Rest");
        }
    }

    private void Rest()
    {
        navAgent.ResetPath();
        npc_so.RecoverStamina();
        if (npc_so.currentStamina >= npc_so.maxStamina)
        {
            npc_so.currentStamina = npc_so.maxStamina;
            m_fsm.ChangeState("Patrol");
        }
    }

    public void ReadyForDispatch()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Dispatch");
    }

    public void Dispatch(Vector3 newPos)
    {
        navAgent.SetDestination(newPos);
    }

    public void BackToPatrol()
    {
        m_fsm.ChangeState("Patrol");
    }

    public void TriggerEvent1()
    {
        if (npc_so.toDoList != null)
        {
            m_fsm.ChangeState("Event1");
        }
        for (int i = 0; i < npc_so.toDoList.Count; i++)
        {
            EventSO evt = npc_so.toDoList[0];
            switch (evt.doingWithNPC)
            {
                case DoingWithNPC.Talking:
                    for (int a = 0; a < evt.NPCTalking.Count; a++)
                    {
                        if (evt.NPCTalking[a].MoveToClassA.Name == npc_so.npcName)
                        {
                            //Move(evt.NPCTalking[a].MoveToClassA.MoveTO)
                            //When Move Finsh, Talking()
                        }
                        else if (evt.NPCTalking[a].MoveToClassB.Name == npc_so.npcName)
                        {
                            //Move(evt.NPCTalking[a].MoveToClassB.MoveTO)
                            //When Move Finsh, Talking()
                        }
                    }
                    break;
                case DoingWithNPC.MoveTo:
                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                    {
                        if (evt.NPCWayPoint[a].Name == npc_so.npcName)
                        {
                            //Move(evt.NPCWayPoint[a].MoveTO)
                        }
                    }
                    break;
                case DoingWithNPC.Patrol:
                    break;
                default:
                    break;
            }
            npc_so.toDoList.Remove(evt);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.maxX, 0, patrolRange.maxZ));
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.minX, 0, patrolRange.minZ));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentPos, 1);
    }
}