using GraphBase;
using GraphBaseEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;

namespace DiaGraph
{
    [CustomNodeEditor(typeof(WaitingNode))]
    public class WaitingNodeEditor : NodeEditor
    {
        private WaitingNode waitingNode;
        private DialogueGraph dialogueGraph;
        private ReorderableList WaitingFor;

        public override void OnCreate()
        {
            base.OnCreate();
            if (waitingNode == null) waitingNode = target as WaitingNode;
            WaitingFor = new ReorderableList(serializedObject, serializedObject.FindProperty("WaitingOption"), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    GUI.Label(rect, "Waiting Option");
                }
            };

            WaitingFor.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
            {
                WaitingFor.elementHeight = 2 * EditorGUIUtility.singleLineHeight + 2;
                WaitingNodeEvent wne = waitingNode.WaitingOption[index];
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.y += 2;
                //wne.waitingFor = EditorGUI.Popup(rect, "Waiting for", wne.waitingFor, );
                EditorGUI.PropertyField(rect, serializedObject.FindProperty("WaitingOption").GetArrayElementAtIndex(index).FindPropertyRelative("waitingFor"));
                
                rect.y += EditorGUIUtility.singleLineHeight;
                switch (wne.waitingFor)
                {
                    case WAITINGFOR.Time:
                        EditorGUI.PropertyField(rect, serializedObject.FindProperty("WaitingOption").GetArrayElementAtIndex(index).FindPropertyRelative("WaitSeconds"));
                        break;
                    case WAITINGFOR.Event:
                        EditorGUI.PropertyField(rect, serializedObject.FindProperty("WaitingOption").GetArrayElementAtIndex(index).FindPropertyRelative("Event"));
                        break;
                    default:
                        break;
                }
            };
        }

        public override void OnHeaderGUI()
        {
            //base.OnHeaderGUI();
            GUI.color = Color.white;
            if (waitingNode == null) waitingNode = target as WaitingNode;
            if (dialogueGraph == null) dialogueGraph = window.graph as DialogueGraph;
            if (dialogueGraph.current == waitingNode) GUI.color = Color.blue;
            GUILayout.Label(waitingNode.GetBriefInfo(), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            waitingNode.name = waitingNode.GetBriefInfo();
        }

        public override void OnBodyGUI()
        {
            GUI.color = Color.white;
            base.OnBodyGUI();
            
            //serializedObject.Update();
            //if (waitingNode == null) waitingNode = target as WaitingNode;
            //foreach (NodePort Port in target.Ports)
            //{
            //    //if (NodeEditorGUILayout.IsDynamicPortListPort(Port)) continue;
            //    NodeEditorGUILayout.PortField(Port);
            //}

            //WaitingFor.DoLayoutList();
            //serializedObject.ApplyModifiedProperties();
        }
    }
}
