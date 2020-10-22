using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCTRL : MonoBehaviour
{
    public Animator Locker;
    public List<Animator> NPC;
    public Animator Enemy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Hug");
            NPC[0].Play("Got Bite", 0);
            Enemy.Play("Zombie_Hug", 0);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Attack");
            NPC[0].Play("Got Hurt", 0);
            Enemy.Play("Zombie_Attack1", 0);
        }
    }

    public void PlayAnim(string AnimName)
    {
        for (int i = 0; i < NPC.Count; i++)
        {
            NPC[i].Play(AnimName, 0);
        }
    }

    public void ZombieAnim(string AnimName)
    {
        Enemy.Play(AnimName, 0);
    }
}
