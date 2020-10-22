using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackCollider : MonoBehaviour
{
    public BoxCollider coll;
    void Awake()
    {
        coll.enabled = false;
    }

    void OnEnable()
    {
        coll.enabled = false;
    }

    public void ON()
    {
        coll.enabled = true;
    }

    public void OFF()
    {
        coll.enabled = false;
    }
}
