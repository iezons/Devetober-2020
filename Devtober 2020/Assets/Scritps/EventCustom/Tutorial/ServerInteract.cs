using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInteract : Interact_SO
{
    public bool Online = false;
    public bool Flashing = false;
    public float FlashingSpeed = 2f;

    bool IsIncreaseing = false;

    private void Awake()
    {
        type = InteractType.TU_Server;
        outline = GetComponent<Outline>();
        IsInteracting = true;
        AddMenu("SendToNextLevel", "Interact", false, SendToNextLevel);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Flashing)
        {
            AlwaysOutline = true;
            SetOutline(true);
            if(outline.OutlineWidth - Time.deltaTime <= 0f)
                IsIncreaseing = true;
            else if (outline.OutlineWidth + Time.deltaTime >= OutlineWidth)
                IsIncreaseing = false;

            if(IsIncreaseing)
            {
                outline.OutlineWidth += Time.deltaTime * FlashingSpeed;
            }
            else
            {
                outline.OutlineWidth -= Time.deltaTime * FlashingSpeed;
            }
        }
    }

    void SendToNextLevel(object obj)
    {

    }

    public void SetInteract(bool Set)
    {
        IsInteracting = Set;
    }

    public void SetFlashing(bool Set)
    {
        Flashing = Set;
    }
}
