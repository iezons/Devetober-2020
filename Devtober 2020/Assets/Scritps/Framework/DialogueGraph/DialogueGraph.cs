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

		public Node current;

        public void Continue(int index = 0)
        {
            OptionNode opt = current as OptionNode;
            DialogueNode dia = current as DialogueNode;
            StartNode sta = current as StartNode;
            if (opt != null)
            {
                current = opt.MoveNext(index);
            }
            else if (dia != null)
            {
                current = dia.MoveNext();
            }
            else if (sta != null)
            {
                current = sta.MoveNext();
            }
            //EventNode evt = current as EventNode;
            //while (evt != null)
            //{
            //    current = evt.MoveNext();
            //    evt = current as EventNode;
            //}
        }

        public void SetStartPoint(string lan = "English")
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
                        current = tStart;
                    }
                }
            }
            if (current == null)
            {
                Debug.LogError("There is not a starting point for current language");
                Debug.Break();
            }
        }

        public void Open()
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
                    if (tStart.Language == "English")
                    {
                        current = tStart;
                    }
                }
            }
        }
    }

	[Serializable]
	public class Empty { }
}