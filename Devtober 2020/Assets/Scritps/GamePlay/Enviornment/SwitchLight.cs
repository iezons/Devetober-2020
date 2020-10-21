using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SwitchLight : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject door;
    public Light currentLight;
    public Color green = new Color(0, 225, 0, 255);
    public Color red = new Color(255, 0, 0, 255);
    // Start is called before the first frame update
    void Awake()
    {
        currentLight = transform.GetChild(0).GetComponent<Light>();
    }

// Update is called once per frame
void Update()
    {
        LightCheck(door.GetComponent<DoorController>().isClosed);
    }

    void LightCheck(bool isClosed)
    {
        //currentLight.
        if (isClosed) { currentLight.color = red; }
        else
        { currentLight.color = green; }
    }
}
