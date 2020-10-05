using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
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

    private void Update()
    {
        Detecting();
        foreach(Collider col in detectedObj)
        {
            print(col.ToString());
        }
    }

    private void Detecting()
    {
        detectedObj = Physics.OverlapBox(transform.position, new Vector3(detectRange.x, detectRange.y, detectRange.z)/2, Quaternion.identity, canDetected);
    }



    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectRange.x, detectRange.y, detectRange.z));        
    }

    #endregion
}
