using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class SingleRoleAssignDataBuilder(IVanillaRoleProvider roleProvider) : IRoleAssignDataBuildBehaviour
{
	public int Priority => (int)ExtremeRoleAssignDataBuilder.Priority.Single;

	private readonly IReadOnlySet<RoleTypes> vanillaCrewRoleType = roleProvider.CrewmateRole;
	private readonly IReadOnlySet<RoleTypes> vanillaImpRoleType = roleProvider.ImpostorRole;

	public void Build(in PreparationData data)
	{
		Logging.Debug(
			$"----------------------------- SingleRoleAssign - Start -----------------------------");
		addImpostorSingleExtremeRoleAssignData(data);
		addNeutralSingleExtremeRoleAssignData(data);
		addCrewmateSingleExtremeRoleAssignData(data);
		Logging.Debug(
			$"----------------------------- SingleRoleAssign - End -----------------------------");
	}

	private void addImpostorSingleExtremeRoleAssignData(in PreparationData data)
	{
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Impostor - Start -------------------------");
		addSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Impostor,
			data.Assign.GetCanImpostorAssignPlayer(),
			vanillaImpRoleType);
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Impostor - End -------------------------");
	}

	private void addNeutralSingleExtremeRoleAssignData(in PreparationData data)
	{
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Neutral - Start -------------------------");
		var neutralAssignTargetPlayer = new List<VanillaRolePlayerAssignData>();

		foreach (var player in data.Assign.GetCanCrewmateAssignPlayer())
		{
			RoleTypes vanillaRoleId = player.Role;

			if ((
					data.Assign.TryGetCombRoleAssign(player.PlayerId, out ExtremeRoleType team) &&
					team != ExtremeRoleType.Neutral
				)
				||
				(
					!ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign &&
					vanillaRoleId != RoleTypes.Crewmate
				))
			{
				continue;
			}
			neutralAssignTargetPlayer.Add(player);
		}

		int assignNum = Math.Clamp(
			data.Limit.Get(ExtremeRoleType.Neutral),
			0, Math.Min(
				neutralAssignTargetPlayer.Count,
				data.RoleSpawn.CurrentSingleRoleUseNum[ExtremeRoleType.Neutral]));

		Logging.Debug($"Neutral assign num:{assignNum}");

		neutralAssignTargetPlayer = neutralAssignTargetPlayer.OrderBy(
			x => RandomGenerator.Instance.Next()).Take(assignNum).ToList();

		addSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Neutral,
			neutralAssignTargetPlayer,
			vanillaCrewRoleType);
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Neutral - End -------------------------");
	}

	private void addCrewmateSingleExtremeRoleAssignData(in PreparationData data)
	{

		Logging.Debug(
			$"------------------------- SingleRoleAssign - Crewmate - Start -------------------------");
		addSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Crewmate,
			data.Assign.GetCanCrewmateAssignPlayer(),
			vanillaCrewRoleType);
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Neutral - Start -------------------------");
	}

	private void addSingleExtremeRoleAssignDataFromTeamAndPlayer(
		in PreparationData data,
		ExtremeRoleType team,
		in IReadOnlyList<VanillaRolePlayerAssignData> targetPlayer,
		in IReadOnlySet<RoleTypes> vanilaTeams)
	{
		var teamSpawnData = data.RoleSpawn.CurrentSingleRoleSpawnData[team];

		if (targetPlayer.Count == 0) { return; }

		var spawnCheckRoleId = createSingleRoleIdData(teamSpawnData);

		if (spawnCheckRoleId.Count == 0) { return; }

		var shuffledSpawnCheckRoleId = spawnCheckRoleId
			.OrderByDescending(x => x.weight) // まずは重みでソート
			.ThenBy(x => RandomGenerator.Instance.Next()) //同じ重みをシャッフル
			.Select(x => x.intedRoleId)
			.ToList();
		var shuffledTargetPlayer = targetPlayer.OrderBy(x => RandomGenerator.Instance.Next());

		foreach (var player in shuffledTargetPlayer)
		{
			Logging.Debug(
				$"-------------------AssignToPlayer:{player.PlayerName}-------------------");
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
				Logging.Debug($"---AssignRole:{vanillaRoleId}---");
			}

			if (data.Limit.CanSpawn(team) &&
				shuffledSpawnCheckRoleId.Count > 0 &&
				removePlayer == null)
			{
				for (int i = 0; i < shuffledSpawnCheckRoleId.Count; ++i)
				{
					int intedRoleId = shuffledSpawnCheckRoleId[i];

					if (RoleAssignFilter.Instance.IsBlock(intedRoleId)) { continue; }

					removePlayer = player;
					shuffledSpawnCheckRoleId.RemoveAt(i);

					Logging.Debug($"---AssignRole:{intedRoleId}---");

					data.Limit.Reduce(team);
					data.Assign.AddAssignData(
						new PlayerToSingleRoleAssignData(
							player.PlayerId, intedRoleId, data.Assign.ControlId));

					RoleAssignFilter.Instance.Update(intedRoleId);
					break;
				}
			}

			Logging.Debug($"-------------------AssignEnd-------------------");
			if (removePlayer.HasValue)
			{
				data.Assign.RemvePlayer(removePlayer.Value);
			}
		}
	}

	private static List<(int intedRoleId, int weight)> createSingleRoleIdData(
		in IReadOnlyDictionary<int, SingleRoleSpawnData> spawnData)
	{
		var result = new List<(int, int)>();

		foreach (var (intedRoleId, data) in spawnData)
		{
			for (int i = 0; i < data.SpawnSetNum; ++i)
			{
				if (!data.IsSpawn()) { continue; }

				result.Add((intedRoleId, data.Weight));
			}
		}

		return result;
	}
}
