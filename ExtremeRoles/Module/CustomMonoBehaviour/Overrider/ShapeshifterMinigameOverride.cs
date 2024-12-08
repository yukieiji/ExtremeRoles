using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Overrider;

[Il2CppRegister]
public sealed class ShapeshifterMinigameShapeshiftOverride : MonoBehaviour
{
	private Action<PlayerControl>? destroyAction;

	public void Add(Action<PlayerControl> @delegate)
	{
		if (this.destroyAction is null)
		{
			this.destroyAction = @delegate;
		}
		else
		{
			this.destroyAction += @delegate;
		}
	}

	public void OverrideShapeshift(
		Minigame game,
		PlayerControl target)
	{
		if (PlayerControl.LocalPlayer.inVent)
		{
			game.Close();
			return;
		}
		if (target != null)
		{
			this.destroyAction?.Invoke(target);
		}
		else
		{
			Logger.GlobalInstance.Warning("Shapeshift: target is null", null);
		}
		game.Close();
	}
}
