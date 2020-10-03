using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvtGraph;

public enum GameManagerState
{
    OFF,
    PROGRESSING,
    PAUSED
}

public class GameManager : MonoBehaviour
{
    public EventGraph eventGraph;
    public List<GameObject> NPC;
    public List<GameObject> Room;
    public GameManagerState gmState;

    bool justEnter;

    // Start is called before the first frame update
    void Awake()
    {
        SetupScene();
    }

    // Update is called once per frame
    void Update()
    {
        switch (gmState)
        {
            case GameManagerState.OFF:
                if(justEnter)
                {
                    justEnter = false;
                }
                break;
            case GameManagerState.PROGRESSING:
                if (justEnter)
                {
                    justEnter = false;
                }
                break;
            case GameManagerState.PAUSED:
                if (justEnter)
                {
                    justEnter = false;
                }
                break;
            default:
                break;
        }
    }

    void SetupScene()
    {
        
    }

    void GoToState(GameManagerState next)
    {
        justEnter = true;
        gmState = next;
    }

    void EventMoveOn()
    {

    }

    void Next()
    {
        switch (gmState)
        {
            case GameManagerState.OFF:
                eventGraph.SetNode();
                eventGraph.Next();
                TriggerEvent();
                break;
            case GameManagerState.PROGRESSING:
                break;
            case GameManagerState.PAUSED:
                eventGraph.Next();
                TriggerEvent();
                break;
            default:
                break;
        }
    }

    void TriggerEvent()
    {
        GoToState(GameManagerState.PROGRESSING);
        //Trigger through EventSO
    }
}
