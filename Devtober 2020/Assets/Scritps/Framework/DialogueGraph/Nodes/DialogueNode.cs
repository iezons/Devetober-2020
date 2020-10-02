using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;

namespace DialogueGraph
{
	[NodeWidth(320)]
	[CreateNodeMenu("Dialogue", order = 0)]
	[NodeTint("#00CED1")]//深绿宝石
	public class DialogueNode : Node
	{
        [Input] public Empty Input;
        [Output] public Empty Output;
        public string TalkingPerson;
        public float Width;

        [TextArea(5, 5)]
        public List<string> Dialogue = new List<string>();

        //public bool IsMax = true;
        public int curIndex = 0;

        // Use this for initialization
        protected override void Init()
        {
            base.Init();
            curIndex = 0;
        }

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}
	}
}
