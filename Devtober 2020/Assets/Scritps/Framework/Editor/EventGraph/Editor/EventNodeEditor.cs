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
        int evt_index;
        int con_index;
        public override void OnHeaderGUI()
        {
            GUI.color = Color.white;
            if (eventGraph == null) eventNode = target as EventNode;
            if (eventGraph == null) eventGraph = window.graph as EventGraph;
            for (int i = 0; i < eventGraph.currentList.Count; i++)
            {
                if(eventGraph.currentList[i] == eventNode)
                {
                    GUI.color = Color.blue;
                    break;
                }
            }
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


            //Condition SO
            //--Number Display
            GUILayout.BeginHorizontal();
            con_index = EditorGUILayout.IntField(eventNode.CurrentEditingConNum + 1) - 1;
            EditorGUILayout.LabelField("/" + eventNode.conditionSOs.Count.ToString());
            GUILayout.EndHorizontal();
            //--Add Button
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                eventNode.conditionSOs.Insert(con_index + 1, new ConditionSO());
                con_index++;
            }
            //--Remove Button
            if (GUILayout.Button("Remove"))
            {
                if (con_index >= 0 && con_index < eventNode.conditionSOs.Count)
                {
                    eventNode.conditionSOs.RemoveAt(con_index);
                    if (eventNode.conditionSOs.Count > 0)
                    {
                        if (con_index - 1 < 0)
                        {
                            con_index = 0;
                        }
                        else
                        {
                            con_index--;
                        }
                    }
                    else
                    {
                        con_index = -1;
                    }
                }
            }
            GUILayout.EndHorizontal();
            //--Previous Next Button
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous") && con_index - 1 >= 0)
            {
                con_index--;
            }

            if (GUILayout.Button("Next") && con_index + 1 <= eventNode.conditionSOs.Count - 1)
            {
                con_index++;
            }
            GUILayout.EndHorizontal();

            //--Separater
            EditorGUILayout.Space(5);

            Rect rectC = EditorGUILayout.GetControlRect(false, 1);

            rectC.height = 1;

            EditorGUI.DrawRect(rectC, new Color(0.5f, 0.5f, 0.5f, 1));

            EditorGUILayout.Space(5);

            //--Actual Drawing
            if (eventNode.conditionSOs.Count > eventNode.CurrentEditingConNum && eventNode.CurrentEditingConNum >= 0)
            {
                ConditionSO condition = eventNode.conditionSOs[eventNode.CurrentEditingConNum];
                if (condition.ID == string.Empty)
                    condition.ID = Guid.NewGuid().ToString();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("conditionWith"));
                switch (condition.conditionWith)
                {
                    case ConditionSO.ConditionWith.NPC:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("NPC"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("nPCConditinoWith"));
                        switch (condition.nPCConditinoWith)
                        {
                            case ConditionSO.NPCConditionWith.HP:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("nPCHPWith"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("HP"));
                                break;
                            default:
                                break;
                        }
                        break;
                    case ConditionSO.ConditionWith.Room:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("roomConditionWith"));
                        switch (condition.roomConditionWith)
                        {
                            case ConditionSO.RoomConditionWith.Number_of_NPCs:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("roomTrackers"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("Room_NPC_Num"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("NPC_Number"));
                                break;
                            case ConditionSO.RoomConditionWith.Specific_NPCs:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("SpecificRoomNPCs"));
                                break;
                            case ConditionSO.RoomConditionWith.Number_of_Enemys:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("roomTrackers"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("Room_Enemy_Num"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("Enemy_Number"));
                                break;
                            default:
                                break;
                        }
                        break;
                    case ConditionSO.ConditionWith.Enemy:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("enemyConditionWith"));
                        switch (condition.enemyConditionWith)
                        {
                            case ConditionSO.EnemyConditionWith.Overall_Numbers:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("Enemy_Num"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("OverallNumbers"));
                                break;
                            default:
                                break;
                        }
                        break;
                    case ConditionSO.ConditionWith.Event:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("eventTriggers"));
                        break;
                    case ConditionSO.ConditionWith.Custom:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionSOs").GetArrayElementAtIndex(eventNode.CurrentEditingConNum).FindPropertyRelative("customConditions"));
                        break;
                    default:
                        break;
                }
            }

            //--Separater
            EditorGUILayout.Space(5);

            Rect rectB = EditorGUILayout.GetControlRect(false, 1);

            rectB.height = 1;

            EditorGUI.DrawRect(rectB, new Color(0.2f, 0.2f, 0.2f, 1));

            EditorGUILayout.Space(5);

            //--Event SO
            //--Number Display
            GUILayout.BeginHorizontal();
            evt_index = EditorGUILayout.IntField(eventNode.CurrentEditingSONum + 1) - 1;
            EditorGUILayout.LabelField("/" + eventNode.eventSO.Count.ToString());
            GUILayout.EndHorizontal();
            //--Add Button
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                eventNode.eventSO.Insert(evt_index + 1, new EventSO());
                evt_index++;
            }
            //--Remove Button
            if (GUILayout.Button("Remove"))
            {
                if(evt_index >= 0 && evt_index < eventNode.eventSO.Count)
                {
                    eventNode.eventSO.RemoveAt(evt_index);
                    if (eventNode.eventSO.Count > 0)
                    {
                        if (evt_index - 1 < 0)
                        {
                            evt_index = 0;
                        }
                        else
                        {
                            evt_index--;
                        }
                    }
                    else
                    {
                        evt_index = -1;
                    }
                }
            }
            GUILayout.EndHorizontal();
            //--Previous Next Button
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Previous") && evt_index - 1 >= 0)
            {
                evt_index--;
            }

            if (GUILayout.Button("Next") && evt_index + 1 <= eventNode.eventSO.Count - 1)
            {
                evt_index++;
            }
            GUILayout.EndHorizontal();

            //--Separater
            EditorGUILayout.Space(5);

            Rect rect = EditorGUILayout.GetControlRect(false, 1);

            rect.height = 1;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

            EditorGUILayout.Space(5);

            //--Actual Drawing
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
                    case DoingWith.Timeline:
                        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("eventSO").GetArrayElementAtIndex(eventNode.CurrentEditingSONum).FindPropertyRelative("timelines"));
                        break;
                    default:
                        break;
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Comment"));

            if (evt_index >= -1 && evt_index < eventNode.eventSO.Count)
            {
                eventNode.CurrentEditingSONum = evt_index;
            }

            if (con_index >= -1 && con_index < eventNode.conditionSOs.Count)
            {
                eventNode.CurrentEditingConNum = con_index;
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

