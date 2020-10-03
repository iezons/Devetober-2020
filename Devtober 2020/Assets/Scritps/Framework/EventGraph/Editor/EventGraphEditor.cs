using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using GraphBaseEditor;

namespace EvtGraph
{
    [CustomNodeGraphEditor(typeof(EventGraph))]
    public class EventGraphEditor : NodeGraphEditor
    {
        public override string GetNodeMenuName(System.Type type)
        {
            if (type == typeof(EventNode))
            {
                return base.GetNodeMenuName(type);
            }
            else if (type == typeof(StartNode))
            {
                return base.GetNodeMenuName(type);
            }
            else return null;
        }

        public override void OnDropObjects(Object[] objects)
        {
            
        }
    }
}
