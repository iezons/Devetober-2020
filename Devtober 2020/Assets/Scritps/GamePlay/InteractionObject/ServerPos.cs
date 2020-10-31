﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPos : Interact_SO
{
    public bool isUnlocked = false;
    public List<GameObject> RedLight;
    public List<GameObject> GreenLight;
    private void Awake()
    {
        outline = GetComponent<Outline>();
        AddMenu("Operate", "Operate", true, CallNPC, 1 << LayerMask.NameToLayer("NPC"));
    }

    private void Update()
    {
        if (RedLight != null && GreenLight != null)
        {
            if (isUnlocked)
            {
                foreach (var item in RedLight)
                {
                    item.SetActive(false);
                }
                foreach (var item in GreenLight)
                {
                    item.SetActive(true);
                }
                IsInteracting = true;
            }
            else
            {
                foreach (var item in RedLight)
                {
                    item.SetActive(true);
                }
                foreach (var item in GreenLight)
                {
                    item.SetActive(false);
                }

            }
        }
    }
}