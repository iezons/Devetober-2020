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
        Collider[] hitItems;

        [SerializeField]
        RaycastHit hitRooms;

        [SerializeField]
        ScaleRate scaleRate = null;


        private void Update()
        {
            Tracking();
            foreach(var stuff in Item())
            {
                print(stuff.ToString());
            }
            if(hitRooms.collider != null)
            {
                print(RoomNumber());
            }
        }

        private void Tracking()
        {
            //Track Object in the area
            hitItems = Physics.OverlapBox(transform.position, new Vector3 (scaleRate.x, scaleRate.y, scaleRate.z)/2, Quaternion.identity, canItemTracked);
            //Track Room Number
            Physics.Raycast(transform.position, -transform.up, out hitRooms, scaleRate.y / 2, canRoomTracked);
        }

        public List<GameObject> Item()
        {
            List<GameObject> tempItem = new List<GameObject>();
            foreach (Collider item in hitItems)
            {
                tempItem.Add(item.gameObject);
            }
            return tempItem;
        }

        public string RoomNumber()
        {
            return hitRooms.collider.gameObject.tag.ToString();
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

