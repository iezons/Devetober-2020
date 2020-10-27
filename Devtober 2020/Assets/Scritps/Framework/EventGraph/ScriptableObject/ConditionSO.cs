using GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvtGraph
{
    [Serializable]
    public class ConditionSO
    {
        public string ID = Guid.NewGuid().ToString();
        public enum ConditionWith
        {
            NPC,
            Room,
            Enemy,
            Event,
            Custom
        }

        public enum NPCConditionWith
        {
            HP,
        }

        public enum EqualType
        {
            Greater,
            Less,
            Equal,
            GEqual,
            LEqual
        }

        public enum EnemyConditionWith
        {
            Overall_Numbers,
        }

        public enum RoomConditionWith
        {
            Number_of_NPCs,
            Specific_NPCs,
            Number_of_Enemys
        }

        public ConditionWith conditionWith;

        //----------------------NPC---------------------------
        public NPCConditionWith nPCConditinoWith = NPCConditionWith.HP;
        public List<NpcController> NPC = new List<NpcController>();
        //--HP
        public EqualType nPCHPWith = EqualType.Equal;
        public float HP = 0f;

        //----------------------Enemy---------------------------
        public EnemyConditionWith enemyConditionWith;
        //--Overall_Numbers
        public EqualType Enemy_Num = EqualType.Equal;
        public int OverallNumbers = 0;

        //----------------------Room---------------------------
        public RoomConditionWith roomConditionWith = RoomConditionWith.Number_of_NPCs;
        public List<RoomTracker> roomTrackers = new List<RoomTracker>();
        //---NPCNumber
        public EqualType Room_NPC_Num = EqualType.Equal;
        public int NPC_Number = 0;
        //---Specific_NPCs
        public List<SpecificRoomNPC> SpecificRoomNPCs = new List<SpecificRoomNPC>();
        //--Number_of_Enemys
        public EqualType Room_Enemy_Num = EqualType.Equal;
        public int Enemy_Number = 0;

        //----------------------Event---------------------------
        public List<EventTrigger> eventTriggers = new List<EventTrigger>();

        //----------------------Custom---------------------------
        public List<CustomCondition> customConditions = new List<CustomCondition>();
    }

    [Serializable]
    public class EventTrigger
    {
        public bool IsTriggered;
        public string EventName;
    }

    [Serializable]
    public class SpecificRoomNPC
    {
        public RoomTracker roomTracker;
        public List<NpcController> npcControllers;
    }

    public class CustomCondition : MonoBehaviour
    {
        public bool Instan = false;

        public virtual void OnEnable()
        {
            Instan = true;
        }

        public virtual bool Conditional()
        {
            return false;
        }
    }
}