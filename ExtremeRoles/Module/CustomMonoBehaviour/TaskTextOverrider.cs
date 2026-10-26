using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public class TaskTextOverrider(IntPtr ptr) : MonoBehaviour(ptr)
{
	public TaskTypes Type { get; set; }
	public SystemTypes Room { private get; set; }
	public NormalPlayerTask? Task { private get; set; }

	public void AppendText(Il2CppSystem.Text.StringBuilder sb)
	{
		bool flag = Task != null && this.Task.ShouldYellowText();
		if (flag)
		{
			if (Task!.IsComplete)
			{
				sb.Append("<color=#00DD00FF>");
			}
			else
			{
				sb.Append("<color=#FFFF00FF>");
			}
		}
		var trans = TranslationController.Instance;
		sb.Append(trans.GetString(this.Room));
		sb.Append(": ");
		sb.Append(trans.GetString(this.Type));
		if (flag)
		{
			sb.Append("</color>");
		}
		sb.AppendLine();
	}
}
