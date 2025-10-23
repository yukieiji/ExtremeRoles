using HarmonyLib;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdatePatch
{
	private static bool isTimeBreakNow =>
		ExtremeSystemTypeManager.Instance.TryGet<TimeBreakerTimeBreakSystem>(
			ExtremeSystemType.TimeBreakerTimeBreakSystem, out var system) &&
		system.Active;
	private static bool isCanMove(PlayerPhysics __instance)
		=> !__instance.AmOwner ||
			ExtremeRoleManager.GetLocalRoleCastedStatusFlag<IStatusMovable>(x => x.CanMove);
		

	public static bool Prefix(PlayerPhysics __instance)
	{
		if (!GameProgressSystem.IsTaskPhase)
		{
			return true;
		}

		if (isTimeBreakNow || !isCanMove(__instance))
		{
			setVelocityToZero(__instance);
			return false;
		}

		return true;
	}

    public static void Postfix(PlayerPhysics __instance)
    {
        if (GameProgressSystem.IsTaskPhase &&
			__instance.AmOwner &&
            __instance.myPlayer.CanMove &&
            GameData.Instance &&
            ExtremeRoleManager.GetLocalPlayerRole().TryGetVelocity(out float velocity))
        {
            __instance.body.velocity *= velocity;
        }
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
