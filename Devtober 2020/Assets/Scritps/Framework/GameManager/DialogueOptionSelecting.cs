using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOptionSelecting : MonoBehaviour
{
    public void ButtonClicking()
    {
        EventCenter.GetInstance().EventTriggered("DialoguePlay.Next", int.Parse(transform.name));
    }
}
