using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;


#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class SingleRoleAssignDataBuilder(IVanillaRoleProvider roleProvider) : IRoleAssignDataBuildBehaviour
{
	public int Priority => (int)ExtremeRoleAssignDataBuilder.Priority.Single;

	private readonly record struct IdedSingleSpawnData(int RoleId, SingleRoleSpawnData Data);

	private readonly IReadOnlySet<RoleTypes> vanillaCrewRoleType = roleProvider.CrewmateRole;
	private readonly IReadOnlySet<RoleTypes> vanillaImpRoleType = roleProvider.ImpostorRole;

	public void Build(in PreparationData data)
	{
		Logging.Debug(
			$"----------------------------- SingleRoleAssign - Start -----------------------------");
		addImpostorSingleExtremeRoleAssignData(data);
		addNeutralSingleExtremeRoleAssignData(data);
		addLiberalSingleExtremeRoleAssignData(data);
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
		int neutralNum = data.Limit.Get(ExtremeRoleType.Neutral);
		if (neutralNum <= 0)
		{
			return;
		}

		Logging.Debug(
			$"------------------------- SingleRoleAssign - Neutral - Start -------------------------");
		var neutralAssignTargetPlayer = getAssignablePlayer(data.Assign, ExtremeRoleType.Neutral).ToList();

		int assignNum = Math.Clamp(
			neutralNum,
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


	private void addLiberalSingleExtremeRoleAssignData(in PreparationData data)
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

		var liberalAssignTargetPlayer = getAssignablePlayer(data.Assign, liberalTeam);

		if (!liberalAssignTargetPlayer.Any())
		{
			return;
		}

		var leaderPlayer = liberalAssignTargetPlayer.OrderBy(x => RandomGenerator.Instance.Next()).Take(1).First();

		// leaderの確実な割当
		data.Limit.Reduce(liberalTeam);
		data.Assign.AddAssignData(
			new PlayerToSingleRoleAssignData(
				leaderPlayer.PlayerId, intedLeaderId, data.Assign.ControlId));
		RoleAssignFilter.Instance.Update(intedLeaderId);

		var remainLiberalAssignTargetPlayer = getAssignablePlayer(data.Assign, liberalTeam);
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
				data.Assign.AddAssignData(
					new PlayerToSingleRoleAssignData(
						player.PlayerId, intedTargetId, data.Assign.ControlId));
				RoleAssignFilter.Instance.Update(intedTargetId);
			}
		}
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
			$"------------------------- SingleRoleAssign - Crewmate - End -------------------------");
	}

	private void addSingleExtremeRoleAssignDataFromTeamAndPlayer(
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

		var spawnCheckRoleId = createSingleRoleIdData(teamSpawnData);

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
					var target = shuffledSpawnCheckRoleId[i];
					int intedRoleId = target.RoleId;

					if (RoleAssignFilter.Instance.IsBlock(intedRoleId))
					{
						continue;
					}

					removePlayer = player;
					shuffledSpawnCheckRoleId.RemoveAt(i);

					Logging.Debug($"---AssignRole:{intedRoleId}---");

					target.Data.ReduceSpawnNum();

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

	private static IReadOnlyList<IdedSingleSpawnData> createSingleRoleIdData(
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

	private static IEnumerable<VanillaRolePlayerAssignData> getAssignablePlayer(PlayerRoleAssignData assignData, ExtremeRoleType targetTeam)
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
