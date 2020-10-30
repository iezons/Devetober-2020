using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiaGraph;
using UnityEngine.EventSystems;
using GamePlay;
using System.Text.RegularExpressions;
using System.Linq;

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
    public RoomTracker roomTracker;
    public DialogueGraph currentGraph;
    public string Language = "English";
    public DiaState d_state = DiaState.OFF;
    public NodeState n_state = NodeState.Start;
    [Header("Settings")]
    public float TextSpeed = 18;
    [Header("Runtime Var")]
    public string WholeText;
    public int MaxVisible;
    public int WordCount = 0;
    float timervalue;
    int LastTimerValue = 0;
    bool justEnter;
    bool isFinished = true;
    [Header("Playing Var")]
    public float WaitingTime = 0.5f;
    bool IsWaitingTime = false;
    float AlreadyWaitTime = 0f;
    bool IsTypingSpeed = false;
    float DefaultTypingSpeed = 0f;
    float ChangedTypingSpeed = 0f;
    bool IsMuting = false;
    Dictionary<int, List<string>> EventList = new Dictionary<int, List<string>>();


    void Awake()
    {
        DefaultTypingSpeed = TextSpeed;
        roomTracker = GetComponent<RoomTracker>();
        //EventCenter.GetInstance().AddEventListener("DialoguePlay.Finished", Finished);
    }

    public void Finished()
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
                    currentGraph = null;
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
                    timervalue = 0;
                    LastTimerValue = -1;
                    justEnter = false;
                }
                if(n_state == NodeState.Dialogue)
                {
                    if(IsTypingSpeed)
                    {
                        TextSpeed = ChangedTypingSpeed;
                    }
                    else
                    {
                        TextSpeed = DefaultTypingSpeed;
                    }
                    if(IsWaitingTime)
                    {
                        if(AlreadyWaitTime >= WaitingTime)
                        {
                            AlreadyWaitTime = 0f;
                            IsWaitingTime = false;
                        }
                        else
                        {
                            AlreadyWaitTime += Time.deltaTime;
                        }
                    }
                    else
                    {
                        UpdateText();
                    }
                    CheckTypingFinished();
                }
                else if(n_state == NodeState.Option)
                {
                    GoToSTATE(DiaState.PAUSED);
                }
                else if(n_state == NodeState.WaitNode)
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

    void LoadText(string TalkingPersonName, string Text)
    {
        string temp = string.Empty;
        if (TalkingPersonName != string.Empty)
        {
            temp = "<color=#00FF00>" + TalkingPersonName + ": " + "</color>" + Text;
            MaxVisible = TalkingPersonName.Length + 2;
        }
        else
        {
            temp = Text;
            MaxVisible = 0;
        }
        //WordCount = Text.Length;
        string pattern = "(?<=\\<)[^\\>]+";
        UpdateContent(temp, pattern);
    }

    void UpdateContent(string text, string pattern)
    {
        string temp = text;
        EventList.Clear();
        MatchCollection match = Regex.Matches(temp, pattern);
        int StartingIndex = 0;
        int SymbolLength = 0;
        for (int i = 0; i < match.Count; i++)
        {
            Match d_match = Regex.Match(temp.Substring(StartingIndex, temp.Length - StartingIndex - 1), pattern);
            if(d_match.Value[0].ToString() == "$")
            {
                string Value = d_match.Value;
                int StringNum = Value.Length + 2;
                int index = d_match.Index - 1;
                temp = temp.Remove(index + StartingIndex, StringNum);
                //Insert Profile Value
            }
            else
            {
                string Value = d_match.Value;
                if(Value.Contains("w=") || Value.Contains("sp=") || Value.Contains("e=") || Value.Equals("sp") || Value.Equals("w") || Value.Equals("/sp"))
                {
                    int StringNum = Value.Length + 2;
                    int Index = d_match.Index - 1;
                    temp = temp.Remove(Index + StartingIndex, StringNum);
                    if(EventList.ContainsKey(Index + StartingIndex - SymbolLength))
                    {
                        EventList[Index + StartingIndex - SymbolLength].Add(Value);
                    }
                    else
                    {
                        EventList.Add(Index + StartingIndex - SymbolLength, new List<string>() { Value });
                    }
                }
                else if (Value.Contains("sprite="))
                {
                    StartingIndex += d_match.Index + Value.Length;
                    SymbolLength += Value.Length + 1;
                }
                else
                {
                    StartingIndex += d_match.Index + Value.Length;
                    SymbolLength += Value.Length + 2;
                }
            }
        }
        WholeText = temp;
        WordCount = temp.Length - SymbolLength;
    }

    void UpdateText()
    {
        timervalue += Time.deltaTime * TextSpeed;
        int diff = (int)Mathf.Floor(timervalue) - LastTimerValue;
        for (int g = 0; g < diff; g++)
        {
            int Min2 = (int)Mathf.Floor(LastTimerValue + g);

            if(IsWaitingTime)
            {
                LastTimerValue--;
            }
            else
            {
                MaxVisible++;
                if(!IsMuting)
                {
                    //Play Voice
                }
            }

            if (EventList.ContainsKey(MaxVisible))
            {
                for (int i = 0; i < EventList[MaxVisible].Count; i++)
                {
                    EventListTrigger(EventList[MaxVisible][i]);
                }
                EventList.Remove(MaxVisible);
            }
        }
        int Min = (int)Mathf.Floor(timervalue);
        if(IsWaitingTime)
        {
            LastTimerValue = Min - diff;
        }
        else
        {
            LastTimerValue = Min;
        }
    }

    void EventListTrigger(string str)
    {
        switch (str)
        {
            case "w":
                IsWaitingTime = true;
                WaitingTime = 0.5f;
                break;
            case "sp":
                IsTypingSpeed = true;
                ChangedTypingSpeed = DefaultTypingSpeed;
                break;
            case "/sp":
                IsTypingSpeed = false;
                break;
            default:
                break;
        }

        if(str.Contains("="))
        {
            string[] sub = str.Split('=');
            switch (sub[0])
            {
                case "w":
                    IsWaitingTime = true;
                    WaitingTime = float.Parse(sub[1]);
                    break;
                case "sp":
                    IsTypingSpeed = true;
                    ChangedTypingSpeed = float.Parse(sub[1]);
                    break;
                case "e":
                    string[] eventNames = sub[1].Split(',');
                    for (int i = 0; i < eventNames.Length; i++)
                    {
                        EventCenter.GetInstance().EventTriggered(eventNames[i]);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void CheckTypingFinished()
    {
        if (d_state == DiaState.TYPING)
        {
            if (MaxVisible > WordCount)
            {
                if(isFinished)
                {
                    GoToSTATE(DiaState.OFF);
                    //EventCenter.GetInstance().EventTriggered("DialoguePlay.OFF");
                    roomTracker.DialogueOFF();
                }
                else
                {
                    GoToSTATE(DiaState.PAUSED);
                    //EventCenter.GetInstance().EventTriggered("DialoguePlay.PAUSED");
                    roomTracker.DialoguePaused();
                }
            }
        }
    }

    public void PlayDia(DialogueGraph graph)
    {
        currentGraph = graph;
        graph.DiaPlay = this;
        Next();
    }

    public void Next(int OptionIndex = 0)
    {
        switch (d_state)
        {
            case DiaState.OFF:
                currentGraph.SetStartPoint("English");
                currentGraph.Next(OptionIndex);
                LoadNodeInfo();
                GoToSTATE(DiaState.TYPING);
                isFinished = false;
                break;
            case DiaState.TYPING:
                break;
            case DiaState.PAUSED:
                if(currentGraph.Next(OptionIndex))
                {
                    if (isFinished)
                    {
                        GoToSTATE(DiaState.OFF);
                    }
                    else
                    {
                        LoadNodeInfo();
                    }
                }
                break;
            default:
                break;
        }
    }

    void LoadNodeInfo()
    {
        DialogueNode dia = currentGraph.current as DialogueNode;
        OptionNode opt = currentGraph.current as OptionNode;
        WaitingNode wat = currentGraph.current as WaitingNode;
        if (dia != null)
        {
            LoadText(dia.TalkingPerson, dia.Dialogue[dia.curIndex]);
            n_state = NodeState.Dialogue;
            GoToSTATE(DiaState.TYPING);
        }
        else if (opt != null)
        {
            MaxVisible = 0;
            n_state = NodeState.Option;
            roomTracker.DialogueOptionShowUp(opt.Option);
            //GoToSTATE(DiaState.TYPING);
        }
        else if(wat != null)
        {
            MaxVisible = 0;
            n_state = NodeState.WaitNode;
        }
    }

    void GoToSTATE(DiaState next)
    {
        justEnter = true;
        d_state = next;
    }
}