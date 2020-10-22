using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SwitchLight : MonoBehaviour
{
    // Start is called before the first frame update
    public bool IsClosed;
    public DoorController door;
    public Light currentLight;
    public Color green = Color.green;
    public Color red = Color.red;
    Renderer renderer;
    [SerializeField]
    Material LightMaterial = null;
    [SerializeField]
    Material BodyMaterial = null;
    Material Light;
    Material Body;
    // Start is called before the first frame update
    void Awake()
    {
        renderer = GetComponent<Renderer>();
        Light = Instantiate(LightMaterial);
        Body = Instantiate(BodyMaterial);
        List<Material> mat = new List<Material>();
        mat.Add(Light);
        mat.Add(Body);
        renderer.sharedMaterials = mat.ToArray();
        currentLight = transform.GetChild(0).GetComponent<Light>();
    }

// Update is called once per frame
    void Update()
    {
        LightCheck(door.isLocked);
    }

    void LightCheck(bool isClosed)
    {
        //currentLight.
        if (isClosed) 
        {
            currentLight.color = red;
            Light.SetColor("_EmissionColor", red);
        }
        else
        {
            currentLight.color = green;
            Light.SetColor("_EmissionColor", green);
        }
    }
}
