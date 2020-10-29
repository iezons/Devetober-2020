using DiaGraph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

namespace GamePlay
{
    [RequireComponent(typeof(DialoguePlay))]
    [RequireComponent(typeof(NavMeshSurface))]
    public class RoomTracker : ControllerBased
    {

        #region Inspector View

        [System.Serializable]
        public class ScaleAndOffset
        {
            [Range(0f, 100f)]
            public float length = 0;
            [Range(0f, 100f)]
            public float height = 0;
            [Range(0f, 100f)]
            public float width = 0;
            [Range(-100f, 100f)]
            public float x = 0;
            [Range(-100f, 100f)]
            public float y = 0;
            [Range(-100f, 100f)]
            public float z = 0;
        }

        [System.Serializable]
        public class CameraList
        {
            public Camera roomCamera = null;
            public bool isLocked = false;
            public float speed = 0;
            public float angle = 0;
            public float recordedAngle = 0;
            public float plusAngle = 0;
        }

        [Header("Camera")]
        public List<CameraList> cameraLists = new List<CameraList>();
        public int CurrentCameraIndex = 0;

        [Header("RoomObject")]
        public Interact_SO CBoard;
        public List<DoorController> Door;
        [HideInInspector]
        public bool CanBeDetected = true;
        [Header("Dialogue")]
        public DialoguePlay DiaPlay;
        public DialogueGraph WaitingGraph;
        [TextArea(5,5)]
        public string HistoryText;
        public int WordCound;
        public Dictionary<string, bool> NPCAgentList = new Dictionary<string, bool>();
        public List<OptionClass> OptionList = new List<OptionClass>();
        [Header("NavMesh")]
        public List<NavMeshSurface> navSurface = new List<NavMeshSurface>();


        [Header("Room Setting")]
        [SerializeField]
        List<ScaleAndOffset> scaleAndOffset = new List<ScaleAndOffset>();

        [SerializeField]
        LayerMask canDetected = 0;
        [SerializeField]
        LayerMask reportEmergency = 0;

        public bool isScanOn;
        public int roomCapacity;

        [Header("Room Object")]
        public List<GameObject> RoomObject;
        #endregion


        #region Value
        bool tempCheck;
        public List<Collider> hitInformation = new List<Collider>();
        List<RoomTracker> roomScripts = new List<RoomTracker>();
        [HideInInspector]
        public List<GameObject> NPCs = new List<GameObject>();
        public List<Transform> tempWayPoints = new List<Transform>();
        public List<GameObject> npcs = new List<GameObject>();
        public List<Interact_SO> beds = new List<Interact_SO>();
        #endregion

        public void Awake()
        {
            outline = GetComponent<Outline>();
            Detecting();
            DiaPlay = GetComponent<DialoguePlay>();
            navSurface = GetComponents<NavMeshSurface>().ToList();
            if (isScanOn)
            {
                AddWayPoints();
            }

            for (int i = 0; i < cameraLists.Count; i++)
            {
                cameraLists[i].recordedAngle = cameraLists[i].roomCamera.transform.rotation.eulerAngles.y;
            }
        }

        public void Start()
        {
            EventCenter.GetInstance().EventTriggered("GM.Room.Add", this);
            Invoke("GenerateList", 0.00001f);

            foreach (var item in RestingPos())
            {
                Interact_SO interact_SO = item.GetComponent<Interact_SO>();
                switch (interact_SO.type)
                {
                    case Interact_SO.InteractType.Bed:
                        beds.Add(interact_SO);
                        break;
                    default:
                        break;
                }
            }
        }

        void GenerateList()
        {
            foreach (RoomTracker temp in GameManager.GetInstance().Rooms)
            {
                roomScripts.Add(temp);
            }

            for (int i = 0; i < roomScripts.Count; i++)
            {
                NPCs.AddRange(roomScripts[i].NPC());
            }
        }

        private void Update()
        {
            Detecting();
            DialogueChecking();
            RescuingCode();
            RoomCapacityCheck();
        }

        private void Detecting()
        {
            hitInformation.Clear();
            //Track Objects in the area
            for (int i = 0; i < scaleAndOffset.Count; i++)
            {
                hitInformation.AddRange(Physics.OverlapBox(transform.position + new Vector3(scaleAndOffset[i].x, scaleAndOffset[i].y, scaleAndOffset[i].z), new Vector3(scaleAndOffset[i].length, scaleAndOffset[i].height, scaleAndOffset[i].width) / 2, Quaternion.identity, canDetected));
            }
        }

