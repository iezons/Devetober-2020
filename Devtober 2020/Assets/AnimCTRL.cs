using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCTRL : MonoBehaviour
{
    public Animator Door;
    public Animator NPC;
    public Rigidbody rig;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Get In Locker");
            Door.SetTrigger("Open");
            //float playtime = NPC.GetCurrentAnimatorStateInfo(0).normalizedTime;
            NPC.Play("GetInLock", 0, 0f);
        }
    }
}
