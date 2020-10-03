using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EvtGraph
{
    [CustomEditor(typeof(EventSO))]
    public class EventSOEditor : Editor
    {
        EventSO eventSO;

        private void OnEnable()
        {
            eventSO = target as EventSO;
        }

        public override void OnInspectorGUI()
        {
            eventSO = target as EventSO;
            serializedObject.Update();
            //EditorGUILayout.LabelField("GUID: " + eventSO.ID);
            EditorGUILayout.TextField("GUID: ", eventSO.ID);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("doingWith"));
            if(eventSO.doingWith == DoingWith.NPC)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("doingWithNPC"));
                if(eventSO.doingWithNPC == DoingWithNPC.Talking)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("NPCTalking"));
                }
                else if (eventSO.doingWithNPC == DoingWithNPC.MoveTo)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("NPCWayPoint"));
                }
            }
            else if (eventSO.doingWith == DoingWith.Enemy)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("doingWithEnemy"));
                if (eventSO.doingWithEnemy == DoingWithEnemy.Spawn)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SpawnPoint"));
                }
                else if (eventSO.doingWithEnemy == DoingWithEnemy.MoveTo)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("EnemyWayPoint"));
                }
            }
            else if (eventSO.doingWith == DoingWith.Room)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("doingWithRoom"));
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

