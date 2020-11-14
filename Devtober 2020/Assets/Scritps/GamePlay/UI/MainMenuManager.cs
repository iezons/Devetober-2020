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
        if(InputText.interactable)
            EventSystem.current.SetSelectedGameObject(InputText.gameObject);
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
                yield return new WaitForSeconds(1f);
                ConsoleText.text += "Last boot: 2049A.U.C/10/27 - 20:56:23" + "\n";
                yield return new WaitForSeconds(0.6f);
                ConsoleText.text += "System info: [Rewritten: Unknown]" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "Searching for Credit List......" + "\n";
                yield return new WaitForSeconds(1.5f);
                ConsoleText.text = "List:" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "      Hongming Wang “Iezons” - Project Leader" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Game Designer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Level Designer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Programmer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      3D Artist" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      2D Artist" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Writter" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Charater Designer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Animator" + "\n";
                yield return new WaitForSeconds(3.5f);
                ConsoleText.text = "List:" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "      Jeffery Hu - Animator" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                   3D Artist" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                   2D Artist" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                   Charater Designer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                   Writter" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                   Animator" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                   QA" + "\n";
                yield return new WaitForSeconds(3.5f);
                ConsoleText.text = "List:" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "      Lawrence Peng - Programmer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Techical Artists" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Game Designer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Cut Scene" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      Writter" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                      QA" + "\n";
                yield return new WaitForSeconds(3.5f);
                ConsoleText.text = "List:" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "      Edward Lu - Programmer" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "                  QA" + "\n";
                yield return new WaitForSeconds(3.5f);
                ConsoleText.text = "List:" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "      Carlos A. Sanchez “TacoRamenBowl” - Music" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "               Audio" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "               Sound Collect" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "               3D Artists" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "               Animator" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "               Charater Designer" + "\n";
                yield return new WaitForSeconds(3.5f);
                ConsoleText.text = "List:" + "\n";
                yield return new WaitForSeconds(0.5f);
                ConsoleText.text += "      Fernando Leonel Salinas Barrera - Voice Actor" + "\n";
                yield return new WaitForSeconds(0.5f);
                //
                //      Edward Lu - Programmer
                //
                //
                InputText.interactable = true;
                break;
            case "exit":
                ConsoleText.text = "Exit";
                yield return new WaitForSeconds(0.6f);
                Application.Quit();
                break;
            case "help":
                ConsoleText.text = "Type the following command to operate the protocol. " + "\n";
                ConsoleText.text += "<color=#00FF00>Connect</color>       Connect to main server" + "\n" + "<color=#00FF00>Info</color>          Display the information of current system" + "\n" + "<color=#00FF00>Help</color>          Show all command" + "\n" + "<color=#00FF00>Exit</color>          Turn off system" + "\n";
                InputText.interactable = true;
                break;
            default:
                ConsoleText.text += "<color=#00FF00>" + input + "</color>" + " is not an internal or external command, nor is it an executable program." +  "\n";
                yield return new WaitForSeconds(0.4f);
                ConsoleText.text += "Type <color=#00FF00>Help</color> for helping. " + "\n";
                InputText.interactable = true;
                break;
        }
    }
}
