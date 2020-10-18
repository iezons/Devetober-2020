using GraphBase;
using GraphBaseEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EvtGraph
{
    [CustomNodeEditor(typeof(EventNode))]
    public class EventNodeEditor : NodeEditor
    {
        EventNode eventNode;
        EventGraph eventGraph;
        int index;
        public override void OnHeaderGUI()
        {
            GUI.color = Color.white;
            if (eventGraph == null) eventNode = target as EventNode;
            if (eventGraph == null) eventGraph = window.graph as EventGraph;
            if (eventGraph.current == eventNode) GUI.color = Color.blue;
            string temp = eventNode.GetBriefDialog();
            eventNode.name = temp;
            GUILayout.Label(temp, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        }

        public override void OnBodyGUI()
        {
            //base.OnBodyGUI();
            GUI.color = Color.white;
            foreach (NodePort Port in target.Ports)//画出所有出入点
                NodeEditorGUILayout.PortField(Port);
            serializedObject.Update();

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("EventName"));

            GUILayout.BeginHorizontal();
            index = EditorGUILayout.IntField(eventNode.CurrentEditingSONum + 1) - 1;
            EditorGUILayout.LabelField("/" + eventNode.eventSO.Count.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                eventNode.eventSO.Insert(index + 1, new EventSO());
                index++;
            }

            if (GUILayout.Button("Remove"))
            {
                if(index >= 0 && index < eventNode.eventSO.Count)
                {
                    eventNode.eventSO.RemoveAt(index);
                    if (eventNode.eventSO.Count > 0)
                    {
                        if (index - 1 < 0)
                        {
                            index = 0;
                        }
                        else
                        {
                            index--;
                        }
                    }
                    else
                    {
                        index = -1;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Previous") && index - 1 >= 0)
            {
                index--;
            }

            if (GUILayout.Button("Next") && index + 1 <= eventNode.eventSO.Count - 1)
            {
                index++;
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            Rect rect = EditorGUILayout.GetControlRect(false, 1);

            rect.height = 1;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

            EditorGUILayout.Space(5);

            if (eventNode.eventSO.Count > eventNode.CurrentEditingSONum && eventNode.CurrentEditingSONum >= 0)
            {
                EventSO eventSO = eventNode.eventSO[eventNode.CurrentEditingSONum];
                
                if(eventSO.ID == string.Empty)
                {
                    eventSO.ID = Guid.NewGuid().ToString();
                }
                //EditorGUILayout.TextField("GUID: ", eventSO.ID);
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("doingWith"));
                switch (eventSO.doingWith)
                {
                    case DoingWith.NPC:
                        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("doingWithNPC"));
                        switch (eventSO.doingWithNPC)
                        {
                            case DoingWithNPC.Talking:
                                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("NPCTalking"));
                                break;
                            case DoingWithNPC.MoveTo:
                                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("NPCWayPoint"));
                                break;
                            case DoingWithNPC.Patrol:
                                break;
                            default:
                                break;
                        }
                        break;
                    case DoingWith.Room:
                        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("doingWithRoom"));
                        break;
                    case DoingWith.Enemy:
                        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("doingWithEnemy"));
                        switch (eventSO.doingWithEnemy)
                        {
                            case DoingWithEnemy.Spawn:
                                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("SpawnPoint"));
                                break;
                            case DoingWithEnemy.MoveTo:
                                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("EnemyWayPoint"));
                                break;
                            default:
                                break;
                        }
                        break;
                    case DoingWith.Custom:
                        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("CustomCode"));
                        break;
                    default:
                        break;
                }
            }

            if (index >= -1 && index < eventNode.eventSO.Count)
            {
                eventNode.CurrentEditingSONum = index;
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

