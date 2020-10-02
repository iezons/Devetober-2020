using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBaseEditor;

namespace DialogueGraph
{
    [CustomNodeEditor(typeof(OptionNode))]
    public class OptionNodeEditor : NodeEditor
    {
        private OptionNode optionNode;
        private DialogueGraph dialogueGraph;

        public override void OnHeaderGUI()
        {
            //base.OnHeaderGUI();
            GUI.color = Color.white;
            if (optionNode == null) optionNode = target as OptionNode;
            if (dialogueGraph == null) dialogueGraph = window.graph as DialogueGraph;
            if (dialogueGraph.current == optionNode) GUI.color = Color.blue;
            GUILayout.Label("Option: " + optionNode.GetBriefOption(), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            optionNode.name = "Option: " + optionNode.GetBriefOption();
        }

        public override void OnBodyGUI()
        {
            GUI.color = Color.white;
            base.OnBodyGUI();
        }
    }
}
