using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class RoomTracker : MonoBehaviour
    {
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
        LayerMask canItemTracked = 0;

        [SerializeField]
        LayerMask canRoomTracked = 0;

        [SerializeField]
        Collider[] hitItem;

        [SerializeField]
        RaycastHit hitRoom;

        [SerializeField]
        ScaleRate scaleRate = null;


        private void Update()
        {
            Tracking();
            foreach(var stuff in Item())
            {
                print(stuff.ToString());
            }
            if(hitRoom.collider != null)
            {
                print(RoomNumber());
            }
        }

        private void Tracking()
        {
            //Track Object in the area
            hitItem = Physics.OverlapBox(transform.position, new Vector3 (scaleRate.x, scaleRate.y, scaleRate.z)/2, Quaternion.identity, canItemTracked);
            //Track Room Number
            Physics.Raycast(transform.position, -transform.up, out hitRoom, scaleRate.y / 2, canRoomTracked);
        }

        public List<GameObject> Item()
        {
            List<GameObject> tempItem = new List<GameObject>();
            foreach (Collider item in hitItem)
            {
                tempItem.Add(item.gameObject);
            }
            return tempItem;
        }

        public string RoomNumber()
        {
            return hitRoom.collider.gameObject.tag.ToString();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(scaleRate.x, scaleRate.y, scaleRate.z));

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, -transform.up * scaleRate.y/2);
        }
    }
}

