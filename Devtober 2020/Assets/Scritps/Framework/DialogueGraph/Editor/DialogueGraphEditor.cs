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

    }
}

