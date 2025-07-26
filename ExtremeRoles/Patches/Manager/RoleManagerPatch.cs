using System.Linq;

using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class RoleManagerAssignSelectRolesPatch
{
	public static void Prefix()
	{
		RoleAssignState.TryDestroy();

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
        if (!(
				RoleAssignState.Instance.IsRoleSetUpEnd &&
				ExtremeRoleManager.TryGetRole(player.PlayerId, out var role)
			))
        {
            return true;
        }

        if (!role.IsAssignGhostRole())
        {
            var roleBehavior = player.Data.Role;

            if (!RoleManager.IsGhostRole(roleBehavior.Role))
            {
                player.RpcSetRole(roleBehavior.DefaultGhostRole);
            }
            return false;
        }

        return !GhostRoleSpawnDataManager.Instance.IsCombRole(role.Core.Id);
    }

    public static void Postfix([HarmonyArgument(0)] PlayerControl player)
    {
        if (!(
				RoleAssignState.Instance.IsRoleSetUpEnd &&
				ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
				role.IsAssignGhostRole()
			))
        {
            return;
        }
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
        if (!RoleAssignState.Instance.IsRoleSetUpEnd ||
			ExtremeGameModeManager.Instance.ShipOption.GhostRole.IsAssignNeutralToVanillaCrewGhostRole ||
			!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role))
		{
            return true;
        }

		if (role.IsNeutral())
		{
			return false;
		}

        // デフォルトのメソッドではニュートラルもクルー陣営の死亡者数にカウントされてアサインされなくなるため
        RoleTypes roleTypes = RoleTypes.GuardianAngel;

        int num = PlayerCache.AllPlayerControl.Count(
            (PlayerControl pc) =>
                pc.Data.IsDead &&
                !pc.Data.Role.IsImpostor &&
                (ExtremeRoleManager.TryGetRole(pc.PlayerId, out var pcRole) && pcRole.IsCrewmate()));

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
