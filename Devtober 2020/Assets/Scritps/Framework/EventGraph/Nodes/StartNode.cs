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
		[Output(connectionType = ConnectionType.Multiple)] public Empty Output;

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

		public List<Node> MoveNext()
        {
            NodePort exitPort = GetOutputPort("Output");

            if (!exitPort.IsConnected)
            {
                EventCenter.GetInstance().EventTriggered("DialoguePlay.Finished");
                Debug.LogError("Start Node isn't connected");
                Debug.Break();
                return new List<Node> { this };
            }

            List<NodePort> ports = exitPort.GetConnections();
            List<Node> evts = new List<Node>();
            for (int i = 0; i < ports.Count; i++)
            {
                EventNode evt = ports[i].node as EventNode;
                if (evt != null)
                {
                    evts.Add(evt);
                }
            }
            if(evts.Count > 0)
            {
                return evts;
            }

            EventCenter.GetInstance().EventTriggered("DialoguePlay.Finished");
            Debug.LogWarning("Start Node isn't connected to a legal node");
            return new List<Node> { this };
        }
	}
}
