using HarmonyLib;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

#nullable enable

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(PlayerPhysics), "TrueSpeed", MethodType.Getter)]
public static class PlayerPhysicsTrueSpeedPatch
{
    // もしもっと高速で動くやつを実装する場合ここを変える
    // 正直9倍速でもカメラ追いつかねぇ・・・
    // 2022/08/14:Xionは最大20倍速で動けるのでとりあえず100を突っ込んどく
    private const float maxModSpeed = 100.0f;

    private static float playerBaseSpeed => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
        FloatOptionNames.PlayerSpeedMod);

    public static bool Prefix(
        PlayerPhysics __instance,
        ref float __result)
    {
        // オバロとかでも以下が最大速度なのでそれを返す
        // 最大速度 = 基本速度 * PlayerControl.GameOptions.PlayerSpeedMod * 3.0f;
        __result = __instance.Speed * maxModSpeed * playerBaseSpeed;
        return false;
    }
}


[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdatePatch
{
	public static bool Prefix(PlayerPhysics __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		if (ExtremeSystemTypeManager.Instance.TryGet<TimeBreakerTimeBreakSystem>(
				ExtremeSystemType.TimeBreakerTimeBreakSystem, out var system) &&
			system.Active)
		{
			setVelocityToZero(__instance);
			return false;
		}

	 	var (main, sub) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleMovable>();

		if (!((main is null || main.CanMove) && (sub is null || sub.CanMove)) &&
			__instance.AmOwner)
		{
			setVelocityToZero(__instance);
			return false;
		}

		return true;
	}

    public static void Postfix(PlayerPhysics __instance)
    {
        if (RoleAssignState.Instance.IsRoleSetUpEnd &&
			ExtremeRoleManager.GameRole.Count != 0 &&
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
