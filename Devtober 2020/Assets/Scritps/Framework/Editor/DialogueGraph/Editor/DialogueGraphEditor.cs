using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using GraphBaseEditor;

namespace DiaGraph
{
    [CustomNodeGraphEditor(typeof(DialogueGraph))]
    public class DialogueGraphEditor : NodeGraphEditor
    {
        private DialogueGraph dialogueGraph;

        public override void OnOpen()
        {
            base.OnOpen();
            if (dialogueGraph == null) dialogueGraph = window.graph as DialogueGraph;
            dialogueGraph.Open();
        }

        public override string GetNodeMenuName(System.Type type)
        {
            if (type == typeof(DialogueNode))
            {
                return base.GetNodeMenuName(type);
            }
            else if (type == typeof(OptionNode))
            {
                return base.GetNodeMenuName(type);
            }
            else if (type == typeof(StartNode))
            {
                return base.GetNodeMenuName(type);
            }
            else if (type == typeof(WaitingNode))
            {
                return base.GetNodeMenuName(type);
            }
            else return null;
        }

    }
}

