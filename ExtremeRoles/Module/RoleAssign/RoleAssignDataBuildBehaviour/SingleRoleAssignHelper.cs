using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class SingleRoleAssignHelper(IModLogger logger) : ISingleRoleAssignHelper
{
	internal readonly record struct IdedSingleSpawnData(int RoleId, SingleRoleSpawnData Data);

	public void AddSingleExtremeRoleAssignDataFromTeamAndPlayer(
		in PreparationData data,
		ExtremeRoleType team,
		in IReadOnlyList<VanillaRolePlayerAssignData> targetPlayer,
		in IReadOnlySet<RoleTypes> vanilaTeams)
	{
		if (targetPlayer.Count == 0 ||
			!data.RoleSpawn.CurrentSingleRoleSpawnData.TryGetValue(team, out var teamSpawnData) ||
			teamSpawnData is null)
		{
			return;
		}

		var spawnCheckRoleId = CreateSingleRoleIdData(teamSpawnData);

		if (spawnCheckRoleId.Count == 0)
		{
			return;
		}

		var shuffledSpawnCheckRoleId = spawnCheckRoleId
			.OrderByDescending(x => x.Data.Weight) // まずは重みでソート
			.ThenBy(x => RandomGenerator.Instance.Next()) //同じ重みをシャッフル
			.ToList();
		var shuffledTargetPlayer = targetPlayer.OrderBy(x => RandomGenerator.Instance.Next());

		foreach (var player in shuffledTargetPlayer)
		{
			logger.LogTrace($"-------------------AssignToPlayer:{player.PlayerName}-------------------");
			VanillaRolePlayerAssignData? removePlayer = null;

			RoleTypes vanillaRoleId = player.Role;

			if (vanilaTeams.Contains(vanillaRoleId))
			{
				// マルチアサインでコンビ役職にアサインされてないプレイヤーは追加でアサインが必要
				removePlayer =
					ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign
					||
					(
						data.Assign.TryGetCombRoleAssign(player.PlayerId, out ExtremeRoleType combTeam) &&
						combTeam == team
					)
					? null : player;

				data.Assign.AddAssignData(
					new PlayerToSingleRoleAssignData(
						player.PlayerId, (int)vanillaRoleId,
						data.Assign.ControlId));
				logger.LogTrace($"---AssignRole:{vanillaRoleId}---");
			}

			if (data.Limit.CanSpawn(team) &&
				shuffledSpawnCheckRoleId.Count > 0 &&
				removePlayer == null)
			{
				for (int i = 0; i < shuffledSpawnCheckRoleId.Count; ++i)
				{
					var target = shuffledSpawnCheckRoleId[i];
					int intedRoleId = target.RoleId;

					if (RoleAssignFilter.Instance.IsBlock(intedRoleId))
					{
						continue;
					}

					removePlayer = player;
					shuffledSpawnCheckRoleId.RemoveAt(i);

					logger.LogTrace($"---AssignRole:{intedRoleId}---");

					target.Data.ReduceSpawnNum();

					data.Limit.Reduce(team);
					data.Assign.AddAssignData(
						new PlayerToSingleRoleAssignData(
							player.PlayerId, intedRoleId, data.Assign.ControlId));

					RoleAssignFilter.Instance.Update(intedRoleId);
					break;
				}
			}

			logger.LogTrace($"-------------------AssignEnd-------------------");
			if (removePlayer.HasValue)
			{
				data.Assign.RemvePlayer(removePlayer.Value);
			}
		}
	}

	internal static IReadOnlyList<IdedSingleSpawnData> CreateSingleRoleIdData(
		in IReadOnlyDictionary<int, SingleRoleSpawnData> spawnData)
	{
		var result = new List<IdedSingleSpawnData>();

		foreach (var (intedRoleId, data) in spawnData)
		{
			for (int i = 0; i < data.SpawnSetNum; ++i)
			{
				if (!data.IsSpawn())
				{
					continue;
				}

				result.Add(new (intedRoleId, data));
			}
		}

		return result;
	}

	public IEnumerable<VanillaRolePlayerAssignData> GetAssignablePlayer(PlayerRoleAssignData assignData, ExtremeRoleType targetTeam)
	{
		foreach (var player in assignData.GetCanCrewmateAssignPlayer())
		{
			var vanillaRoleId = player.Role;

			if ((
					assignData.TryGetCombRoleAssign(player.PlayerId, out var team) &&
					team != targetTeam
				)
				||
				!(
					ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign ||
					VanillaRoleProvider.IsDefaultCrewmateRole(vanillaRoleId)
				))
			{
				continue;
			}
			yield return player;
		}
	}
}
