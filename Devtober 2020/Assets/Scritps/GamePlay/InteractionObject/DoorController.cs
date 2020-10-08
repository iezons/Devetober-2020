using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public delegate void MenuHandler(object obj);
public class RightClickMenus
{
    public string unchangedName;
    public string functionName;
    public event MenuHandler function;
    public void DoFunction(object obj)
    {
        function(obj);
    }

}

public class DoorController : MonoBehaviour
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
    
    public List<RightClickMenus> rightClickMenus = new List<RightClickMenus>();
    #endregion


    private void Awake()
    {
        door = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        AddMenu("SwitchStates", "Lock", SwtichStates);
    }

    public void AddMenu(string unchangedName, string functionName, MenuHandler function)
    {
        rightClickMenus.Add(new RightClickMenus { unchangedName = unchangedName });
        rightClickMenus[rightClickMenus.Count - 1].functionName = functionName;
        rightClickMenus[rightClickMenus.Count - 1].function += function;
    }

    private void Update()
    {
        Detecting();
        Operation();
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
        Debug.Log("LLLLLOCK");
        if (!isOperating)
        {
            isLocked = !isLocked;
            if (isLocked)
                rightClickMenus[0].functionName = "Unlock";
            else
                rightClickMenus[0].functionName = "lock";
        }
    }

    public void Operation()
    {
        if (isActivated() && !isLocked && !isOpened)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, moveEnd, lerpTime * Time.deltaTime);
            isOperating = Vector3.Distance(door.transform.position, moveEnd) > 0.1f ? true : false;
            isOpened = !isOperating;
            isClosed = false;
        }
        else if (!isActivated() && !isLocked && !isClosed)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, transform.position, lerpTime * Time.deltaTime);
            isOperating = Vector3.Distance(door.transform.position, transform.position) > 0.1f ? true : false;
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
        Gizmos.DrawWireSphere(moveEnd, 1);
    }
    #endregion
}
