using GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible : MonoBehaviour
{
    public RoomTracker tracker;
    public Transform RoomCamera;

    private void Update()
    {
        if(Time.frameCount % 2 == 0)
        {
            if (RoomCamera.eulerAngles.y > 94f && GameManager.GetInstance().CurrentRoom == tracker)
            {
                EventCenter.GetInstance().DiaEventTrigger("01_LookAtMedicalKit");
            }
        }
    }
}
