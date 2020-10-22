using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocatorList
{
    //public bool isTaken;
    public NpcController npc;
    public Transform Locator;
}

public class Interact_SO : ControllerBased
{
    public enum InteractType
    {
        Locker,
        Box,
        Bed,
        Chair,
        Terminal,
        Switch,
        Storage
    }

    public InteractType type;

    [Header("State")]

    [Header("Animation")]
    public Animator Anim;

    [Header("Locator")]
    public List<LocatorList> Locators = new List<LocatorList>();

    [Header("Health")]
    public int maxHealth = 0;
    public int currentHealth = 0;

    public void ApplyHealth(int healthAmount)
    {
        currentHealth = currentHealth + healthAmount > maxHealth ? maxHealth : currentHealth += healthAmount;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            //Death();
        }
    }

    public void PlayAnimation(string AnimName, int Layer = 0)
    {
        Anim.Play(AnimName, Layer);
    }

    //public virtual void CallNPC(object obj)
    //{
    //    Debug.Log("Call NPC");
    //    GameObject gameObj = (GameObject)obj;
    //    NpcController npc = gameObj.GetComponent<NpcController>();
    //    npc.ReceiveItemCall(gameObj);
    //}

    public void CallNPC(object obj)
    {
        GameObject gameObj = (GameObject)obj;
        NpcController npc = gameObj.GetComponent<NpcController>();
        npc.ReceiveInteractCall(gameObject);
    }

    public virtual void NPCInteract(int InteractWay = 0)
    {
        switch (type)
        {
            case InteractType.Locker:
                PlayAnimation(InteractWay.ToString());
                RemoveAndInsertMenu("Hide In", "Leave", "Leave", false, NPCInteractFinish);
                break;
            case InteractType.Box:
                break;
            case InteractType.Bed:
                RemoveAndInsertMenu("RestIn", "Leave", "Leave", false, NPCInteractFinish);
                break;
            case InteractType.Chair:
                RemoveAndInsertMenu("RestIn", "Leave", "Leave", false, NPCInteractFinish);
                break;
            case InteractType.Terminal:
                //PlayAnimation(InteractWay.ToString());
                RemoveAndInsertMenu("Operate", "Leave", "Leave", false, NPCInteractFinish);
                break;
            case InteractType.Switch:
                break;
            default:
                break;
        }
    }

    public virtual void NPCInteractFinish(object obj)
    {
        for (int i = 0; i < Locators.Count; i++)
        {
            Locators[i].npc.PlayGetOutAnim(gameObject);
            Locators[i].npc = null;
        }
        switch (type)
        {
            case InteractType.Locker:
                PlayAnimation("1");
                break;
            case InteractType.Box:
                break;
            case InteractType.Bed:
                break;
            case InteractType.Chair:
                break;
            case InteractType.Terminal:
                break;
            case InteractType.Switch:
                break;
            default:
                break;
        }
    }
}
