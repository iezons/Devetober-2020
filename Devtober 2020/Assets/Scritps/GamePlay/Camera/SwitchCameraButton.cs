using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SwitchCameraButton : MonoBehaviour
{
    public string RoomName;
    public int CameraIndex;
    public Color NormalColor;
    public Color HighLightColor;
    public float FlashingTime;
    [SerializeField]
    float CurrentFlashingTime = 0;

    public Button button;
    [SerializeField]
    bool HighLight = false;
    [SerializeField]
    Color tempca = Color.white;
    [SerializeField]
    Color tempcb = Color.white;

    void OnEnable()
    {
        button = GetComponent<Button>();
    }

    public void AfterInstantiate()
    {
        EventCenter.GetInstance().AddEventListener(RoomName + CameraIndex.ToString() + "CameraEvent", EventHappening);
    }

    void EventHappening()
    {
        tempca = NormalColor;
        tempcb = HighLightColor;
        HighLight = true;
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
        if (HighLight)
        {
            CurrentFlashingTime += Time.deltaTime * FlashingTime;
            if(CurrentFlashingTime >= 1)
            {
                Color a = tempca;
                Color b = tempcb;
                tempca = b;
                tempcb = a;
                CurrentFlashingTime = 0;
            }
            ColorBlock cb = button.colors;
            cb.normalColor = Color.Lerp(tempca, tempcb, CurrentFlashingTime);
            button.colors = cb;
        }
    }

    public void Click()
    {
        GameManager.GetInstance().RoomSwitch(RoomName, CameraIndex);
        HighLight = false;
        ColorBlock cb = button.colors;
        cb.normalColor = NormalColor;
        button.colors = cb;
    }
}