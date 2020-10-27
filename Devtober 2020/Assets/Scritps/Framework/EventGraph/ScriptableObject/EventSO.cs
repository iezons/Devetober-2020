using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiaGraph;
using UnityEngine.Timeline;
using GamePlay;

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
        Patrol
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

        public DoingWithNPC doingWithNPC = DoingWithNPC.Talking;
        public List<TalkingClass> NPCTalking= new List<TalkingClass>();
        public List<MoveToClass> NPCWayPoint = new List<MoveToClass>();

        public DoingWithEnemy doingWithEnemy = DoingWithEnemy.Spawn;
        public List<Transform> SpawnPoint = new List<Transform>();
        public List<MoveToClass> EnemyWayPoint = new List<MoveToClass>();

        public DialogueGraph Dialogue_Graph;
        public RoomTracker Dialogue_Room;

        public DoingWithRoom doingWithRoom = DoingWithRoom.None;

        public List<EventScriptInterface> CustomCode = new List<EventScriptInterface>();

        public List<TimelineAsset> timelines = new List<TimelineAsset>();

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
        public GameObject NPC; //指的是谁要去移动
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