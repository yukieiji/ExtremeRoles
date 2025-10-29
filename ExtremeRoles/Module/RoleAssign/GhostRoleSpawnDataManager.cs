using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class GhostRoleSpawnDataManager :
	NullableSingleton<GhostRoleSpawnDataManager>
{
	private Dictionary<ExtremeRoleType, int> globalSpawnLimit = new Dictionary<ExtremeRoleType, int>();

	private Dictionary<ExtremeRoleId, CombinationRoleType> combRole = new Dictionary<
		ExtremeRoleId, CombinationRoleType>();

	private Dictionary<ExtremeRoleType, List<GhostRoleSpawnData>> useGhostRole = new Dictionary<
		ExtremeRoleType, List<GhostRoleSpawnData>>();

	public GhostRoleSpawnDataManager()
	{
		this.clear();
	}

	public void Create(
		IReadOnlyList<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)> useGhostCombRole)
	{
		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo("-------- GostRoleSpawnDataManager - Construct Start --------");
		this.clear();

		foreach (var (combRoleId, mng) in useGhostCombRole)
		{
			foreach (ExtremeRoleId roleId in mng.CombGhostRole.Keys)
			{
				this.combRole.Add(roleId, combRoleId);
			}
		}

		var opt = OptionManager.Instance;

		if (opt.TryGetCategory(OptionTab.GeneralTab, (int)SpawnOptionCategory.GhostRoleSpawnCategory, out var cate))
		{
			var loader = cate.Loader;
			this.globalSpawnLimit = new Dictionary<ExtremeRoleType, int>
			{
				{
					ExtremeRoleType.Crewmate,
					ISpawnLimiter.ComputeSpawnNum(
						loader,
						RoleSpawnOption.MinCrewmate,
						RoleSpawnOption.MaxCrewmate)
				},
				{
					ExtremeRoleType.Neutral,
					ISpawnLimiter.ComputeSpawnNum(
						loader,
						RoleSpawnOption.MinNeutral,
						RoleSpawnOption.MaxNeutral)
				},
				{
					ExtremeRoleType.Impostor,
					ISpawnLimiter.ComputeSpawnNum(
						loader,
						RoleSpawnOption.MinImpostor,
						RoleSpawnOption.MaxImpostor)
				},
			};
		}
		else
		{
			this.globalSpawnLimit = new();
		}
		var tmpUseData = new Dictionary<ExtremeRoleType, List<GhostRoleSpawnData>>();

		foreach (ExtremeGhostRoleId roleId in
			ExtremeGameModeManager.Instance.RoleSelector.UseGhostRoleId)
		{
			var role = ExtremeGhostRoleManager.AllGhostRole[roleId];

			var tab = role.Team switch
			{
				ExtremeRoleType.Neutral => OptionTab.GhostNeutralTab,
				ExtremeRoleType.Crewmate => OptionTab.GhostCrewmateTab,
				ExtremeRoleType.Impostor => OptionTab.GhostImpostorTab,
				_ => throw new System.ArgumentOutOfRangeException(),
			};

			if (!opt.TryGetCategory(
					tab, ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleOptionCategoryIdGenerator>().Get(role.Id),
					out var roleCate))
			{
				continue;
			}

			var loader = roleCate.Loader;
			int spawnRate = loader.GetValue<RoleCommonOption, int>(RoleCommonOption.SpawnRate);
			int weight = loader.GetValue<RoleCommonOption, int>(RoleCommonOption.AssignWeight);
			int roleNum = loader.GetValue<RoleCommonOption, int>(RoleCommonOption.RoleNum);

			logger.LogInfo(
				$"GhostRoleSpawnInfo,  Name:{role.Name}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

			if (roleNum <= 0 || spawnRate <= 0)
			{
				continue;
			}

			var addData = new GhostRoleSpawnData(
				roleId, roleNum, spawnRate, weight, role.GetRoleFilter());

			ExtremeRoleType team = role.Team;

			if (!tmpUseData.ContainsKey(team))
			{
				List<GhostRoleSpawnData> teamGhostRole = new List<GhostRoleSpawnData>()
				{
					addData,
				};

				tmpUseData.Add(team, teamGhostRole);
			}
			else
			{
				tmpUseData[team].Add(addData);
			}
		}

		foreach (var (team, spawnDataList) in tmpUseData)
		{
			logger.LogInfo($"Add {team} ghost role spawn data");
			this.useGhostRole[team] = spawnDataList
				.OrderByDescending(x => x.Weight)
				.ThenBy(x => RandomGenerator.Instance.Next())
				.ToList();
		}
		logger.LogInfo("-------- GostRoleSpawnDataManager - Construct End --------");
	}

	public CombinationRoleType GetCombRoleType(ExtremeRoleId roleId) =>
		this.combRole[roleId];

	public int GetGlobalSpawnLimit(ExtremeRoleType team)
	{
		if (this.globalSpawnLimit.TryGetValue(team, out int limit))
		{
			return limit;
		}
		else
		{
			return int.MinValue;
		}
	}

	public List<GhostRoleSpawnData> GetUseGhostRole(
		ExtremeRoleType team)
	{
		this.useGhostRole.TryGetValue(team, out List<GhostRoleSpawnData> data);
		return data;
	}

	public bool IsCombRole(ExtremeRoleId roleId) => this.combRole.ContainsKey(roleId);

	public bool IsGlobalSpawnLimit(ExtremeRoleType team)
	{
		bool isGhostRoleArrive = this.globalSpawnLimit.TryGetValue(
			team, out int globalSpawnLimit);

		return isGhostRoleArrive && globalSpawnLimit <= 0;
	}

	public void ReduceGlobalSpawnLimit(ExtremeRoleType team)
	{
		this.globalSpawnLimit[team] = this.globalSpawnLimit[team] - 1;
	}

	private void clear()
	{
		this.globalSpawnLimit.Clear();
		this.useGhostRole.Clear();
		this.combRole.Clear();
	}
}
