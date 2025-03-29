using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.RoleAssign;

using PlayerHelper = ExtremeRoles.Helper.Player;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControlSetKillTimernPatch
{
	public static bool Prefix(
		PlayerControl __instance, [HarmonyArgument(0)] float time)
	{
		if (!(
				RoleAssignState.Instance.IsRoleSetUpEnd &&
				ExtremeRoleManager.TryGetRole(__instance.PlayerId, out var role)
			))
		{
			return true;
		}

		float killCool = PlayerHelper.DefaultKillCoolTime;
		if (killCool <= 0f || !role.CanKill())
		{
			return false;
		}

		float maxTime = role.TryGetKillCool(out float otherKillCool) ? otherKillCool : killCool;

		__instance.killTimer = Mathf.Clamp(
			time, 0f, maxTime);
		HudManager.Instance.KillButton.SetCoolDown(
			__instance.killTimer, maxTime);

		return false;

	}
}
