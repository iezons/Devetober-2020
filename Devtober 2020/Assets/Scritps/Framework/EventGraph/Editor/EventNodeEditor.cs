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
            int index = EditorGUILayout.IntField(eventNode.CurrentEditingSONum + 1) - 1;
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
                eventNode.eventSO.RemoveAt(index);
                index--;
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

            if (index >= 0 && index < eventNode.eventSO.Count)
            {
                eventNode.CurrentEditingSONum = index;
            }

            Debug.Log(index);

            if (eventNode.eventSO.Count > eventNode.CurrentEditingSONum)
            {
                EventSO eventSO = eventNode.eventSO[index];
                
                if(eventSO.ID == string.Empty)
                {
                    eventSO.ID = Guid.NewGuid().ToString();
                }
                EditorGUILayout.TextField("GUID: ", eventSO.ID);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("doingWith"));
                switch (eventSO.doingWith)
                {
                    case DoingWith.NPC:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("doingWithNPC"));
                        switch (eventSO.doingWithNPC)
                        {
                            case DoingWithNPC.Talking:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("NPCTalking"));
                                break;
                            case DoingWithNPC.MoveTo:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("NPCWayPoint"));
                                break;
                            case DoingWithNPC.Patrol:
                                break;
                            default:
                                break;
                        }
                        break;
                    case DoingWith.Room:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("doingWithRoom"));
                        break;
                    case DoingWith.Enemy:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("doingWithEnemy"));
                        switch (eventSO.doingWithEnemy)
                        {
                            case DoingWithEnemy.Spawn:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("SpawnPoint"));
                                break;
                            case DoingWithEnemy.MoveTo:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("EnemyWayPoint"));
                                break;
                            default:
                                break;
                        }
                        break;
                    case DoingWith.Custom:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(index).FindPropertyRelative("CustomCode"));
                        break;
                    default:
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

