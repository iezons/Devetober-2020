using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using System;

namespace DiaGraph
{
	[CreateAssetMenu(menuName = "Graph/Dialogue Graph")]
	public class DialogueGraph : NodeGraph
	{
		public DialogueNode dialogueNode;
		public OptionNode optionNode;
		public StartNode startNode;
        public WaitingNode waitingNode;

		public Node current;

        public void Next(int index = 0)
        {
            //MoveNext
            OptionNode opt = current as OptionNode;
            DialogueNode dia = current as DialogueNode;
            StartNode sta = current as StartNode;
            WaitingNode wai = current as WaitingNode;
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
            else if (wai != null)
            {
                current = wai.MoveNext();
            }

            //If the Next One is Waiting Node, init it.
            WaitingNode Wait = current as WaitingNode;
            if (Wait != null)
            {
                Wait.StartWaiting();
            }
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