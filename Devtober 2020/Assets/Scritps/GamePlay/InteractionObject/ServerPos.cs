using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPos : Interact_SO
{
    public bool isUnlocked = false;
    public GameObject light;
    private void Awake()
    {
        outline = GetComponent<Outline>();
        AddMenu("Operate", "Operate", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

    private void Update()
    {
        if (light != null)
        {
            if (isUnlocked)
            {
                light.SetActive(false);
                IsInteracting = true;
            }
            else
                light.SetActive(true);
        }
    }
}
