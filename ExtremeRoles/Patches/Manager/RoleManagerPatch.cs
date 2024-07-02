using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Host;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class RoleManagerAssignSelectRolesPatch
{
	public static void Prefix()
	{
		if (!ExtremeGameModeManager.Instance.EnableXion) { return; }

		PlayerControl loaclPlayer = PlayerControl.LocalPlayer;

		loaclPlayer.RpcSetRole(RoleTypes.Crewmate);
		loaclPlayer.Data.IsDead = true;
	}
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
public static class RoleManagerAssignRoleOnDeathPatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return true; }
        if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

        var role = ExtremeRoleManager.GameRole[player.PlayerId];
        if (!role.IsAssignGhostRole())
        {
            var roleBehavior = player.Data.Role;

            if (!RoleManager.IsGhostRole(roleBehavior.Role))
            {
                player.RpcSetRole(roleBehavior.DefaultGhostRole);
            }
            return false;
        }
        if (GhostRoleSpawnDataManager.Instance.IsCombRole(role.Id)) { return false; }

        return true;
    }

    public static void Postfix([HarmonyArgument(0)] PlayerControl player)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return; }
        if (!RoleAssignState.Instance.IsRoleSetUpEnd ||
            !ExtremeRoleManager.GameRole[player.PlayerId].IsAssignGhostRole()) { return; }

        ExtremeGhostRoleManager.AssignGhostRoleToPlayer(player);
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.TryAssignSpecialGhostRoles))]
public static class RoleManagerTryAssignRoleOnDeathPatch
{
    // クルーの幽霊役職の処理（インポスターの時はここに来ない）
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        // バニラ幽霊クルー役職にニュートラルがアサインされる時はTrueを返す
        if (ExtremeRoleManager.GameRole.Count == 0 ||
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			ExtremeGameModeManager.Instance.ShipOption.GhostRole.IsAssignNeutralToVanillaCrewGhostRole)
        {
            return true;
        }

        var role = ExtremeRoleManager.GameRole[player.PlayerId];

        if (role.IsNeutral()) { return false; }

        // デフォルトのメソッドではニュートラルもクルー陣営の死亡者数にカウントされてアサインされなくなるため
        RoleTypes roleTypes = RoleTypes.GuardianAngel;

        int num = PlayerCache.AllPlayerControl.Count(
            (PlayerControl pc) =>
                pc.Data.IsDead &&
                !pc.Data.Role.IsImpostor &&
                ExtremeRoleManager.GameRole[pc.PlayerId].IsCrewmate());

        IRoleOptionsCollection roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
        {
            player.RpcSetRole(roleTypes);
            return false;
        }
        if (num > roleOptions.GetNumPerGame(roleTypes))
        {
            return false;
        }

        int chancePerGame = roleOptions.GetChancePerGame(roleTypes);

        if (HashRandom.Next(101) < chancePerGame)
        {
            player.RpcSetRole(roleTypes);
        }

        return false;
    }
}
