using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Task;
using ExtremeRoles.Module.SystemType.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

using SaboTask = ExtremeRoles.Module.SystemType.Roles.TeroristTeroSabotageSystem.TeroSabotageTask;

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

[Il2CppRegister]
public sealed class TeroristTeroSabotageMinigame : Minigame
{
	public byte TargetBombId { private get; set; } = byte.MaxValue;

	private SaboTask? task;

	public override void Begin(PlayerTask task)
	{
		this.AbstractBegin(task);

		if (!task.IsTryCast<ExtremePlayerTask>(out var playerTask) ||
			playerTask!.Behavior is not SaboTask saboTask)
		{
			throw new ArgumentException("invalided Task");
		}
		this.task = saboTask;
	}
}
