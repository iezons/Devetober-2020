using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject DonDestroy;
    public InputField InputText;
    public TMP_Text ConsoleText;
    public Scrollbar Sbar;

    // Start is called before the first frame update
    void Start()
    {
        //InputText.Select();
        EventSystem.current.SetSelectedGameObject(InputText.gameObject);
        //EventCenter.GetInstance().AddEventListener<float>("ScenesMgr_ProgressUpdate", UpdateProgress);
    }

    // Update is called once per frame
    void Update()
    {
        if(EventSystem.current.currentSelectedGameObject == InputText.gameObject && Input.GetKeyUp(KeyCode.Return))
        {
            StartCoroutine(UpdateConsole(InputText.text));
        }
    }

    void UpdateProgress(float progress)
    {
        float p = Mathf.Floor(Mathf.Clamp(progress * 100, 0, 90));
        ConsoleText.text = "Connecting: " + p.ToString() + "%" + "\n";
        Sbar.value = 0;
    }

    void DestoryThis()
    {
        Destroy(DonDestroy);
    }

    IEnumerator UpdateConsole(string input)
    {
        InputText.interactable = false;
        ConsoleText.text += ">><color=#00FF00>" + input + "</color>" + "\n";
        InputText.text = string.Empty;
        yield return new WaitForSeconds(0.8f);
        string inp = input.ToLower();
        switch (inp)
        {
            case "connect":
                InputText.interactable = false;
                DontDestroyOnLoad(DonDestroy);
                ConsoleText.text += "Connecting" + "\n";
                ScenesMgr.GetInstance().LoadSceneAsyn("Level", DestoryThis, UpdateProgress);
                break;
            case "info":
                InputText.interactable = false;
                ConsoleText.text += "Searching Data......" + "\n";
                //Searching Data......

                //20%

                //80%

                //100%

                //System info: [Rewritten: Unknown]
                //Last boot: 2049/10/27 - 20:56:23
                //It's been ***** days since the last boot
                //Searching for Credit List......
                //
                //List:
                //      Hongming Wang - Project Leader  
                //                      Game Designer   
                //                      Level Designer  
                //                      Programmer      
                //                      3D artist
                //                      2D artist
                //                      Writter
                //
                //                                        
                //      Jeffery Hu    - Animator                                     
                //                      3D artist
                //                      2D artist                                     
                //                      Writter
                //
                //
                //
                //
                //
                //
                //
                InputText.interactable = true;
                EventSystem.current.SetSelectedGameObject(InputText.gameObject);
                break;
            case "exit":
                ConsoleText.text = "Exit";
                yield return new WaitForSeconds(0.6f);
                Application.Quit();
                break;
            case "help":
                ConsoleText.text += "Connect       Connect to main server" + "\n" + "Info          Display the information of current system" + "\n" + "Exit          Turn off system" + "\n";
                InputText.interactable = true;
                EventSystem.current.SetSelectedGameObject(InputText.gameObject);
                break;
            default:
                ConsoleText.text += input + " is not an internal or external command, nor is it an executable program." +  "\n";
                yield return new WaitForSeconds(0.4f);
                ConsoleText.text += "Type <color=#00FF00>Help</color> for helping. " + "\n";
                InputText.interactable = true;
                EventSystem.current.SetSelectedGameObject(InputText.gameObject);
                break;
        }
    }
}
