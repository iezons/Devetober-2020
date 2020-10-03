using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        MoveTo
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
        public List<GameObject> NPCTalking= new List<GameObject>();
        public List<MoveToClass> NPCWayPoint = new List<MoveToClass>();

        public DoingWithEnemy doingWithEnemy = DoingWithEnemy.Spawn;
        public List<Transform> SpawnPoint = new List<Transform>();
        public List<MoveToClass> EnemyWayPoint = new List<MoveToClass>();

        public DoingWithRoom doingWithRoom = DoingWithRoom.None;
    }

    [Serializable]
    public class MoveToClass
    {
        public Transform Object;
        public Transform MoveTO;
    }

    public class EventScriptInterface : MonoBehaviour{ }
}