using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    List<Camera> cameraList = new List<Camera>();

    private void Awake()
    {
        for(int i=0; i<Camera.allCameras.Length; i++)
        {
            cameraList.Add(Camera.allCameras[i]);
        }
    }

    public void CameraSwtich(string buttonName)
    {
        for(int i=0; i<cameraList.Count; i++)
        {
            cameraList[i].enabled = false;
            if(cameraList[i].gameObject.tag == buttonName)
            {
                cameraList[i].enabled = true;            }
        }
    }
}
