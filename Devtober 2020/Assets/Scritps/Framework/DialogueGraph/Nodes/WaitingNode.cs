using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using System;
using System.Text.RegularExpressions;

namespace DiaGraph
{
    [NodeWidth(300)]
    [CreateNodeMenu("Waiting", order = 2)]
    [NodeTint("#FF6347")]//番茄红
    public class WaitingNode : Node
    {

        [Input] public Empty Input;
        [Output(connectionType = ConnectionType.Override, dynamicPortList = true)] public List<WaitingNodeEvent> WaitingOption;

        // Use this for initialization
        protected override void Init()
        {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }

        public Node MoveNext(int index)
        {
            foreach (var port in DynamicOutputs)
            {
                if (port.fieldName == "WaitingOption " + index.ToString())
                {
                    if (!port.IsConnected)
                    {
                        FinishDia();
                        return this;
                    }
                    else
                    {
                        return port.Connection.node;
                    }
                }
            }
            FinishDia();
            return this;
        }

        public void StartWaiting()
        {
            for (int i = 0; i < WaitingOption.Count; i++)
            {
                switch (WaitingOption[i].waitingFor)
                {
                    case WAITINGFOR.Time:
                        EventCenter.GetInstance().AddTimeListener(WaitingOption[i].WaitSeconds, MoveNextByTime, i);
                        break;
                    case WAITINGFOR.Event:
                        EventCenter.GetInstance().AddEventListener<string>(WaitingOption[i].Event, MoveNextByEvent);
                        break;
                    default:
                        break;
                }
            }
        }

        public void MoveNextByTime(int index)
        {
            GoNext(index);
        }

        public void MoveNextByEvent(string EventName)
        {
            EventCenter.GetInstance().RemoveEventListenerKeys(EventName);
            for (int i = 0; i < WaitingOption.Count; i++)
            {
                if(WaitingOption[i].Event == EventName)
                {
                    GoNext(i);
                    break;
                }
            }
        }

        void FinishDia()
        {
            DialogueGraph diaGraph = graph as DialogueGraph;
            if (diaGraph != null)
            {
                diaGraph.DiaPlay.Finished();
            }
        }

        void GoNext(int index)
        {
            DialogueGraph diaGraph = graph as DialogueGraph;
            if (diaGraph != null)
            {
                diaGraph.IsWaiting = false;
                Debug.Log(index);
                diaGraph.DiaPlay.Next(index);
            }
        }

        public string GetBriefInfo()
        {
            string temp = string.Empty;
            if(WaitingOption != null)
            {
                if (WaitingOption.Count >= 1)
                {
                    if (WaitingOption[0].waitingFor == WAITINGFOR.Time)
                    {
                        temp = "Waiting for:" + WaitingOption[0].WaitSeconds + "s";
                    }
                    else
                    {
                        temp = "Waiting for:" + WaitingOption[0].Event;
                        if (temp.IndexOf('\n') > 0)
                            temp = Regex.Match(WaitingOption[0].Event, @".+(?=\n)").Value;
                    }
                }
                else
                {
                    temp = "Waiting Node";
                }
            }
            
            
            if (temp.Length >= 15)
            {
                temp = temp.Substring(0, 14) + "…";
            }
            return temp;
        }
    }

    public enum WAITINGFOR
    {
        Time,
        Event
    }

    [Serializable]
    public class WaitingNodeEvent
    {
        public WAITINGFOR waitingFor = WAITINGFOR.Time;
        public float WaitSeconds = 0;
        public string Event;
    }
}