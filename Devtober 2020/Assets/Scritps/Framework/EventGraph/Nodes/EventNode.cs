using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using System;

namespace EvtGraph
{
	[NodeWidth(280)]
	[CreateNodeMenu("Event", order = 0)]
	[NodeTint("#00CED1")]//深绿宝石
	public class EventNode : Node
	{
		[Input] public Empty Input;
		[Output] public Empty Output;

		[HideInInspector]
		public string GUID = Guid.NewGuid().ToString();
		public List<EventSO> eventSO;
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

		public Node MoveNext()
		{
			NodePort exitPort = GetOutputPort("Output");

			if (!exitPort.IsConnected)
			{
				EventCenter.GetInstance().EventTriggered("Dialogue.Finished");
				return this;
			}

			Node node = exitPort.Connection.node;
			EventNode evt = node as EventNode;
			if (evt != null)
			{
				return evt;
			}

			EventCenter.GetInstance().EventTriggered("Dialogue.Finished");
			return this;
		}

		public void EventDistribution()
        {
            for (int i = 0; i < eventSO.Count; i++)
            {
				EventSO evt = eventSO[i];
				if(evt != null)
                {
                    switch (evt.doingWith)
                    {
                        case DoingWith.NPC:
                            switch (evt.doingWithNPC)
                            {
                                case DoingWithNPC.Talking:
                                    for (int a = 0; a < evt.NPCTalking.Count; a++)
                                    {
										Transform traA = evt.NPCTalking[a].MoveToClassA.Object;
										Transform traB = evt.NPCTalking[a].MoveToClassB.Object;
										traA.GetComponent<NpcController>().npc_so.toDoList.Add(evt);
										traB.GetComponent<NpcController>().npc_so.toDoList.Add(evt);
									}
                                    break;
                                case DoingWithNPC.MoveTo:
                                    for (int a = 0; a < evt.NPCWayPoint.Count; a++)
                                    {
										Transform tra = evt.NPCWayPoint[a].Object;
										tra.GetComponent<NpcController>().npc_so.toDoList.Add(evt);
                                    }
                                    break;
                                case DoingWithNPC.Patrol:
									//TODO
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case DoingWith.Room:
							//Nothing
                            break;
                        case DoingWith.Enemy:
							//TODO
                            break;
                        default:
                            break;
                    }
                }
            }
        }
	}

	[Serializable]
	public class Empty { }
}