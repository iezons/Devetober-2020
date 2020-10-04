using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiaGraph;
using UnityEngine.EventSystems;

public enum DiaState
{
    OFF,
    TYPING,
    PAUSED
}

public enum NodeState
{
    Dialogue,
    Option,
    Start,
    WaitNode
}

public class DialoguePlay : MonoBehaviour
{
    [Header("Properties")]
    public DialogueGraph currentGraph;
    public string Language = "English";
    public DiaState d_state = DiaState.OFF;
    public NodeState n_state = NodeState.Start;
    [Header("Settings")]
    public float TextSpeed;
    [Header("Runtime Var")]
    public string WholeText;
    public int MaxVisible;
    int WordCount = 0;
    float timervalue;
    int LastTimerValue = 0;
    bool justEnter;
    bool isFinished = true;

    void Awake()
    {
        EventCenter.GetInstance().AddEventListener<DialogueGraph>("DialoguePlay.Start", PlayDia);
        EventCenter.GetInstance().AddEventListener<int>("DialoguePlay.Next", Next);
        EventCenter.GetInstance().AddEventListener("DialoguePlay.Finished", Finished);
    }

    void Finished()
    {
        Debug.Log("Finished");
        isFinished = true;
    }

    void Update()
    {
        switch (d_state)
        {
            case DiaState.OFF:
                if(justEnter)
                {
                    LastTimerValue = 0;
                    MaxVisible = 0;
                    timervalue = 0;
                    LastTimerValue = -1;
                    WholeText = string.Empty;
                    justEnter = false;
                }
                break;
            case DiaState.TYPING:
                if (justEnter)
                {
                    LastTimerValue = 0;
                    MaxVisible = 0;
                    timervalue = 0;
                    LastTimerValue = -1;
                    justEnter = false;
                }
                if(n_state == NodeState.Dialogue)
                {
                    UpdateText();
                    CheckTypingFinished();
                }
                else if(n_state == NodeState.Option)
                {
                    GoToSTATE(DiaState.PAUSED);
                }
                break;
            case DiaState.PAUSED:
                if (justEnter)
                {
                    justEnter = false;
                }
                break;
            default:
                break;
        }
    }

    void LoadText(string Text)
    {
        WholeText = Text;
        WordCount = Text.Length;
    }

    void UpdateText()
    {
        timervalue += Time.deltaTime * TextSpeed;
        int diff = (int)Mathf.Floor(timervalue) - LastTimerValue;
        for (int g = 0; g < diff; g++)
        {
            MaxVisible++;
        }
        int Min = (int)Mathf.Floor(timervalue);
        LastTimerValue = Min;
    }

    void CheckTypingFinished()
    {
        if (d_state == DiaState.TYPING)
        {
            if ((int)Mathf.Floor(timervalue) >= WordCount)
            {
                GoToSTATE(DiaState.PAUSED);
                EventCenter.GetInstance().EventTriggered("DialoguePlay.PAUSED");
            }
        }
    }

    void PlayDia(DialogueGraph graph)
    {
        currentGraph = graph;
        Next();
    }

    void Next(int OptionIndex = 0)
    {
        switch (d_state)
        {
            case DiaState.OFF:
                Debug.Log("StartFromOFF");
                currentGraph.SetStartPoint("English");
                currentGraph.Next(OptionIndex);
                LoadNodeInfo();
                GoToSTATE(DiaState.TYPING);
                isFinished = false;
                break;
            case DiaState.TYPING:
                break;
            case DiaState.PAUSED:
                if(isFinished)
                {
                    GoToSTATE(DiaState.OFF);
                }
                else
                {
                    currentGraph.Next(OptionIndex);
                    LoadNodeInfo();
                    GoToSTATE(DiaState.TYPING);
                }
                break;
            default:
                break;
        }
    }

    void LoadNodeInfo()
    {
        DialogueNode dia = currentGraph.current as DialogueNode;
        if (dia != null)
        {
            LoadText(dia.Dialogue[dia.curIndex]);
            n_state = NodeState.Dialogue;
        }
        else
        {
            OptionNode opt = currentGraph.current as OptionNode;
            if(opt != null)
            {
                n_state = NodeState.Option;
                EventCenter.GetInstance().EventTriggered("DialoguePlay.OptionShowUP", opt.Option);
            }
        }
    }

    void GoToSTATE(DiaState next)
    {
        justEnter = true;
        d_state = next;
    }
}