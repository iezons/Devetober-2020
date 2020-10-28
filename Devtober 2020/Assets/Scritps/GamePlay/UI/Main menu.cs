using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    [Header("Input")]
    public string inputString;
    public int lengthOfField;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InputSwitch(string input)
    {
        switch (input)
        {
            case "reconnect":
                SceneManager.LoadScene("场景");
                break;
            case "info":
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
                break;
            case "exist":
                //exist the game
                break;
            case "Help":
                ///HELP
    //Connect Reconnect to main server
    //Info   Display the information of current system
    //Exist Turn off system
                break;
        }
    }
}
