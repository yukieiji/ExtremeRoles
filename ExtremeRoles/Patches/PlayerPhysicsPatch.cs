using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface.Status;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Patches;

public class PlayerPhysicsFixedUpdatePatchBody(IExtremeSystemTypeManager system, IGameProgress progress, IGameRuntime runtime)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;
	private readonly IExtremeSystemTypeManager _system = system;

	private bool isTimeBreakNow =>
		_system.TryGet<TimeBreakerTimeBreakSystem>(ExtremeSystemType.TimeBreakerTimeBreakSystem, out var system) &&
		system.Active;

	private bool isCanMove(IGameContext ctx, PlayerPhysics __instance)
		=> !__instance.AmOwner ||
			ctx.Roles.GetLocalRoleCastedStatusFlag<IStatusMovable>(x => x.CanMove);

	public bool Prefix(PlayerPhysics __instance)
	{
		if (!(_progress.IsTaskPhase && _runtime.TryGetGameContext(out var ctx)))
		{
			return true;
		}
		if (isTimeBreakNow || !isCanMove(ctx, __instance))
		{
			setVelocityToZero(__instance);
			return false;
		}
		setVelocityToZero(__instance);
		return true;
	}

	public void Postfix(PlayerPhysics __instance)
	{
		if (_progress.IsTaskPhase &&
			__instance.AmOwner &&
			__instance.myPlayer.CanMove &&
			GameData.Instance != null &&
			_runtime.TryGetGameContext(out var ctx) &&
			ctx.Roles.GetLocalPlayerRole().TryGetVelocity(out float velocity))
		{
			__instance.body.velocity *= velocity;
		}
	}

	private static void setVelocityToZero(PlayerPhysics body)
	{
		body.body.velocity = Vector2.zero;
	}
}


[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdatePatch
{
	private static PlayerPhysicsFixedUpdatePatchBody? _body;

	public static bool Prefix(PlayerPhysics __instance)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<PlayerPhysicsFixedUpdatePatchBody>();
		}
		return _body.Prefix(__instance);
	}

    public static void Postfix(PlayerPhysics __instance)
    {
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<PlayerPhysicsFixedUpdatePatchBody>();
		}
		_body.Postfix(__instance);
	}
	private static void setVelocityToZero(PlayerPhysics body)
	{
		body.body.velocity = Vector2.zero;
	}
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
public static class PlayerPhysicsFHandleAnimationPatch
{
	public static bool Prefix(PlayerPhysics __instance)
	{
		return
			__instance.Animations != null &&
			__instance.body != null &&
			__instance.myPlayer != null;
	}
}
