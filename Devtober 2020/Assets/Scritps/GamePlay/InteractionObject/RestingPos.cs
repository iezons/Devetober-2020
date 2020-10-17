using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestingPos : ControllerBased
{
    public List<Transform> restLocators = new List<Transform>();
    public bool isTaken;
    public bool isBed;
    public bool isCafeteriaTable;

    void Awake()
    {
    }

    void Start()
    {
        AddMenu("Rest", "Rest", true, CallNPC);
    }

    public void CallNPC(object obj)
    {

    }
}
