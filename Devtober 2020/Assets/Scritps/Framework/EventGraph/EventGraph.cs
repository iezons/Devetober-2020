using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;

namespace EvtGraph
{
    [CreateAssetMenu(menuName = "Graph/Event Graph")]
    public class EventGraph : NodeGraph
    {
        public EventNode eventNode;

        public Node current;

        public void Next()
        {
            StartNode sta = current as StartNode;
            EventNode evt = current as EventNode;
            if(sta != null)
            {
                current = sta.MoveNext();
            }
            else if(evt != null)
            {
                current = evt.MoveNext();
            }
        }

        public void SetNode(string NodeGUID = "StartNode0_First")
        {
            if(NodeGUID == "StartNode0_First")
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
                    else if(temp.Count == 0)
                    {
                        Debug.LogError("There is not a starting point for current event graph");
                        Debug.Break();
                    }
                    else
                    {
                        current = temp[0];
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
                for (int i = 0; i < temp.Count; i++)
                {
                    EventNode tEvent = temp[i] as EventNode;
                    if (tEvent != null)
                    {
                        if (tEvent.GUID == NodeGUID)
                        {
                            current = tEvent;
                        }
                    }
                }
                if (current == null)
                {
                    Debug.LogError("Cannot Find a EventNode with GUID provided");
                    Debug.Break();
                }
            }
        }
    }
}