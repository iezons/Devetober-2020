using GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionSO
{
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

    public enum NPCHPWith
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
    public NPCHPWith nPCHPWith = NPCHPWith.Equal;
    public float HP = 0f;

    //----------------------Enemy---------------------------
    public EnemyConditionWith enemyConditionWith;
    //--Overall_Numbers
    public int OverallNumbers = 0;

    //----------------------Room---------------------------
    public RoomConditionWith roomConditionWith = RoomConditionWith.Number_of_NPCs;
    public List<RoomTracker> roomTrackers = new List<RoomTracker>();
    //---NPCNumber
    public int NPC_Number = 0;
    //---Specific_NPCs
    public List<NpcController> npcControllers = new List<NpcController>();
    //--Number_of_Enemys
    public int Enemy_Number = 0;

    //----------------------Event---------------------------
    public List<EventTrigger> eventTriggers = new List<EventTrigger>();

    //----------------------Custom---------------------------
    public List<CustomCondition> customConditions = new List<CustomCondition>();
}

public class EventTrigger
{
    public bool IsTriggered;
    public string EventName;
}

public class CustomCondition
{
    public bool IsWaiting;
    public virtual bool Conditional()
    {
        return false;
    }
}