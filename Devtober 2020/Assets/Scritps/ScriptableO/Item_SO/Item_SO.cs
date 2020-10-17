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

public class Item_SO : ControllerBased
{
    public enum ItemType
    {
        Locker,
        Box,
        Bed,
        Chair,
        Terminal
    }

    public ItemType type;

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
        Debug.Log("Call NPC");
        GameObject gameObj = (GameObject)obj;
        NpcController npc = gameObj.GetComponent<NpcController>();
        npc.ReceiveItemCall(gameObject);
    }

    public void NPCInteract(int InteractWay = 0)
    {
        switch (type)
        {
            case ItemType.Locker:
                PlayAnimation(InteractWay.ToString());
                RemoveAndInsertMenu("Hide In", "Leave", "Leave", false, NPCInteractFinish, 1 << LayerMask.NameToLayer("HiddenPos"));
                break;
            case ItemType.Box:
                break;
            case ItemType.Bed:
                RemoveAndInsertMenu("RestIn", "Leave", "Leave", false, NPCInteractFinish, 1 << LayerMask.NameToLayer("HiddenPos"));
                break;
            case ItemType.Chair:
                RemoveAndInsertMenu("RestIn", "Leave", "Leave", false, NPCInteractFinish, 1 << LayerMask.NameToLayer("HiddenPos"));
                break;
            case ItemType.Terminal:
                //PlayAnimation(InteractWay.ToString());
                RemoveAndInsertMenu("Operate", "Leave", "Leave", false, NPCInteractFinish, 1 << LayerMask.NameToLayer("HiddenPos"));
                break;
            default:
                break;
        }
    }

    public void NPCInteractFinish(object obj)
    {
        for (int i = 0; i < Locators.Count; i++)
        {
            Locators[i].npc.PlayGetOutAnim(gameObject);
        }
        switch (type)
        {
            case ItemType.Locker:
                PlayAnimation("1");
                break;
            case ItemType.Box:
                break;
            case ItemType.Bed:
                break;
            case ItemType.Chair:
                break;
            case ItemType.Terminal:
                break;
            default:
                break;
        }
    }
}
