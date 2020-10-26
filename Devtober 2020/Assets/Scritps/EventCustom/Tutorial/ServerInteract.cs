using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInteract : Interact_SO
{
    private void Awake()
    {
        type = InteractType.TU_Server;
        outline = GetComponent<Outline>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
