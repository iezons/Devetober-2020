using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

[System.Serializable]
public class Locators
{
    //public bool isTaken;
    public NpcController npc;
    public Transform Locator;
}

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
    [Header("Detecting Setting")]
    [SerializeField]
    DetectRange detectRange = null;
    [SerializeField]
    LayerMask canDetected = 0;

    [Header("Door Setting")]
    [SerializeField]
    Vector3 moveEnd = Vector3.zero;
    [SerializeField]
    [Range(0f, 50f)]
    float lerpTime = 0;

    [Header("Locking Setting")]
    [SerializeField]
    float lockTime = 0;

    [Header("Locator")]
    public List<LocatorList> Locators = new List<LocatorList>();

    [Header("Health")]
    public float maxHealth = 0;
    public float currentHealth = 0;
    public float preFixedHealth = 0;

    public bool isLocked;
    public bool isOperating;
    #endregion

    #region Value
    NavMeshObstacle navOb;
    GameObject door;
    Collider[] detectedObj = null;
    public bool isClosed, isOpened, isPowerOff, isFixing;
    [HideInInspector]
    public bool isNPCCalled = false;
    float recordLockTime;
    public CBordPos cBord = null;
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
        AddMenu("SwitchStates", "Lock", false, SwtichStates, 1 << LayerMask.GetMask("Door"));
    }

    private void Update()
    {
        navOb.enabled = isLocked;

        Detecting();
        Operation();

        if (!isPowerOff)
        {
            CheckCurrentStatus();
        }

        if (isPowerOff)
        {
            Fixing();
        }

        if(cBord != null)
        {
            if (cBord.isLocked && !isPowerOff)
            {
                RemoveAllMenu();
                isLocked = true;
                isPowerOff = true;
            }
            else if (currentHealth <= 0 && !isPowerOff && !cBord.isLocked)
            {
                RemoveAndInsertMenu("SwitchStates", "Repair", "Repair", true, SendFixingNPC, 1 << LayerMask.NameToLayer("NPC"));
                isLocked = true;
                isPowerOff = true;
            }
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
        isNPCCalled = false;
        if (!isOperating)
        {
            if (!isLocked && rightClickMenus[0].functionName == "Lock")
            {
                isLocked = true;
                rightClickMenus[0].functionName = "Unlock";
                lockTime = recordLockTime;
            }            
            else if(isLocked && rightClickMenus[0].functionName == "Unlock")
            {
                isLocked = false;
                rightClickMenus[0].functionName = "Lock";
            }
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
        if(rightClickMenus.Count !=0)
        {
            if (isLocked && !isPowerOff && !isNPCCalled)
            {
                lockTime -= Time.deltaTime;
                if (lockTime <= 0)
                {
                    isLocked = !isLocked;
                    isNPCCalled = true;
                    lockTime = recordLockTime;
                    rightClickMenus[0].functionName = "Lock";
                }
            }

            if (isLocked && rightClickMenus[0].functionName != "Unlock")
            {
                rightClickMenus[0].functionName = "Unlock";
            }
            else if (!isLocked && rightClickMenus[0].functionName != "Lock")
            {
                rightClickMenus[0].functionName = "Lock";
            }
        }   
    }

    void SendFixingNPC(object obj)
    {    
        GameObject gameObj = (GameObject)obj;
        NpcController npc = gameObj.GetComponent<NpcController>();

        float minDistance = Mathf.Infinity;
        for (int i = 0; i < Locators.Count; i++)
        {
            float a = Locators[i].Locator.position.x - gameObj.transform.position.x;
            float b = Locators[i].Locator.position.z - gameObj.transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            float distance = Mathf.Abs(c);
            print(distance + Locators[i].Locator.name);

            if (distance < minDistance)
            {
                minDistance = distance;
                npc.fixTargetTransform = Locators[i].Locator;
            }
        }
        Debug.Log("Got a Fix Guy");
        npc.fixTarget = gameObject;
        npc.HasInteract = false;
        //print(npc.fixTargetTransform.name);
        npc.Dispatch(npc.fixTargetTransform.position);
        npc.TriggerFixing();
    }

    void Fixing()
    {
        if(isFixing)
        {
            if (currentHealth >= maxHealth)
            {
                Debug.Log("Fixed");
                currentHealth = maxHealth;
                if (cBord != null)
                {
                    if (!cBord.isPowerOff)
                    {
                        isLocked = false;
                        isPowerOff = false;
                        isFixing = false;
                    }
                }           
                else if(isActivated())
                {
                    isLocked = false;
                }
                else
                {
                    currentHealth = preFixedHealth;
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
    }
    #endregion
}
