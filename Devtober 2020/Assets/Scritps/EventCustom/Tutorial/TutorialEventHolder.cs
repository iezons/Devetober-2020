using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventHolder : MonoBehaviour
{
    private void Awake()
    {
        //EventCenter.GetInstance().AddEventListener();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EventTrigger(string EventName)
    {
        EventCenter.GetInstance().EventTriggered(EventName, EventName);
    }
}
