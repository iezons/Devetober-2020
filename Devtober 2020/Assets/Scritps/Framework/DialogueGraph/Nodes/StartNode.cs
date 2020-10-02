using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;

namespace DialogueGraph
{
	[CreateNodeMenu("Start", order = 2)]
	[NodeWidth(120)]
	[NodeTint("#6495ED")]//矢车菊的蓝色
	public class StartNode : Node
	{
		[Output] public Empty Output;
		public string Language;


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
	}
}