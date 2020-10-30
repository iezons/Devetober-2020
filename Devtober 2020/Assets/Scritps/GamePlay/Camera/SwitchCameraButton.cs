using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchCameraButton : MonoBehaviour
{
    public string RoomName;
    public int CameraIndex;

    public Button button;

    public void OnEnable()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        if (GameManager.GetInstance().CurrentRoom.RoomName() == RoomName && GameManager.GetInstance().CurrentRoom.CurrentCameraIndex == CameraIndex)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }

    public void Click()
    {
        GameManager.GetInstance().RoomSwitch(RoomName, CameraIndex); 
    }
}