using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using System;

namespace DialogueGraph
{
	[CreateAssetMenu(menuName = "Graph/Dialogue Graph")]
	public class DialogueGraph : NodeGraph
	{
		public DialogueNode dialogueNode;
		public OptionNode optionNode;
		public StartNode startNode;

		public Node currenetNode;

        public void SetStartPoint(string lan)
        {
            List<Node> temp = nodes.FindAll(node =>
            {
                return node.GetType() == typeof(StartNode);
            });
            for (int i = 0; i < temp.Count; i++)
            {
                StartNode tStart = temp[i] as StartNode;
                if (tStart != null)
                {
                    if (tStart.Language == lan)
                    {
                        currenetNode = tStart;
                    }
                }
            }
            if (currenetNode == null)
            {
                Debug.LogError("There is not a starting point for current language");
                Debug.Break();
            }
        }
    }

	[Serializable]
	public class Empty
	{

	}
}