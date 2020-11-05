using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenButton : MonoBehaviour
{
    public Text btnText;

    private void Awake()
    {
        btnText = GetComponentInChildren<Text>();
    }

    private void Start()
    {
        if(Screen.fullScreen)
        {
            btnText.text = "WINDOWED";
        }
        else
        {
            btnText.text = "FULL SCREEN";
        }
    }

    public void Click()
    {
        if(Screen.fullScreen)
        {
            Resolution[] res = UnityEngine.Screen.resolutions;
            if (res.Length > 0)
            {
                if (res[0].width >= res[1].height)
                {
                    UnityEngine.Screen.SetResolution(res[0].width, res[0].width / 16 * 9, false);
                    Debug.Log(res[0].width.ToString() + "x" + (res[0].width / 16 * 9).ToString());
                }
                else
                {
                    UnityEngine.Screen.SetResolution(res[0].height / 9 * 16, res[0].height, false);
                    Debug.Log((res[0].height / 9 * 16).ToString() + "x" + res[0].height.ToString());
                }
            }
            btnText.text = "FULL SCREEN";
        }
        else
        {
            Resolution[] res = UnityEngine.Screen.resolutions;
            if (res.Length > 0)
            {
                if (res[0].width >= res[1].height)
                {
                    UnityEngine.Screen.SetResolution(res[0].width, res[0].width / 16 * 9, true);
                }
                else
                {
                    UnityEngine.Screen.SetResolution(res[0].height / 9 * 16, res[0].height, true);
                }
            }
            btnText.text = "WINDOWED";
        }
    }
}
