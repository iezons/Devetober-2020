using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCTRL : MonoBehaviour
{
    public Animator Locker;
    public Animator NPC;

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
            Locker.Play("0", 0);
            NPC.Play("GetInLocker", 0);
        }
    }
}
