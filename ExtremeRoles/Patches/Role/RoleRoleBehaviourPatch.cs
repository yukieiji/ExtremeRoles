using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface.Ability;
using AmongUs.GameOptions;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Core.Abstract;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Patches.Role;

public class RoleBehaviourGetAbilityDistancePatchBody(IGameProgress progress, IGameRuntime runtime)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;

	public bool Prefix(
		RoleBehaviour __instance,
		ref float __result)
	{
		if (!(
				_runtime.TryGetGameContext(out var ctx) &&
				_progress.IsGameNow &&
				ctx.Roles.TryGetRole(__instance.Player.PlayerId, out var role) &&
				role.CanKill() &&
				role.TryGetKillRange(out int range) &&
				GameSystem.TryGetKillDistance(out var killRange) &&
				killRange.Length > range
			))
		{
			return true;
		}
		__result = killRange[range];
		return false;
	}
}

public class RoleBehaviourIsValidTargetPatchBody(IGameProgress progress, IGameRuntime runtime)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;

	public bool Prefix(
		RoleBehaviour __instance,
		ref bool __result,
		NetworkedPlayerInfo target)
	{
		if (!(_runtime.TryGetGameContext(out var ctx) && _progress.IsGameNow))
		{
			return true;
		}
		byte instancePlayerId = __instance.Player.PlayerId;
		
		if (!(
				ctx.Roles.TryGetRole(instancePlayerId, out var role) &&
				role.CanKill()
			))
		{
			return true;
		}

		byte targetPlayerId = target.PlayerId;
		__result =
			target.IsAlive() &&
			targetPlayerId != instancePlayerId &&
			target.Role != null &&
			(
				!target.Object.inVent ||
				ctx.GlobalOption.Vent.CanKillVentInPlayer
			) &&
			!target.Object.inMovingPlat &&
			ctx.Roles.TryGetRole(targetPlayerId, out var targetRole) &&
			!role.IsSameTeam(targetRole) &&
			(targetRole.AbilityClass is not IInvincible invincible || invincible.IsValidKillFromSource(instancePlayerId));
		return false;
	}

	public void Postfix(
		RoleBehaviour __instance,
		ref bool __result,
		NetworkedPlayerInfo target)
	{
		if (!(
				_runtime.TryGetGameContext(out var ctx) &&
				_progress.IsGameNow &&
				__result &&
				(
					__instance.Role == RoleTypes.Detective ||
					__instance.Role == RoleTypes.Tracker
				) &&
				ctx.Roles.TryGetRole(target.PlayerId, out var targetRole) &&
				targetRole.AbilityClass is IInvincible invincible
			))
		{
			return;
		}
		// 探偵とトラッカーの能力の対象からモニカやリーダーを外す処理
		__result &= invincible.IsValidAbilitySource(__instance.Player.PlayerId);
	}
}


[HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.GetAbilityDistance))]
public static class RoleBehaviourGetAbilityDistancePatch
{
	private static RoleBehaviourGetAbilityDistancePatchBody? _body;

	public static bool Prefix(
        RoleBehaviour __instance,
        ref float __result)
    {
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<RoleBehaviourGetAbilityDistancePatchBody>();
		}
		return _body.Prefix(__instance, ref __result);
	}
}

[HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.IsValidTarget))]
public static class RoleBehaviourIsValidTargetPatch
{
	private static RoleBehaviourIsValidTargetPatchBody? _body;

	public static bool Prefix(
        RoleBehaviour __instance,
        ref bool __result,
        [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<RoleBehaviourIsValidTargetPatchBody>();
		}
		return _body.Prefix(__instance, ref __result, target);
	}

	public static void Postfix(
		RoleBehaviour __instance,
		ref bool __result,
		[HarmonyArgument(0)] NetworkedPlayerInfo target)
	{
		_body?.Postfix(__instance, ref __result, target);
	}
}
