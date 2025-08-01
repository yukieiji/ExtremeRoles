using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;

using CommomSystem = ExtremeRoles.Roles.API.Systems.Common;

#nullable enable

namespace ExtremeRoles.Patches.MiniGame;

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
public static class HauntMenuMinigameFilterTextPatch
{
    public static bool Prefix(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget == null ||
			ExtremeRoleManager.GameRole.Count == 0 ||
			!ExtremeRoleManager.TryGetRole(__instance.HauntTarget.PlayerId, out var targetRole))
		{
			return true;
		}

        var role = ExtremeRoleManager.GetLocalPlayerRole();
        var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

        bool isBlocked = role.IsBlockShowPlayingRoleInfo();
        if (ghostRole != null)
        {
            isBlocked = true;
        }

        __instance.FilterText.text =
            isBlocked ||
            (
                ExtremeRolesPlugin.ShipState.IsAssassinAssign &&
			CommomSystem.IsForceInfoBlockRole(role)
		) ?
                "？？？" :
                Tr.GetString(targetRole.Core.Team.ToString());

        return false;
    }
}

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.FixedUpdate))]
public static class HauntMenuMinigameFixedUpdatePatch
{
	public static bool Prefix(HauntMenuMinigame __instance)
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null ||
			local.MyPhysics == null ||
			__instance.amClosing != Minigame.CloseState.None)
		{
			return false;
		}

		var vector = Vector2.zero;
		var physics = local.MyPhysics;

		if (__instance.HauntTarget != null)
		{
			Vector2 vector2 = __instance.HauntTarget.GetTruePosition() + __instance.Offset;
			Vector2 truePosition = local.GetTruePosition();
			Vector2 vector3 = physics.GetVelocity() / physics.TrueSpeed;
			Vector2 vector4 = vector2 - truePosition;
			float magnitude = vector4.magnitude;
			if (magnitude > 0.05f)
			{
				vector4 = vector4.normalized * Mathf.Clamp(
					magnitude, 0.75f,
					ExtremeGameModeManager.Instance.ShipOption.GhostRole.HauntMinigameMaxSpeed);
				vector = (vector3 * 0.8f) + (vector4 * 0.2f);
			}
			else
			{
				vector *= 0.7f;
			}
		}
		physics.SetNormalizedVelocity(vector);
		return false;
	}
}
