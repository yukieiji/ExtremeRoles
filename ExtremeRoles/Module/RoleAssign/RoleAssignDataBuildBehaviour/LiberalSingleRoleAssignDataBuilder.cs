using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class LiberalSingleRoleAssignDataBuilder(
	ISingleRoleAssignHelper helper,
	IModLogger logger) : ILiberalSingleRoleAssignDataBuilder
{
	private readonly IModLogger logger = logger;

	public void Build(in PreparationData data)
	{
		const ExtremeRoleType liberalTeam = ExtremeRoleType.Liberal;

		int liberalNum = data.Limit.Get(liberalTeam);
		int intedLeaderId = (int)ExtremeRoleId.Leader;
		if (liberalNum <= 0 ||
			RoleAssignFilter.Instance.IsBlock(intedLeaderId) ||
			!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)SpawnOptionCategory.LiberalSetting, out var cate))
		{
			return;
		}

		logger.LogTrace("------------------------- SingleRoleAssign - Liberal - Start -------------------------");

		var liberalAssignTargetPlayer = helper.GetAssignablePlayer(data.Assign, liberalTeam);

		if (!liberalAssignTargetPlayer.Any())
		{
			return;
		}

		var leaderPlayer = liberalAssignTargetPlayer.OrderBy(x => RandomGenerator.Instance.Next()).Take(1).First();

		logger.LogTrace($"Liberal Leader: {leaderPlayer.PlayerId}");

		// leaderの確実な割当
		data.Limit.Reduce(liberalTeam);
		data.Assign.AddAssignData(
			new PlayerToSingleRoleAssignData(
				leaderPlayer.PlayerId, intedLeaderId, data.Assign.ControlId));
		data.Assign.RemvePlayer(leaderPlayer);
		RoleAssignFilter.Instance.Update(intedLeaderId);

		var remainLiberalAssignTargetPlayer = helper.GetAssignablePlayer(data.Assign, liberalTeam);
		// 一人だとリーダー一人で終了！！
		if (liberalNum <= 1 || !remainLiberalAssignTargetPlayer.Any())
		{
			return;
		}

		var targetPlayers = remainLiberalAssignTargetPlayer
			.OrderBy(x => RandomGenerator.Instance.Next())
			.ToList();

		// 過激派の数を想定する
		int mini = cate.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LiberalMilitantMini);
		int max = cate.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LiberalMilitantMax);
		int clampedMax = Math.Max(mini, max);
		int militantNum = RandomGenerator.Instance.Next(mini, clampedMax + 1);

		militantNum = Math.Clamp(militantNum, 0, targetPlayers.Count);
		int doveNum = Math.Clamp(liberalNum - 1 - militantNum, 0, targetPlayers.Count - militantNum);

		var militantTargetPlayers = targetPlayers.Take(militantNum).ToList();
		var doveTargetPlayers = targetPlayers.Skip(militantNum).Take(doveNum).ToList();

		if (!data.RoleSpawn.CurrentSingleRoleSpawnData.TryGetValue(liberalTeam, out var liberalSpawnDict))
		{
			logger.LogError("Can't find liberal single role spawn data.");
			return;
		}

		var militantSpawnDict = new Dictionary<int, SingleRoleSpawnData>();
		var doveSpawnDict = new Dictionary<int, SingleRoleSpawnData>();

		foreach (var (roleId, spawnData) in liberalSpawnDict)
		{
			if (ExtremeRoleManager.NormalRole.TryGetValue(roleId, out var role))
			{
				if (role.HasTask)
				{
					doveSpawnDict.Add(roleId, spawnData);
				}
				else
				{
					militantSpawnDict.Add(roleId, spawnData);
				}
			}
		}

		var militantCandidates = SingleRoleAssignHelper.CreateSingleRoleIdData(militantSpawnDict)
			.OrderByDescending(x => x.Data.Weight)
			.ThenBy(x => RandomGenerator.Instance.Next())
			.ToList();

		var doveCandidates = SingleRoleAssignHelper.CreateSingleRoleIdData(doveSpawnDict)
			.OrderByDescending(x => x.Data.Weight)
			.ThenBy(x => RandomGenerator.Instance.Next())
			.ToList();

		if (militantTargetPlayers.Count > 0)
		{
			assignLiberalRoles(
				militantTargetPlayers,
				militantCandidates,
				(int)ExtremeRoleId.Militant,
				data,
				liberalTeam);
		}

		if (doveTargetPlayers.Count > 0)
		{
			assignLiberalRoles(
				doveTargetPlayers,
				doveCandidates,
				(int)ExtremeRoleId.Dove,
				data,
				liberalTeam);
		}

		logger.LogTrace("------------------------- SingleRoleAssign - Liberal - End -------------------------");
	}

	private void assignLiberalRoles(
		IReadOnlyList<VanillaRolePlayerAssignData> targetPlayers,
		List<SingleRoleAssignHelper.IdedSingleSpawnData> candidates,
		int defaultRoleId,
		in PreparationData data,
		ExtremeRoleType liberalTeam)
	{
		foreach (var player in targetPlayers)
		{
			if (!data.Limit.CanSpawn(liberalTeam))
			{
				break;
			}

			bool assigned = false;

			for (int i = 0; i < candidates.Count; ++i)
			{
				var candidate = candidates[i];
				int roleId = candidate.RoleId;

				if (RoleAssignFilter.Instance.IsBlock(roleId))
				{
					continue;
				}

				candidates.RemoveAt(i);
				candidate.Data.ReduceSpawnNum();
				data.Limit.Reduce(liberalTeam);
				logger.LogTrace($"Liberal Role:{roleId} to {player.PlayerId}");
				data.Assign.AddAssignData(
					new PlayerToSingleRoleAssignData(
						player.PlayerId, roleId, data.Assign.ControlId));
				data.Assign.RemvePlayer(player);
				RoleAssignFilter.Instance.Update(roleId);
				assigned = true;
				break;
			}

			if (!assigned && !RoleAssignFilter.Instance.IsBlock(defaultRoleId))
			{
				data.Limit.Reduce(liberalTeam);
				logger.LogTrace($"Liberal Default Role:{defaultRoleId} to {player.PlayerId}");
				data.Assign.AddAssignData(
					new PlayerToSingleRoleAssignData(
						player.PlayerId, defaultRoleId, data.Assign.ControlId));
				data.Assign.RemvePlayer(player);
				RoleAssignFilter.Instance.Update(defaultRoleId);
			}
		}
	}
}
