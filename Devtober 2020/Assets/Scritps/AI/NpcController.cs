using EvtGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay;


[RequireComponent(typeof(NavMeshAgent))]
public class NpcController : MonoBehaviour
{
    #region Inspector View
    [System.Serializable]
    public class Status
    {
        public string npcName;
        public string description;

        public int maxHealth = 0;
        public int currentHealth = 0;

        public int maxStamina = 0;
        public float currentStamina = 0;

        public int maxCredit = 0;
        public int currentCredit = 0;

        public List<EventSO> toDoList;
    }
    [SerializeField]
    Status status = null;

    [System.Serializable]
    public class PatrolRange
    {
        [Range(0f, 50f)]
        public float maxX = 0;
        [Range(0f, 50f)]
        public float minX = 0;
        [Range(0f, 50f)]
        public float y = 0;
        [Range(0f, 50f)]
        public float maxZ = 0;
        [Range(0f, 50f)]
        public float minZ = 0;
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

    #endregion


    #region Fields
    StringRestrictedFiniteStateMachine m_fsm;
    Animator animator;

    public NPC_SO npc_so;


    NavMeshAgent navAgent;
    NavMeshPath path;
    #endregion


    #region Value
    [HideInInspector]
    public Vector3 currentTerminalPos;

    public List<RightClickMenus> rightClickMenus = new List<RightClickMenus>();
    Transform finalPos;
    #endregion



    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        animator = GetComponent<Animator>();
        npc_so.toDoList.Clear();

        #region StringRestrictedFiniteStateMachine
        Dictionary<string, List<string>> NPCDictionary = new Dictionary<string, List<string>>()
        {
            { "Patrol", new List<string> { "Rest", "Event", "Dispatch", "Dodging", "Hiding", "Escaping" } },
            { "Rest", new List<string> { "Patrol", "Event", "Dispatch", "Dodging", "Hiding", "Escaping" } },
            { "Event", new List<string> { "Patrol", "Rest", "Dispatch", "Dodging", "Hiding", "Escaping" } },
            { "Dispatch", new List<string> { "Patrol", "Rest", "Event", "Dodging", "Hiding", "Escaping" } },
            { "Dodging", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Hiding", "Escaping" } },
            { "Hiding", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Escaping" } },
            { "Escaping", new List<string> { "Patrol", "Rest", "Event", "Dispatch", "Dodging", "Hiding" } }
        };

        m_fsm = new StringRestrictedFiniteStateMachine(NPCDictionary, "Patrol");
        #endregion
    }

    private void Start()
    {
        currentTerminalPos = NewDestination();
        EventCenter.GetInstance().EventTriggered("GM.NPC.Add", this);
        AddMenu("Move", "Move", Dispatch);

        
    }

    public void AddMenu(string unchangedName, string functionName, MenuHandler function)
    {
        rightClickMenus.Add(new RightClickMenus { unchangedName = unchangedName });
        rightClickMenus[rightClickMenus.Count - 1].functionName = functionName;
        rightClickMenus[rightClickMenus.Count - 1].function += function;
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
                animator.SetFloat("Ground", 0);
                //Rest();
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
                CompleteHiding();
                break;
            case "Escaping":
                Escaping();
                break;
            default:
                break;
        }
        #endregion

        //CheckEvent();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerHiding();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            BackToPatrol();
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

    private void GenerateNewDestination()
    {
        if (Vector3.Distance(currentTerminalPos, transform.position) < 1 || !navAgent.CalculatePath(currentTerminalPos, path) 
            //|| currentPos.x > transform.position.x - patrolRange.minX / 2
            //|| currentPos.x < transform.position.x + patrolRange.minX / 2
            //|| currentPos.z > transform.position.z - patrolRange.minZ / 2
            //|| currentPos.z < transform.position.x + patrolRange.minZ / 2 
            )
        {
            currentTerminalPos = NewDestination();
        }
    }

    public void Dispatch(object newPos)
    {
        navAgent.SetDestination((Vector3)newPos);
        animator.SetFloat("Ground", 1);
    }


    #endregion

    #region Special Action
    private void Rest()
    {
        navAgent.ResetPath();
        if (npc_so.currentStamina >= npc_so.maxStamina)
        {
            npc_so.currentStamina = npc_so.maxStamina;
            BackToPatrol();
        }
    }

    private void Dodging()
    {
        hitObjects = Physics.OverlapSphere(transform.position, alertRadius, needDodged);

        for (int i = 0; i< hitObjects.Length; i++)
        {
            Vector3 enemyDirection = (transform.position - hitObjects[i].gameObject.transform.position).normalized;
            Vector3 movingDirection = (currentTerminalPos- transform.position).normalized;

            if (Vector3.Angle(enemyDirection, movingDirection) > dodgeAngle / 2 || Vector3.Distance(currentTerminalPos, transform.position) < 1 || !navAgent.CalculatePath(currentTerminalPos, path))
            {
                currentTerminalPos = NewDestination();
            }
        }
        Dispatch(currentTerminalPos);

        if(hitObjects.Length == 0)
        {
            BackToPatrol();
        }
    }

    private void Hiding()
    {
        List<RoomTracker> rooms = new List<RoomTracker>();
        foreach (RoomTracker temp in GameManager.GetInstance().Rooms)
        {
            rooms.Add(temp);
        }

        List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i< rooms.Count; i++)
        {
            foreach(GameObject temp in rooms[i].HiddenPos())
            {
                objs.Add(temp);
            }
        }

        float minDistance = Mathf.Infinity;
        foreach (GameObject temp in objs)
        {
            float distance = Vector3.Distance(transform.position, temp.transform.position);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                finalPos = temp.transform;
            }
        }
        Dispatch(finalPos.position);
    }

