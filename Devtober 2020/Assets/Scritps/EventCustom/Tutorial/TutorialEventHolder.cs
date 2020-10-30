using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialEventHolder : MonoBehaviour
{
    public GameObject CameraScreen;
    public GameObject CameraText;
    public GameObject NPCListPanel;
    public GameObject CameraListPanel;

    private void Awake()
    {
        //EventCenter.GetInstance().AddEventListener();
        a("TU_ShowCamera", () => { CameraScreen.SetActive(true); CameraText.SetActive(true); }) ;
        a("TU_UnlockCameraToLeft", () => { GameManager.GetInstance().CanCameraTurnLeft = true; }) ;
        a("TU_UnlockCameraToRight", () => { GameManager.GetInstance().CanCameraTurnRight = true; }) ;
        a("TU_UnlockCameraToRight", () => { GameManager.GetInstance().CanCameraTurnRight = true; }) ;
        a("TU_NPCList_Show", () => { NPCListPanel.SetActive(true); }) ;
        a("TU_SwitchToMainLevel", () => { GameManager.GetInstance().Stage = 1; GameManager.GetInstance().SetupStage(1);});
        a("TU_ShowCameraList", () => { CameraListPanel.SetActive(true); });
    }

    void a(string EventName, UnityAction action = null)
    {
        EventCenter.GetInstance().AddEventListener(EventName, action);
    }

    void b(string name)
    {
        EventCenter.GetInstance().DiaEventTrigger(name);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.GetInstance().CurrentRoom.cameraLists[0].plusAngle <= -GameManager.GetInstance().CurrentRoom.cameraLists[0].angle / 2 + 1f)
        {
            b("TU_TurnLeftCheck");
        }

        if (GameManager.GetInstance().CurrentRoom.cameraLists[0].plusAngle >= GameManager.GetInstance().CurrentRoom.cameraLists[0].angle / 2 - 1f)
        {
            b("TU_TurnRightCheck");
        }
    }

    public void EventTrigger(string EventName)
    {
        EventCenter.GetInstance().EventTriggered(EventName, EventName);
    }
}
