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
        AddMenu("SwitchStates", "Lock", false, SwtichStates, 1 << LayerMask.GetMask("Door"));
    }

    private void Update()
    {
        Detecting();
        Operation();
        navOb.enabled = isLocked;
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
            float a = door.transform.position.x - (moveEnd.x + transform.position.x);
            float b = door.transform.position.z - (moveEnd.z + transform.position.z);
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            isOpened = Mathf.Abs(c) < 0.1f ? true : false; ;
            isClosed = false;
        }
        else if (!isActivated() && !isClosed || isLocked)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, transform.position, lerpTime * Time.deltaTime);
            float a = door.transform.position.x - transform.position.x;
            float b = door.transform.position.z - transform.position.z;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            isOperating = Mathf.Abs(c) > 0.1f ? true : false;
            isClosed = !isOperating;
            isOpened = false;
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