        void RescuingCode()
        {
            if(NPCs.Count != 0)
            {
                for (int i = 0; i < NPCs.Count; i++)
                {
                    NpcController npc = NPCs[i].GetComponent<NpcController>();                   
                    if (npc.status.isStruggling)
                    {
                        foreach (var item in NPCs)
                        {
                            NpcController npcCtrl = item.GetComponent<NpcController>();
                            if (npcCtrl.status.isStruggling)
                                continue;
                            if (npcCtrl.MenuContains("Rescue") >= 0)
                                continue;
                            if (item.layer == LayerMask.NameToLayer("Dead"))
                                continue;
                            npcCtrl.InsertMenu(rightClickMenus.Count, "Rescue", "Rescue", true, npcCtrl.TriggerRescuing, 1 << LayerMask.NameToLayer("NPC"));
                        }
                        break;
                    }
                    else
                    {
                        npc.RemoveMenu("Rescue");
                    }
                }              
            }
        }

        void RoomCapacityCheck()
        {
            if(NPC().Count > roomCapacity && !isEnemyDetected())
            {
                npcs.AddRange(NPC());
                npcs = NPC().OrderBy(npc => npc.GetComponent<NpcController>().status.currentStamina).ToList();

                for (int i = 0; i < beds.Count; i++)
                {
                    NpcController npc = npcs[i].GetComponent<NpcController>();
                    if (npc.m_fsm.GetCurrentState() == "Patrol" && !npc.isRoomCalled)
                    {
                        npcs[i].GetComponent<NpcController>().TriggerBedResting(beds[i].gameObject);
                    }
                }

                if (npcs.Count > beds.Count)
                {
                    for (int i = 0; i < npcs.Count - beds.Count; i++)
                    {
                        NpcController npc = npcs[i].GetComponent<NpcController>();
                        if (npc.m_fsm.GetCurrentState() == "Patrol" && !npc.isRoomCalled)
                        {
                            npcs[i].GetComponent<NpcController>().TriggerRandomResting();
                        }
                    }
                }
            }
        }

        #region Camera Movement
        public void Rotate(float direction, int index)
        {
            CameraList currentCam = cameraLists[index];
            float rotationSpeed = direction * cameraLists[index].speed * Time.deltaTime;
            if(currentCam.plusAngle + rotationSpeed > currentCam.angle / 2)
            {
                currentCam.plusAngle = currentCam.angle / 2;
            }
            else if(currentCam.plusAngle + rotationSpeed < -currentCam.angle / 2)
            {
                currentCam.plusAngle = -currentCam.angle / 2;
            }
            else
            {
                currentCam.plusAngle += rotationSpeed;
            }
            currentCam.roomCamera.transform.eulerAngles = new Vector3(currentCam.roomCamera.transform.eulerAngles.x, currentCam.recordedAngle + currentCam.plusAngle, currentCam.roomCamera.transform.eulerAngles.z);
        }


        #endregion

        #region Information Pool
        public List<GameObject> AllObjs()
        {
            List<GameObject> tempobjs = new List<GameObject>();
            foreach (Collider temp in hitInformation)
            {
                if(!tempobjs.Contains(temp.gameObject))
                    tempobjs.Add(temp.gameObject);
            }

            foreach (var obj in tempobjs)
            {
                if (!hitInformation.Contains(obj.GetComponent<Collider>()))
                {
                    tempobjs.Remove(obj);
                }
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

        public List<GameObject> RestingPos()
        {
            List<GameObject> tempObjs = new List<GameObject>();
            foreach (GameObject temp in AllObjs())
            {
                if (temp.layer == LayerMask.NameToLayer("RestingPos"))
                    tempObjs.Add(temp);
            }
            return tempObjs;
        }

        public void AddWayPoints()
        {
            foreach (var temp in AllObjs())
            {
                if (temp.layer == LayerMask.NameToLayer("WayPoints"))
                    tempWayPoints.Add(temp.transform);
            }
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
            for (int i = 0; i < hitInformation.Count; i++)
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

        void DialogueChecking()
        {
            if (WaitingGraph != null)
            {
                bool tempBool = false;
                foreach (bool value in NPCAgentList.Values)
                {
                    if (value == false)
                    {
                        tempBool = false;
                        break;
                    }
                    else
                    {
                        tempBool = true;
                    }
                }
                if (tempBool)
                {
                    PlayingDialogue(WaitingGraph);
                    WaitingGraph = null;
                    NPCAgentList.Clear();
                }
            }
        }

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
            HistoryText += DiaPlay.WholeText + "\n";
            WordCound += DiaPlay.WordCount + 1;
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
                DiaPlay.PlayDia(graph);
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
            OptionList = opts;
            if(GameManager.GetInstance().CurrentRoom == this)
            {
                GameManager.GetInstance().SetupOption(this);
            }
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
            for (int i = 0; i < scaleAndOffset.Count; i++)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(scaleAndOffset[i].x, scaleAndOffset[i].y, scaleAndOffset[i].z), new Vector3(scaleAndOffset[i].length, scaleAndOffset[i].height, scaleAndOffset[i].width));
            }
        }
        #endregion
    }
}

