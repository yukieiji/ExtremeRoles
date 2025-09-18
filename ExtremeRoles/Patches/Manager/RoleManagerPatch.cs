using System;
using System.Linq;

using HarmonyLib;

using InnerNet;
using Il2CppSystem.Linq;
using Il2CppSystem.Collections.Generic;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance.Il2Cpp;

using UnityHelper = ExtremeRoles.Helper.Unity;

#nullable enable

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class RoleManagerAssignSelectRolesPatch
{
	public static bool Prefix()
	{
		RoleAssignState.TryDestroy();

		if (ExtremeGameModeManager.Instance.EnableXion)
		{

			PlayerControl loaclPlayer = PlayerControl.LocalPlayer;

			loaclPlayer.RpcSetRole(RoleTypes.Crewmate);
			loaclPlayer.Data.IsDead = true;
		}

		var client = AmongUsClient.Instance.allClients;
		ClientData[] allPlayerArray;
		lock (client)
		{
			allPlayerArray = [..client.GetFastEnumerator()];
		}
		var filltedList = allPlayerArray
			.Where(
				c => 
					!(
						c == null ||
						c.Character == null ||
						c.Character.Data == null ||
						c.Character.Data.Disconnected ||
						c.Character.Data.IsDead
					))
			.OrderBy(c => c.Id)
			.Select(c => c.Character.Data)
			.ToList();

		foreach (var npd in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (npd.Object != null && 
				npd.Object.isDummy)
			{
				filltedList.Add(npd);
			}
		}
		
		IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		int maxRoleNum = filltedList.Count;
		int adjustedNumImpostors =
			ExtremeGameModeManager.Instance.RoleSelector.IsAdjustImpostorNum ?
			currentGameOptions.GetAdjustedNumImpostors(maxRoleNum) :
			Math.Clamp(currentGameOptions.NumImpostors, 0, maxRoleNum);


		var il2CppFiltedList = new List<NetworkedPlayerInfo>();
		foreach (var data in filltedList)
		{
			il2CppFiltedList.Add(data);
		}

		var logicRoleSelection = GameManager.Instance.LogicRoleSelection;
		
		logicRoleSelection.AssignRolesForTeam(
			il2CppFiltedList, currentGameOptions, RoleTeamTypes.Impostor,
			adjustedNumImpostors, UnityHelper.CreateNullAble(RoleTypes.Impostor));
		logicRoleSelection.AssignRolesForTeam(
			il2CppFiltedList, currentGameOptions, RoleTeamTypes.Crewmate,
			int.MaxValue, UnityHelper.CreateNullAble(RoleTypes.Crewmate));

		return false;
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
