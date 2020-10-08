using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class RoomTracker : MonoBehaviour
    {

        #region Inspector View

        [System.Serializable]
        public class ScaleRate
        {
            [Range(0f, 100f)]
            public float x = 0;
            [Range(0f, 100f)]
            public float y = 0;
            [Range(0f, 100f)]
            public float z = 0;
        }
        [SerializeField]
        ScaleRate scaleRate = null;

        [SerializeField]
        LayerMask canDetected = 0;

        [SerializeField]
        LayerMask canRoomTracked = 0;

        [SerializeField]
        LayerMask reportEmergency = 0;

        [SerializeField]
        Collider[] hitInformation;

        #endregion


        #region Value

        RaycastHit hitRooms;

        bool tempCheck;

        #endregion

        public void Start()
        {
            EventCenter.GetInstance().EventTriggered("GM.Room.Add", this);
        }

        private void Update()
        {
            Detecting();
        }

        private void Detecting()
        {
            //Track Object in the area
            hitInformation = Physics.OverlapBox(transform.position, new Vector3 (scaleRate.x, scaleRate.y, scaleRate.z)/2, Quaternion.identity, canDetected);
            //Track Room Number
            Physics.Raycast(transform.position, -transform.up, out hitRooms, scaleRate.y / 2, canRoomTracked);
        }

        #region Information Pool
        public List<GameObject> Item()
        {
            List<GameObject> tempItem = new List<GameObject>();
            foreach (Collider item in hitInformation)
            {
                tempItem.Add(item.gameObject);
            }
            return tempItem;
        }

        public string RoomNumber()
        {
            return hitRooms.collider.gameObject.tag.ToString();
        }

        public GameObject Room()
        {
            return hitRooms.collider.gameObject;
        }

        public bool isEnemyDetected()
        {
            for (int i = 0; i < hitInformation.Length; i++)
            {
                if (hitInformation[i].gameObject.layer == Mathf.Log(reportEmergency.value, 2))
                {
                    tempCheck = true;
                    break;
                }
                else
                {
                    tempCheck = false;
                }          
            }
            return tempCheck;
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(scaleRate.x, scaleRate.y, scaleRate.z));

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, -transform.up * scaleRate.y/2);
        }
        #endregion
    }
}

