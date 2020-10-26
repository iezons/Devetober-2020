using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

[RequireComponent(typeof(Outline))]
public class DoorController : ControllerBased
{
    #region Inspector View
    [System.Serializable]
    public class DetectRange
    {
        [Range(0f, 50f)]
        public float x = 0;
        [Range(0f, 50f)]
        public float y = 0;
        [Range(0f, 50f)]
        public float z = 0;
    }
    [SerializeField]
    DetectRange detectRange = null;

    [SerializeField]
    LayerMask canDetected = 0;

    [SerializeField]
    Collider[] detectedObj = null;

    [SerializeField]
    [Range(0f, 50f)]
    float lerpTime = 0;

    [SerializeField]
    float lockTime = 0;

    [SerializeField]
    Vector3 moveEnd = Vector3.zero;

    [SerializeField]
    public CBordPos cBord;

    [Header("Health")]
    public float maxHealth = 0;
    public float currentHealth = 0;

    public bool isLocked;
    public bool isOperating;
    #endregion

    #region Value
    GameObject door, fixNPC;
    NpcController npc;
    public bool isClosed, isOpened, isPowerOff, isFixing;
    NavMeshObstacle navOb;
    float recordLockTime;
    bool isNPCCalled = false;
    #endregion

    private void Awake()
    {
        outline = GetComponent<Outline>();
        door = transform.GetChild(0).gameObject;
        navOb = door.GetComponent<NavMeshObstacle>();
        recordLockTime = lockTime;
    }

    private void Start()
    {
        AddMenu("SwitchStates", "Lock", false, SwtichStates, 1 << LayerMask.GetMask("Door"), false);
    }

    private void Update()
    {
        navOb.enabled = isLocked;

        Detecting();
        Operation();
        CheckCurrentStatus();
        if (isPowerOff)
        {
            Fixing();
        }
    }

    private void Detecting()
    {
        detectedObj = Physics.OverlapBox(transform.position, new Vector3(detectRange.x, detectRange.y, detectRange.z)/2, Quaternion.identity, canDetected);
    }

    public bool isActivated()
    {
        if(detectedObj.Length != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SwtichStates(object obj)
    {
        DefaultValueWithGO temp = (DefaultValueWithGO)obj;
        isNPCCalled = (bool)temp.DefaultValue;
        if (!isOperating)
        {
            isLocked = !isLocked;
            if (isLocked)
            {
                rightClickMenus[0].functionName = "Unlock";
                lockTime = recordLockTime;
            }            
            else
                rightClickMenus[0].functionName = "Lock";
        }
    }

    public void Operation()
    {
        if (isActivated() && !isLocked && !isOpened)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, moveEnd + transform.position, lerpTime * Time.deltaTime);
            isOpened = moveEnd.y + transform.position.y - door.transform.position.y < 0.1f ? true : false; ;
            isClosed = false;
        }
        else if (!isActivated() && !isClosed || isLocked)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, transform.position, lerpTime * Time.deltaTime);
            isOperating = door.transform.position.y - transform.position.y > 0.1f ? true : false;
            isClosed = !isOperating;
            isOpened = false;
        }
    }
    
    void CheckCurrentStatus()
    {
        if (isLocked && !isPowerOff && !isNPCCalled)
        {
            lockTime -= Time.deltaTime;
            if (lockTime <= 0)
            {
                isLocked = !isLocked;
                lockTime = recordLockTime;
            }
        }

        if (currentHealth <= 0 && !isPowerOff)
        {
            RemoveAndInsertMenu("SwitchStates", "Repair", "Repair", true, SendFixingNPC, 1 << LayerMask.NameToLayer("NPC"));
            isLocked = true;
            isPowerOff = true;
        }
    }

    void SendFixingNPC(object obj)
    {
        Debug.Log("Got a Fix Guy");
        GameObject gameObj = (GameObject)obj;
        fixNPC = gameObj;
        npc = gameObj.GetComponent<NpcController>();
        npc.Dispatch(transform.position);
    }

    void Fixing()
    {
        if (!isFixing)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale, Quaternion.identity, 1 << LayerMask.NameToLayer("NPC"));
            foreach (var item in hits)
            {
                if (item.gameObject == fixNPC)
                {
                    npc.TriggerFixing();
                    isFixing = true;
                }
            }
        }
        else
        {
            currentHealth += 10 * Time.deltaTime;
            if (currentHealth >= maxHealth)
            {
                Debug.Log("Fixed");
                currentHealth = maxHealth;
                if (cBord.currentHealth > 0)
                {                    
                    isLocked = false;
                    isPowerOff = false;
                    isFixing = false;
                    RemoveAndInsertMenu("Repair", "SwitchStates", "Lock", false, SwtichStates, 1 << LayerMask.GetMask("Door"));
                }
                else if(isActivated())
                {
                    isLocked = false;
                }
                else
                {
                    currentHealth = 80;
                    isFixing = false;
                    isLocked = true;
                }
            }
        }
    }

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectRange.x, detectRange.y, detectRange.z));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(moveEnd+transform.position, 1);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale / 2);
    }
    #endregion
}
