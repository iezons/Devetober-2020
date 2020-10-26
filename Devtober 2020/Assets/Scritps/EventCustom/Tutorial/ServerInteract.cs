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
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        IsInteracting = !Online;

        Color temp = outline.OutlineColor;
        if (Flashing)
        {
            outline.OutlineWidth = OutlineWidth;
            if(outline.OutlineColor.a <= 0.001f)
                IsIncreaseing = true;
            else if (outline.OutlineColor.a >= 0.999f)
                IsIncreaseing = false;

            if(IsIncreaseing)
            {
                temp = new Color(temp.r, temp.g, temp.b, temp.a + Time.deltaTime * FlashingSpeed);
            }
            else
            {
                temp = new Color(temp.r, temp.g, temp.b, temp.a - Time.deltaTime * FlashingSpeed);
            }
        }
    }

    public void SetOnline(bool Set)
    {
        Online = Set;
    }

    public void SetFlashing(bool Set)
    {
        Flashing = Set;
    }
}
