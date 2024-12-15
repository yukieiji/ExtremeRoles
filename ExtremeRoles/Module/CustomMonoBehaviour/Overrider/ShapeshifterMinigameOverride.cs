using System;

using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Overrider;

[Il2CppRegister]
public sealed class ShapeshifterMinigameShapeshiftOverride : MonoBehaviour
{
	private Action<PlayerControl>? destroyAction;

	[HideFromIl2Cpp]
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
