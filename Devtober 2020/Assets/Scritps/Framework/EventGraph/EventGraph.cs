using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;

namespace EvtGraph
{
    public class EventGraph : NodeGraph
    {
        public StartNode startNode;
        public EventNode eventNode;

        public List<Node> currentList = new List<Node>();

        public void Next(Node current)
        {
            currentList.Clear();
            StartNode sta = current as StartNode;
            EventNode evt = current as EventNode;
            if(sta != null)
            {
                currentList.AddRange(sta.MoveNext());
            }
            else if(evt != null)
            {
                currentList.AddRange(evt.MoveNext());
            }
        }

        public List<Node> SetNode(List<string> NodeGUID)
        {
            currentList.Clear();
            for (int i = 0; i < NodeGUID.Count; i++)
            {
                if (NodeGUID[i] == "StartNode")
                {
                    List<Node> temp = nodes.FindAll(node =>
                    {
                        return node.GetType() == typeof(StartNode);
                    });
                    if (temp != null)
                    {
                        if (temp.Count > 1)
                        {
                            Debug.LogError("There are multiple Start Node for this Event Graph. You can only have one Start Node for One Graph");
                        }
                        else if (temp.Count == 0)
                        {
                            Debug.LogError("There is not a starting point for current event graph");
                            Debug.Break();
                        }
                        else
                        {
                            currentList.Add(temp[0]);
                        }
                    }
                    else
                    {
                        Debug.LogError("There is not a starting point for current event graph");
                        Debug.Break();
                    }
                }
                else
                {
                    List<Node> temp = nodes.FindAll(node =>
                    {
                        return node.GetType() == typeof(EventNode);
                    });
                    for (int a = 0; a < temp.Count; a++)
                    {
                        EventNode tEvent = temp[a] as EventNode;
                        if (tEvent != null)
                        {
                            if (tEvent.GUID == NodeGUID[a])
                            {
                                currentList.Add(tEvent);
                            }
                        }
                    }
                }
            }
            if (currentList.Count <= 0)
            {
                Debug.LogError("Cannot Find a EventNode with GUID provided");
                Debug.Break();
                return null;
            }
            else
            {
                return currentList;
            }
        }
    }
}