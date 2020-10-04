using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs
{
    public static bool leftMousePressed()
    {
        return Input.GetMouseButtonDown(0);
    }

    public static bool rightMousePressed()
    {
        return Input.GetMouseButtonDown(1);
    }
}
