using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackCollider : MonoBehaviour
{
    public float Damage;
    public Transform Hand;
    public Vector3 Size;
    bool IsON;
    void Update()
    {
        if(IsON)
        {
            Collider[] coll = Physics.OverlapBox(Hand.position, Size / 2, Hand.rotation);
            foreach (var collider in coll)
            {
                collider.TryGetComponent(out NpcController npcController);
                if(npcController != null)
                {

                }
                    //npcController.GetHurt(Damage);
                else
                {
                    collider.TryGetComponent(out Interact_SO interact);
                    if(interact != null)
                    {
                        //interact.TakeDamage();
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
        Gizmos.DrawWireCube(Hand.position, Size);
    }
}
