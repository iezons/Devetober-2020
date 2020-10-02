﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBase;
using System;
using System.Text.RegularExpressions;

namespace DialogueGraph
{
	[CreateNodeMenu("Option", order = 1)]
	[NodeWidth(300)]
	[NodeTint("#F4A460")]//沙棕色
	public class OptionNode : Node
	{
		[Input] public Empty Input;
		[Output(dynamicPortList = true)] public List<OptionClass> Option;

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

		public List<OptionClass> GetOptions()
		{
			return Option;
		}

		public Node MoveNext(int index)
        {
			Node temp = this;
			foreach (var port in DynamicOutputs)
			{
				if (port.fieldName == "Option " + index.ToString())
				{
					if (!port.IsConnected) return temp;
					temp = port.Connection.node;
				}
			}
			if (temp == this)
			{
				EventCenter.GetInstance().EventTriggered("PlayText.TalkingFinished");
				return temp;
			}
			else
			{
				return temp;
			}
		}

		public string GetBriefOption()
		{
			string temp = string.Empty;
			if (Option != null)
			{
				if (Option.Count >= 1)
				{
					temp = Option[0].Text;
					if (temp.IndexOf('\n') > 0)
						temp = Regex.Match(Option[0].Text, @".+(?=\n)").Value;
					if (temp.Length >= 15)
					{
						temp = temp.Substring(0, 14) + "…";
					}
				}
			}
			name = temp;
			return temp;
		}
	}

	[Serializable]
	public class OptionClass
	{
		//public string Condition;
		[TextArea]
		public string Text;
	}

}