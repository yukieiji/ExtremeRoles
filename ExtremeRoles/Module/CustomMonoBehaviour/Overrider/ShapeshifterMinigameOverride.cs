using System;

using UnityEngine;

using Il2CppInterop.Runtime.Attributes;
using ExtremeRoles.Module.CustomMonoBehaviour.WithAction;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Overrider;

[Il2CppRegister]
public sealed class ShapeshifterMinigameShapeshiftOverride(IntPtr ptr) : MonoBehaviour(ptr)
{
	private Action<PlayerControl>? overrideAction;
	private OnDestroyBehavior? destroyBehavior;

	[HideFromIl2Cpp]
	public Func<PlayerControl, bool>? Filter { get; set; }

	public void Awake()
	{
		this.destroyBehavior = this.gameObject.AddComponent<OnDestroyBehavior>();
	}

	[HideFromIl2Cpp]
	public void AddSelectPlayerAction(Action<PlayerControl> @delegate)
	{
		if (this.overrideAction is null)
		{
			this.overrideAction = @delegate;
		}
		else
		{
			this.overrideAction += @delegate;
		}
	}

	[HideFromIl2Cpp]
	public void AddCloseAction(Action act)
	{
		if (this.destroyBehavior != null)
		{
			this.destroyBehavior.Add(act);
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
			if (this.Filter != null && !this.Filter(target))
			{
				return;
			}
			this.overrideAction?.Invoke(target);
		}
		else
		{
			Logger.GlobalInstance.Warning("Shapeshift: target is null", null);
		}
		game.Close();
	}
}
