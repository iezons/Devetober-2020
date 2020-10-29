using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackCollider : MonoBehaviour
{
    public float damage;
    public Transform hand;
    public Vector3 size;
    bool IsON;
    public bool isHit = false;
    void Update()
    {
        if (IsON)
        {
            Collider[] coll = Physics.OverlapBox(hand.position, size / 2, hand.rotation);
            foreach (var collider in coll)
            {
                collider.TryGetComponent(out NpcController npcController);
                if (npcController != null)
                {
                    if (!isHit)
                    {
                        npcController.TakeDamage(damage);
                        npcController.m_fsm.ChangeState("GotAttacked");
                        npcController.GotBitted();
                        isHit = !isHit;
                    }  
                }
                else
                {
                    collider.TryGetComponent(out Interact_SO interact);
                    if (interact != null)
                    {
                        if (!isHit)
                        {
                            interact.TakeDamage(damage);
                            isHit = !isHit;
                        }      
                    }
                }
            }
        }
    }

    public void ON()
    {
        IsON = true;
    }

    public void OFF()
    {
        IsON = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(hand != null)
        {
            Gizmos.DrawWireCube(hand.position, size);
        }
    }
}
