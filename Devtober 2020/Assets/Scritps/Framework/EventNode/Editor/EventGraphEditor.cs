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
            if (type == typeof(BasicEventNode))
            {
                return base.GetNodeMenuName(type);
            }
            else return null;
        }
    }
}
