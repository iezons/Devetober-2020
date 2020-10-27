using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using System.Security.Permissions;

namespace DiaGraph
{
	[CreateNodeMenu("Start", order = 3)]
	[NodeTint("#6495ED")]//矢车菊的蓝色
	public class StartNode : Node
	{
        private DialogueGraph dialogueGraph;
        [Output(connectionType = ConnectionType.Override)] public Empty Output;

		public string Language = "English";

		// Use this for initialization
		protected override void Init()
		{
            name = "Start";
            base.Init();
            if (dialogueGraph == null) dialogueGraph = graph as DialogueGraph;
            dialogueGraph.current = this;
        }

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}

		public Node MoveNext()
        {
            NodePort exitPort = GetOutputPort("Output");

            if (!exitPort.IsConnected)
            {
                FinishDia();
                Debug.LogError("Start Node isn't connected");
                Debug.Break();
                return this;
            }

            Node node = exitPort.Connection.node;
            DialogueNode dia = node as DialogueNode;
            if (dia != null)
            {
                return dia;
            }

            OptionNode opt = node as OptionNode;
            if (opt != null)
            {
                return opt;
            }

            WaitingNode wat = node as WaitingNode;
            if(wat != null)
            {
                return wat;
            }
            FinishDia();
            Debug.LogWarning("Start Node isn't connected");
            return this;
        }

        void FinishDia()
        {
            DialogueGraph diaGraph = graph as DialogueGraph;
            if (diaGraph != null)
            {
                diaGraph.DiaPlay.Finished();
            }
        }
    }
}