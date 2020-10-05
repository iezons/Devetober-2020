using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public bool isOpen;
    #endregion

    #region Value
    GameObject door;
    public class RightClickMenus
    {
        public string functionName;
        public UnityAction function;
    }
    List<RightClickMenus> rightClickMenus = new List<RightClickMenus>();
    #endregion


    private void Awake()
    {
        door = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        rightClickMenus.Add(new RightClickMenus { functionName = "SwtichStates", function = SwtichStates });
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

    //public bool checkState()
    //{

    //}

    public void SwtichStates()
    {
        isLocked = !isLocked;
    }

    public void Operation()
    {
        if(isActivated() && !isLocked)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, moveEnd, lerpTime * Time.deltaTime);
        }
        else if (!isActivated() && !isLocked)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, transform.position, lerpTime * Time.deltaTime);
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
