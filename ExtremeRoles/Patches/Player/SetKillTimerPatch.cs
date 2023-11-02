using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControlSetKillTimernPatch
{
	public static bool Prefix(
		PlayerControl __instance, [HarmonyArgument(0)] float time)
	{
		var roles = ExtremeRoleManager.GameRole;
		if (roles.Count == 0 || !roles.ContainsKey(__instance.PlayerId)) { return true; }

		float killCool = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
			FloatOptionNames.KillCooldown);
		var role = roles[__instance.PlayerId];

		if (killCool <= 0f || !role.CanKill()) { return false; }

		float maxTime = role.TryGetKillCool(out float otherKillCool) ? otherKillCool : killCool;

		__instance.killTimer = Mathf.Clamp(
			time, 0f, maxTime);
		FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(
			__instance.killTimer, maxTime);

		return false;

	}
}
