using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverRetry : MonoBehaviour
{
    public Text text;
    public Button btn;

    public void OnEnable()
    {
        btn = GetComponent<Button>();
        text = GetComponentInChildren<Text>();
    }

    public void Click()
    {
        btn.interactable = false;
        ScenesMgr.GetInstance().LoadSceneAsyn("Level", null, UpdateProgress);
    }

    void UpdateProgress(float pro)
    {
        float p = Mathf.Floor(Mathf.Clamp(pro * 100, 0, 90));
        text.text = "Connecting: " + p.ToString() + "%" + "\n";
    }
}
