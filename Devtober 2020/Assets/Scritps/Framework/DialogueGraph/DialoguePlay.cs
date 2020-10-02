using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePlay : MonoBehaviour
{

    // Start is called before the first frame update
    void Awake()
    {
        EventCenter.GetInstance().AddEventListener<int>("Dialogue.Pause", PlayDia);
        EventCenter.GetInstance().EventTriggered("Dialogue.Play", 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayDia(int fffff)
    {

    }

    void asfsaf(int fffff)
    {

    }
}
