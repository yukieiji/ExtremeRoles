using System;
using System.Linq;

using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using InnerNet;

using AmongUs.GameOptions;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;

using UnityHelper = ExtremeRoles.Helper.Unity;

using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;


#nullable enable

namespace ExtremeRoles.Patches.Manager;


// GetAdjustedNumImpostorsのパッチが動作しないのでバニラの実装を完コピしてインポスターの人数だけ強制的にパッチを当てる
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class RoleManagerAssignSelectRolesPatch
{
	public static bool Prefix()
	{
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
						c == null || c.Character.IsInValid()
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
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] bool specialRolesAllowed)
    {
        if (!(
				GameProgressSystem.IsGameNow &&
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
				GameProgressSystem.IsGameNow &&
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
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
    {
		var flag = ExtremeGameModeManager.Instance.ShipOption.GhostRole.AssignToVanillaCrewmateGhostRole;
		return
			!GameProgressSystem.IsGameNow ||
			!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
			role.IsImpostor() ||
			role.IsCrewmate() ||
			(flag.HasFlag(VanillaCrewmateGhostRoleAssign.NeutalOk) && role.IsNeutral()) ||
			(flag.HasFlag(VanillaCrewmateGhostRoleAssign.LiberalOk) && role.IsLiberal());
	}
}
