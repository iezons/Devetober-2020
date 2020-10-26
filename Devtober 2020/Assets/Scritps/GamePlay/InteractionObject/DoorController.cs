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
    Vector3 moveEnd = Vector3.zero;

    public bool isLocked;
    public bool isOperating;
    #endregion

    #region Value
    GameObject door;
    public bool isClosed, isOpened;
    public float timeLeft, timeInTotal;
    NavMeshObstacle navOb;
    #endregion

    private void Awake()
    {
        outline = GetComponent<Outline>();
        door = transform.GetChild(0).gameObject;
        navOb = door.GetComponent<NavMeshObstacle>();
    }

    private void Start()
    {
        AddMenu("SwitchStates", "Lock", false, TimeBreak, 1 << LayerMask.GetMask("Door"));
    }

    private void Update()
    {
        Detecting();
        Operation();
        navOb.enabled = isLocked;
        TimeCount();
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
        if (!isOperating)
        {
            isLocked = !isLocked;
            if (isLocked)
                rightClickMenus[0].functionName = "Unlock";
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

    public void TimeBreak(object obj)
    {
        timeLeft = timeInTotal;
        if (!isOperating && !isLocked)
        {
            isLocked = !isLocked;
            if (isLocked)
                rightClickMenus[0].functionName = "Unlock";
            else
                rightClickMenus[0].functionName = "Lock";
        }
    }
    public void TimeCount()
    {
        if(timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
        else
        {
            if (!isOperating && isLocked)
            {
                isLocked = !isLocked;
                if (isLocked)
                    rightClickMenus[0].functionName = "Unlock";
                else
                    rightClickMenus[0].functionName = "Lock";
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
