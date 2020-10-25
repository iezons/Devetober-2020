using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using System;
using System.Text.RegularExpressions;

namespace EvtGraph
{
	[NodeWidth(350)]
	[CreateNodeMenu("Event", order = 0)]
	[NodeTint("#00CED1")]//深绿宝石
	public class EventNode : Node
	{
		[Input] public Empty Input;
		[Output(connectionType = ConnectionType.Multiple)] public Empty Output;

		//[HideInInspector]
		public string GUID = Guid.NewGuid().ToString();
		public string EventName = string.Empty;

		public List<ConditionSO> conditionSOs = new List<ConditionSO>();
		public int CurrentEditingConNum = -1;
		public List<EventSO> eventSO = new List<EventSO>();
		public int CurrentEditingSONum = -1;
		[TextArea(5, 5)]
		public string Comment;

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
				EventCenter.GetInstance().EventTriggered("Event.Finished");
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
			if (evts.Count > 0)
			{
				return evts;
			}

			EventCenter.GetInstance().EventTriggered("Event.Finished");
			return new List<Node> { this };
		}

		public string GetBriefDialog()
        {
			string temp = string.Empty;
			if (EventName != string.Empty)
			{
				temp = EventName;
				if (temp.IndexOf('\n') > 0)
					temp = Regex.Match(EventName, @".+(?=\n)").Value;
				if (temp.Length >= 15)
				{
					temp = temp.Substring(0, 14) + "…";
				}
				temp = "Event: " + temp;
			}
			else
			{
				temp = "Basic Event Node";
			}
			return temp;
		}
	}

	[Serializable]
	public class Empty { }
}