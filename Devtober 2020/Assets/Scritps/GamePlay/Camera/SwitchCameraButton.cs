using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCameraButton : MonoBehaviour
{
    public string RoomName;
    public int CameraIndex;

    public void Click()
    {
        GameManager.GetInstance().RoomSwitch(RoomName, CameraIndex);
    }
}