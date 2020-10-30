using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible : MonoBehaviour
{
    public bool IsVisible;

    private void Update()
    {
        if(IsVisible)
        {
            //Debug.Log("Visible");
        }
    }

    private void OnBecameVisible()
    {
        IsVisible = true;
    }

    private void OnBecameInvisible()
    {
        IsVisible = false;
    }
}
