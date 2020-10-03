using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiaGraph;

namespace EvtGraph
{
    public enum DoingWith
    {
        NPC,
        Room,
        Enemy
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

    [CreateAssetMenu(menuName = "Graph/Event SO")]
    public class EventSO : ScriptableObject
    {
        public string ID = System.Guid.NewGuid().ToString();
        public List<EventScriptInterface> TODOList = new List<EventScriptInterface>();

        public DoingWith doingWith = DoingWith.NPC;

        public DoingWithNPC doingWithNPC = DoingWithNPC.Talking;
        public List<TalkingClass> NPCTalking= new List<TalkingClass>();
        public List<MoveToClass> NPCWayPoint = new List<MoveToClass>();

        public DoingWithEnemy doingWithEnemy = DoingWithEnemy.Spawn;
        public List<Transform> SpawnPoint = new List<Transform>();
        public List<MoveToClass> EnemyWayPoint = new List<MoveToClass>();

        public DoingWithRoom doingWithRoom = DoingWithRoom.None;

        //public void TalkingTO(TalkingClass talking)
        //{
        //    NPCMoveTO(talking.MoveToClassA);
        //    NPCMoveTO(talking.MoveToClassB);
        //}

        //public void NPCMoveTO(MoveToClass move)
        //{
        //    NpcController NPCCtrl = move.Object.GetComponent<NpcController>();
        //    NPCCtrl.readyForDispatch();
        //    NPCCtrl.Dispatch(move.MoveTO.position);
        //}
    }

    [Serializable]
    public class TalkingClass
    {
        public MoveToClass MoveToClassA;
        public MoveToClass MoveToClassB;
        public DialogueGraph Graph;
    }

    [Serializable]
    public class MoveToClass
    {
        public Transform Object;
        public Transform MoveTO;
    }

    public class EventScriptInterface : MonoBehaviour{ }
}