using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLight : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject pointLight;
    public GameObject door;
    public Light currentLight;
    public Vector4 green;
    public Vector4 red;
    // Start is called before the first frame update
    void Start()
    {
        pointLight = transform.GetChild(0).gameObject;
        currentLight = pointLight.GetComponent<Light>();
        red = new Vector4(255f, 0f, 0f, 255f);
        green = new Vector4(20f, 225f, 20f, 255f);
    }

// Update is called once per frame
void Update()
    {
        LightCheck(door.GetComponent<DoorController>().isClosed);
    }

    void LightCheck(bool isClosed)
    {
        if (isClosed) { currentLight.color = red; }
        else
        { currentLight.color = green; }

    }
}
