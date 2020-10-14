using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DiaGraph;

public class Testing : MonoBehaviour
{
    public DialogueGraph Graph;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            EventCenter.GetInstance().EventTriggered("GM.DialoguePlay.Start", Graph);
        }
        //TODO
        //Event SO
        //Event Graph
        //GM 空壳
    }
}
