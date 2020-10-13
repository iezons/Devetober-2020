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

        //[SerializeField]
        //LayerMask canRoomTracked = 0;

        [SerializeField]
        LayerMask reportEmergency = 0;

        [SerializeField]
        Collider[] hitInformation;

        #endregion


        #region Value

        //RaycastHit hitRoom;

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
            ////Track Room Number
            //Physics.Raycast(transform.position, -transform.up, out hitRoom, scaleRate.y / 2, canRoomTracked);
        }

        #region Information Pool
        public List<GameObject> AllObjs()
        {
            List<GameObject> tempobjs = new List<GameObject>();
            foreach (Collider temp in hitInformation)
            {
                tempobjs.Add(temp.gameObject);
            }
            return tempobjs;
        }

        public List<GameObject> Enemy()
        {
            List<GameObject> tempObjs = new List<GameObject>();
            foreach (GameObject temp in AllObjs())
            {
                if (temp.layer == 10)
                    tempObjs.Add(temp);
            }
            return tempObjs;
        }

        public List<GameObject> NPC()
        {
            List<GameObject> tempObjs = new List<GameObject>();
            foreach (GameObject temp in AllObjs())
            {
                if (temp.layer == 8)
                    tempObjs.Add(temp);
            }
            return tempObjs;
        }

        public List<GameObject> HiddenPos()
        {
            List<GameObject> tempObjs = new List<GameObject>();
            foreach (GameObject temp in AllObjs())
            {
                if (temp.layer == 11)
                    tempObjs.Add(temp);
            }
            return tempObjs;
        }

        public string RoomName()
        {
            return this.gameObject.name;
        }

        public GameObject Room()
        {
            return this.gameObject;
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

