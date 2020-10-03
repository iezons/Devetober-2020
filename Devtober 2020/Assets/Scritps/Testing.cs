using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DiaGraph;

public class Testing : MonoBehaviour
{
    public TMP_Text text;
    public DialoguePlay DiaPlay;
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
            //DiaPlay.PlayDia(Graph);
            EventCenter.GetInstance().EventTriggered("DialoguePlay.Start", Graph);
        }
        text.text = DiaPlay.WholeText;
        text.maxVisibleCharacters = DiaPlay.MaxVisible;
        //TODO
        //Event SO
        //Event Graph
        //GM 空壳
    }
}