    void Escaping()
    {
        List<RoomTracker> rooms = new List<RoomTracker>();
        foreach (RoomTracker temp in GameManager.GetInstance().Rooms)
        {
            rooms.Add(temp);
        }

        List<string> names = new List<string>();
        for (int i = 0; i < rooms.Count; i++)
        {
            names.Add(rooms[i].RoomName());
        }

        RaycastHit hitrooms;
        Physics.Raycast(transform.position, -transform.up, out hitrooms, patrolRange.y, 9);
        
        for(int i = 0; i < names.Count; i++)
        {
            if(hitrooms.collider.gameObject.name == names[i])
            {

            }
        }
    }

    private void Event()
    {
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
                            Dispatch(evt.NPCTalking[a].MoveToClassA.MoveTO);
                        }
                        else if (evt.NPCTalking[a].MoveToClassB.Name == npc_so.npcName)
                        {
                            Dispatch(evt.NPCTalking[a].MoveToClassB.MoveTO);
                        }
                    }
                    break;
                case DoingWithNPC.MoveTo:
                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                    {
                        if (evt.NPCWayPoint[a].Name == npc_so.npcName)
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
            npc_so.toDoList.Remove(evt);
        }
    }

    #endregion

    #region Swtich State
    public void ReadyForDispatch()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Dispatch");
    }

    public void BackToPatrol()
    {
        m_fsm.ChangeState("Patrol");
    }

    public void TriggerEvent()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Event");
    }

    public void TriggerDodging()
    {
        hitObjects = Physics.OverlapSphere(transform.position, alertRadius, needDodged);
        if (hitObjects.Length != 0)
        {
            m_fsm.ChangeState("Dodging");
        }
    }
    public void TriggerHiding()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Hiding");
    }

    public void TriggerEscaping()
    {
        navAgent.ResetPath();
        m_fsm.ChangeState("Escaping");
    }

    public void CompleteHiding()
    {
        if (Mathf.Abs(navAgent.destination.x - navAgent.nextPosition.x) <= 1 && Mathf.Abs(navAgent.destination.z - navAgent.nextPosition.z) <= 1)
        {
            navAgent.ResetPath();
            m_fsm.ChangeState("Rest");
        }
    }
    
    public void CheckEvent()
    {
        if(npc_so.toDoList != null)
        {
            if(npc_so.toDoList.Count != 0)
            {
                m_fsm.ChangeState("Event");
            }
        }
    }

    public void ReachDestination()
    {
        if(Mathf.Abs(navAgent.destination.x - navAgent.nextPosition.x) <= 1 && Mathf.Abs(navAgent.destination.z - navAgent.nextPosition.z) <= 1)
        {
            EventCenter.GetInstance().EventTriggered("GM.AllNPCArrive", npc_so.npcName);
        }
    }


    #endregion

    #region Status Change
    public void ApplyHealth(int healthAmount)
    {
        status.currentHealth = status.currentHealth + healthAmount > status.maxHealth ? status.maxHealth : status.currentHealth += healthAmount;
    }

    public void ApplyStamina(int staminaAmount)
    {
        status.currentStamina = status.currentStamina + staminaAmount > status.maxStamina ? status.maxStamina : status.currentStamina += staminaAmount;
    }

    public void ApplyCredit(int creditAmount)
    {
        status.currentCredit = status.currentCredit + creditAmount > status.maxCredit ? status.maxCredit : status.currentCredit += creditAmount;
    }


    public void TakeDamage(int damageAmount)
    {
        status.currentHealth -= damageAmount;

        if (status.currentHealth <= 0)
        {
            //Death();
        }
    }

    public void ConsumeStamina(int staminaAmount)
    {
        status.currentStamina = status.currentStamina - staminaAmount <= 0 ? 0 : status.currentStamina -= staminaAmount;
    }

    public void ReduceCredit(int creditAmount)
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
        Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange.minX, 0, patrolRange.minZ));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentTerminalPos, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * patrolRange.y);
    }
    #endregion
}