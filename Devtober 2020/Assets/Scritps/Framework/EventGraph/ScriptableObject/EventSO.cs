using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiaGraph;
using UnityEngine.Timeline;
using GamePlay;
using UnityEngine.Playables;

namespace EvtGraph
{
    public enum DoingWith
    {
        NPC,
        Room,
        Enemy,
        Dialogue,
        Timeline,
        Custom
    }

    public enum DoingWithNPC
    {
        Talking,
        MoveTo,
        Patrol,
        Interact,
        AnimState
    }

    public enum DoingWithNPC_Interact
    {
        InteractObject,
        InteractItem
    }

    public enum DoingWithEnemy
    {
        Spawn,
        MoveTo
    }

    public enum DoingWithRoom
    {
        None,
    }

    [Serializable]
    public class EventSO
    {
        public string ID = Guid.NewGuid().ToString();
        public List<EventScriptInterface> TODOList = new List<EventScriptInterface>();

        public DoingWith doingWith = DoingWith.NPC;

        //NPC
        public DoingWithNPC doingWithNPC = DoingWithNPC.Talking;
        //--Talking
        public List<TalkingClass> NPCTalking= new List<TalkingClass>();
        //--WayPoint
        public List<MoveToClass> NPCWayPoint = new List<MoveToClass>();
        //--Interact
        public NpcController NPCInteract = null;
        public DoingWithNPC_Interact doingWithNPC_Interact;
        public Interact_SO InteractObject = null;
        public Item_SO Item = null;
        //--AnimState
        public List<NpcController> NPC = new List<NpcController>();
        public bool IsAnimState = true;
        public string AnimStateName = string.Empty;

        //Enemy
        public DoingWithEnemy doingWithEnemy = DoingWithEnemy.Spawn;
        public List<Transform> SpawnPoint = new List<Transform>();
        public List<MoveToClass> EnemyWayPoint = new List<MoveToClass>();

        //Dialogue
        public DialogueGraph Dialogue_Graph;
        public RoomTracker Dialogue_Room;

        public DoingWithRoom doingWithRoom = DoingWithRoom.None;

        //Custom
        public List<EventScriptInterface> CustomCode = new List<EventScriptInterface>();
        
        //Timelines
        public List<PlayableDirector> timelines = new List<PlayableDirector>();

        public EventSO()
        {
            ID = Guid.NewGuid().ToString();
            TODOList = new List<EventScriptInterface>();

            doingWith = DoingWith.NPC;

            doingWithNPC = DoingWithNPC.Talking;
            NPCTalking = new List<TalkingClass>();
            NPCWayPoint = new List<MoveToClass>();

            doingWithEnemy = DoingWithEnemy.Spawn;
            SpawnPoint = new List<Transform>();
            EnemyWayPoint = new List<MoveToClass>();

            doingWithRoom = DoingWithRoom.None;

            CustomCode = new List<EventScriptInterface>();
        }
    }

    [Serializable]
    public class TalkingClass
    {
        public List<MoveToClass> moveToClasses;
        public RoomTracker room;
        public DialogueGraph Graph;
    }

    [Serializable]
    public class MoveToClass
    {
        public GameObject Obj; //指的是谁要去移动
        public Transform MoveTO; //指的是要移动到哪里
    }

    public class EventScriptInterface : MonoBehaviour
    {
        private bool isEventFinish = false;
        public bool IsEventFinish { get => isEventFinish;}

        public bool Instan = false;

        public virtual void OnEnable()
        {
            Instan = true;
        }

        public virtual void DoEvent(object obj)
        {
            
        }

        public void FinishEvent()
        {
            isEventFinish = true;
        }
    }
}