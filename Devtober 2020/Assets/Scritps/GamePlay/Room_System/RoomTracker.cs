using DiaGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    [RequireComponent(typeof(DialoguePlay))]
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

        public Camera RoomCamera;

        public DialoguePlay DiaPlay;
        public DialogueGraph WaitingGraph;
        public string HistoryText;
        public Dictionary<string, bool> NPCAgentList = new Dictionary<string, bool>();
        public List<OptionClass> OptionList = new List<OptionClass>();

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
        public List<GameObject> temp = new List<GameObject>();
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
                if (temp.layer == LayerMask.NameToLayer("Enemy"))
                    tempObjs.Add(temp);
            }
            return tempObjs;
        }

        public List<GameObject> NPC()
        {
            List<GameObject> tempObjs = new List<GameObject>();
            foreach (GameObject temp in AllObjs())
            {
                if (temp.layer == LayerMask.NameToLayer("NPC"))
                    tempObjs.Add(temp);
            }
            return tempObjs;
        }

        public List<GameObject> HiddenPos()
        {
            List<GameObject> tempObjs = new List<GameObject>();
            foreach (GameObject temp in AllObjs())
            {
                if (temp.layer == LayerMask.NameToLayer("HiddenPos"))
                    tempObjs.Add(temp);
            }
            return tempObjs;
        }

        public List<GameObject> Tiles()
        {
            List<GameObject> tempObjs = new List<GameObject>();
            foreach (GameObject temp in AllObjs())
            {
                if (temp.layer == LayerMask.NameToLayer("Tile"))
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

        #region DialoguePlaying
        public void DialoguePaused()
        {
            if (DiaPlay.n_state == NodeState.Dialogue && DiaPlay.d_state != DiaState.OFF)
                StartCoroutine(WaitAndPlay());
            else if (DiaPlay.n_state == NodeState.Option)
            {

            }
        }

        IEnumerator WaitAndPlay()
        {
            yield return new WaitForSeconds(0.7f);
            HistoryText += DiaPlay.WholeText + System.Environment.NewLine;
            //EventCenter.GetInstance().EventTriggered("DialoguePlay.Next", 0);
            DiaPlay.Next(0);
        }

        public void OptionSelect(int index)
        {
            DiaPlay.Next(index);
        }

        public void PlayingDialogue(DialogueGraph graph)
        {
            if (DiaPlay.d_state == DiaState.OFF)
                EventCenter.GetInstance().EventTriggered("DialoguePlay.Start", graph);
        }

        public void NPCArrive(string NPCName)
        {
            if (NPCAgentList.ContainsKey(NPCName))
            {
                NPCAgentList[NPCName] = true;
            }
        }

        public void DialogueOptionShowUp(List<OptionClass> opts)
        {
            OptionList.Clear();
            OptionList = opts;
        }

        public void DialogueOFF()
        {
            //HistoryText += DiaPlay.WholeText + System.Environment.NewLine;
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

