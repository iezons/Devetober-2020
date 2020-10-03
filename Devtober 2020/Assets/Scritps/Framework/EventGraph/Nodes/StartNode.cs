using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;

namespace EvtGraph
{
	[NodeWidth(120)]
	[CreateNodeMenu("Start", order = 1)]
	[NodeTint("#6495ED")]//矢车菊的蓝色
	public class StartNode : Node
	{
		[Output] public Empty Output;

		// Use this for initialization
		protected override void Init()
		{
			base.Init();

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
                EventCenter.GetInstance().EventTriggered("Dialogue.Finished");
                Debug.LogError("Start Node isn't connected");
                Debug.Break();
                return this;
            }

            Node node = exitPort.Connection.node;
            EventNode evt = node as EventNode;
            if (evt != null)
            {
                return evt;
            }

            EventCenter.GetInstance().EventTriggered("Dialogue.Finished");
            Debug.LogWarning("Start Node isn't connected to a legal node");
            return this;
        }
	}
}
