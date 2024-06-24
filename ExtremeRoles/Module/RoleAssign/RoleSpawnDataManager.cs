using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.NewOption;
using Unity.Jobs.LowLevel.Unsafe;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleSpawnDataManager : ISpawnDataManager
{
	public Dictionary<ExtremeRoleType, int> MaxRoleNum { get; private set; }

	public Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>> CurrentSingleRoleSpawnData
	{ get; private set; }

	public Dictionary<byte, CombinationRoleSpawnData> CurrentCombRoleSpawnData
	{ get; private set; }

	public List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)> UseGhostCombRole
	{ get; private set; }

	public Dictionary<ExtremeRoleType, int> CurrentSingleRoleUseNum
	{ get; private set; }

	public RoleSpawnDataManager()
	{
		UseGhostCombRole = new List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)>();
		CurrentCombRoleSpawnData = new Dictionary<byte, CombinationRoleSpawnData>();

		CurrentSingleRoleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
		{
			{ ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnData>() },
			{ ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnData>() },
			{ ExtremeRoleType.Neutral , new Dictionary<int, SingleRoleSpawnData>() },
		};

		var opt = NewOptionManager.Instance;

		MaxRoleNum = opt.TryGetCategory(OptionTab.General, (int)SpawnOptionCategory.RoleSpawnCategory, out var cate)
			? new Dictionary<ExtremeRoleType, int>
			{
				{
					ExtremeRoleType.Crewmate,
					ISpawnDataManager.ComputeSpawnNum(
						cate,
						RoleSpawnOption.MinCrewmate,
						RoleSpawnOption.MaxCrewmate)
				},
				{
					ExtremeRoleType.Neutral,
					ISpawnDataManager.ComputeSpawnNum(
						cate,
						RoleSpawnOption.MinNeutral,
						RoleSpawnOption.MaxNeutral)
				},
				{
					ExtremeRoleType.Impostor,
					ISpawnDataManager.ComputeSpawnNum(
						cate,
						RoleSpawnOption.MinImpostor,
						RoleSpawnOption.MaxImpostor)
				},
			} : new ();


		CurrentSingleRoleUseNum = new Dictionary<ExtremeRoleType, int>()
		{
			{ ExtremeRoleType.Crewmate, 0 },
			{ ExtremeRoleType.Impostor, 0 },
			{ ExtremeRoleType.Neutral , 0 },
		};

		foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseCombRoleType)
		{
			byte combType = (byte)roleId;
			if (!opt.TryGetCategory(
					OptionTab.Combination,
					ExtremeRoleManager.GetCombRoleGroupId(roleId),
					out var conbCate))
			{
				continue;
			}
			int spawnRate = conbCate.GetValue<RoleCommonOption, int>(RoleCommonOption.SpawnRate);
			int roleSet = conbCate.GetValue<RoleCommonOption, int>(RoleCommonOption.RoleNum);
			int weight = conbCate.GetValue<RoleCommonOption, int>(RoleCommonOption.AssignWeight);
			bool isMultiAssign = conbCate.GetValue<CombinationRoleCommonOption, bool>(CombinationRoleCommonOption.IsMultiAssign);

			var role = ExtremeRoleManager.CombRole[combType];
			Logging.Debug($"Role:{role}    SpawnRate:{spawnRate}   RoleSet:{roleSet}");

			if (roleSet <= 0 || spawnRate <= 0.0)
			{
				continue;
			}
			CurrentCombRoleSpawnData.Add(
				combType,
				new CombinationRoleSpawnData(
					role: role,
					spawnSetNum: roleSet,
					spawnRate: spawnRate,
					weight: weight,
					isMultiAssign: isMultiAssign));

			if (role is GhostAndAliveCombinationRoleManagerBase ghostComb)
			{
				this.UseGhostCombRole.Add((roleId, ghostComb));
			}
		}

		foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseNormalRoleId)
		{
			int intedRoleId = (int)roleId;
			SingleRoleBase role = ExtremeRoleManager.NormalRole[intedRoleId];
			if (!opt.TryGetCategory(
					role.Tab,
					ExtremeRoleManager.GetRoleGroupId(roleId),
					out var roleCate))
			{
				continue;
			}

			int spawnRate = roleCate.GetValue<RoleCommonOption, int>(RoleCommonOption.SpawnRate);
			int weight = roleCate.GetValue<RoleCommonOption, int>(RoleCommonOption.AssignWeight);
			int roleNum = roleCate.GetValue<RoleCommonOption, int>(RoleCommonOption.RoleNum);

			Logging.Debug(
				$"Role Name:{role.RoleName}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

			if (roleNum <= 0 || spawnRate <= 0.0)
			{
				continue;
			}

			CurrentSingleRoleSpawnData[role.Team].Add(
				intedRoleId, new SingleRoleSpawnData(roleNum, spawnRate, weight));
			CurrentSingleRoleUseNum[role.Team] += roleNum;
		}
	}

	public bool IsCanSpawnTeam(ExtremeRoleType roleType, int reduceNum = 1)
	{
		return
			this.MaxRoleNum.TryGetValue(roleType, out int maxNum) &&
			maxNum - reduceNum >= 0;
	}

	public void ReduceSpawnLimit(ExtremeRoleType roleType, int reduceNum = 1)
	{
		this.MaxRoleNum[roleType] = this.MaxRoleNum[roleType] - reduceNum;
	}
}
