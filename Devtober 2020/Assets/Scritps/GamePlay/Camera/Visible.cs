using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible : MonoBehaviour
{
    public Transform RoomCamera;

    private void Update()
    {
        if(Time.frameCount % 2 == 0)
        {
            if (RoomCamera.eulerAngles.y > 105.63f)
            {
                EventCenter.GetInstance().DiaEventTrigger("01_LookAtMedicalKit");
            }
        }
    }
}
