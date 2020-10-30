using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainLevelEventHolder : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        
    }

    void a(string name, UnityAction action)
    {
        EventCenter.GetInstance().AddEventListener(name, action);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
