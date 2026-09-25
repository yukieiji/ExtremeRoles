using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class LiberalSingleRoleAssignDataBuilder : ILiberalSingleRoleAssignDataBuilder
{
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

		Logging.Debug(
			$"------------------------- SingleRoleAssign - Liberal - Start -------------------------");

		var liberalAssignTargetPlayer = SingleRoleAssignHelper.GetAssignablePlayer(data.Assign, liberalTeam);

		if (!liberalAssignTargetPlayer.Any())
		{
			return;
		}

		var leaderPlayer = liberalAssignTargetPlayer.OrderBy(x => RandomGenerator.Instance.Next()).Take(1).First();

		Logging.Debug($"Liberal Leader: {leaderPlayer.PlayerId}");

		// leaderの確実な割当
		data.Limit.Reduce(liberalTeam);
		data.Assign.AddAssignData(
			new PlayerToSingleRoleAssignData(
				leaderPlayer.PlayerId, intedLeaderId, data.Assign.ControlId));
		data.Assign.RemvePlayer(leaderPlayer);
		RoleAssignFilter.Instance.Update(intedLeaderId);

		var remainLiberalAssignTargetPlayer = SingleRoleAssignHelper.GetAssignablePlayer(data.Assign, liberalTeam);
		// 一人だとリーダー一人で終了！！
		if (liberalNum <= 1 || !remainLiberalAssignTargetPlayer.Any())
		{
			return;
		}

		var shuffle = remainLiberalAssignTargetPlayer.OrderBy(x => RandomGenerator.Instance.Next());

		// 過激派の数を想定する
		int mini = cate.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LiberalMilitantMini);
		int max = cate.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LiberalMilitantMax);
		int clampedMax = Math.Max(mini, max);
		int militantNum = RandomGenerator.Instance.Next(mini, clampedMax + 1);

		// リベラルのデフォルト役職以外の割当はまだ作ってない(というかまだ想定してない・・・・)
		if (militantNum > 0)
		{
			/// リベラル過激派割当
			/// デフォルトの割当
			addDefaultLiberalRoleAssignData(cate, (int)ExtremeRoleId.Militant, militantNum, data, shuffle);
		}

		/// リベル穏健派割当
		/// デフォルトの割当
		addDefaultLiberalRoleAssignData(cate, (int)ExtremeRoleId.Dove, liberalNum - 1 - militantNum, data, shuffle);

		Logging.Debug(
			$"------------------------- SingleRoleAssign - Liberal - End -------------------------");
	}

	private static void addDefaultLiberalRoleAssignData(
		in OptionCategory option,
		in int intedTargetId,
		in int targetNum,
		in PreparationData data,
		in IEnumerable<VanillaRolePlayerAssignData> randomTargetPlayer)
	{
		if (RoleAssignFilter.Instance.IsBlock(intedTargetId))
		{
			return;
		}

		var target = randomTargetPlayer.Take(targetNum);

		// 固定しておく
		foreach (var player in target.ToArray())
		{
			if (data.Limit.CanSpawn(ExtremeRoleType.Liberal) &&
				!RoleAssignFilter.Instance.IsBlock(intedTargetId))
			{
				data.Limit.Reduce(ExtremeRoleType.Liberal);
				Logging.Debug($"Liberal Default Role:{intedTargetId} to {player.PlayerId}");
				data.Assign.AddAssignData(
					new PlayerToSingleRoleAssignData(
						player.PlayerId, intedTargetId, data.Assign.ControlId));
				data.Assign.RemvePlayer(player);
				RoleAssignFilter.Instance.Update(intedTargetId);
			}
		}
	}
}
