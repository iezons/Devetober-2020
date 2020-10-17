using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOptionSelecting : MonoBehaviour
{
    public void ButtonClicking()
    {
        GameManager.GetInstance().OptionSelect(int.Parse(transform.name));
    }
}
