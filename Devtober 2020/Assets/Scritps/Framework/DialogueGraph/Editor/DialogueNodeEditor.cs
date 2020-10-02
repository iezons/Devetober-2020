using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBaseEditor;
using GraphBase;
using UnityEditor;

namespace DialogueGraph
{
    [CustomNodeEditor(typeof(DialogueNode))]
    public class DialogueNodeEditor : NodeEditor
    {
        private DialogueNode dialogueNode;

        private DialogueGraph dialogueGraph;

        public override void OnHeaderGUI()
        {
            GUI.color = Color.white;
            if (dialogueNode == null) dialogueNode = target as DialogueNode;
            if (dialogueGraph == null) dialogueGraph = window.graph as DialogueGraph;
            if (dialogueGraph.current == dialogueNode) GUI.color = Color.blue;
            string temp = dialogueNode.GetBriefDialog();
            dialogueNode.name = temp;
            GUILayout.Label(temp, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        }

        public override void OnBodyGUI()
        {
            //base.OnBodyGUI();
            GUI.color = Color.white;
            if (dialogueNode == null) dialogueNode = target as DialogueNode;
            foreach (NodePort Port in target.Ports)//画出所有出入点
                NodeEditorGUILayout.PortField(Port);
            if (dialogueNode.IsMax)
            {
                serializedObject.Update();
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("TalkingPerson"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Dialogue"));
                if (GUILayout.Button("Minimize", EditorStyles.miniButton))
                    dialogueNode.IsMax = false;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                if (GUILayout.Button("Maximize", EditorStyles.miniButton))
                    dialogueNode.IsMax = true;
            }
        }
    }
}