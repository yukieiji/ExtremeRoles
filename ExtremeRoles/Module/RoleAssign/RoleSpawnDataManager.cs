using System.Collections.Generic;
using System.Text;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleSpawnDataManager : ISpawnDataManager
{
	public IReadOnlyDictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>> CurrentSingleRoleSpawnData
	{ get; private set; }

	public IReadOnlyDictionary<byte, CombinationRoleSpawnData> CurrentCombRoleSpawnData
	{ get; private set; }

	public IReadOnlyList<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)> UseGhostCombRole
	{ get; private set; }

	public IReadOnlyDictionary<ExtremeRoleType, int> CurrentSingleRoleUseNum
	{ get; private set; }

	public RoleSpawnDataManager()
	{

		var log = ExtremeRolesPlugin.Logger;

		log.LogInfo("-------- RoleSpawnDataManager - Construct START --------");

		log.LogInfo("---- RoleSpawnDataManager - Phase1 : instance variable initialize - START ----");
		var ghostRole = new List<(CombinationRoleType, GhostAndAliveCombinationRoleManagerBase)>();
		var combRole  = new Dictionary<byte, CombinationRoleSpawnData>();

		CurrentSingleRoleSpawnData = new Dictionary<ExtremeRoleType, Dictionary<int, SingleRoleSpawnData>>
		{
			{ ExtremeRoleType.Crewmate, new Dictionary<int, SingleRoleSpawnData>() },
			{ ExtremeRoleType.Impostor, new Dictionary<int, SingleRoleSpawnData>() },
			{ ExtremeRoleType.Neutral , new Dictionary<int, SingleRoleSpawnData>() },
		};

		var opt = OptionManager.Instance;

		var allRoleNum = new Dictionary<ExtremeRoleType, int>()
		{
			{ ExtremeRoleType.Crewmate, 0 },
			{ ExtremeRoleType.Impostor, 0 },
			{ ExtremeRoleType.Neutral , 0 },
		};

		log.LogInfo("---- RoleSpawnDataManager - Phase1 : instance variable initialize - END ----");
		log.LogInfo("---- RoleSpawnDataManager - Phase2 : Collect using CombinationRole - START ----");

		foreach (var roleId in ExtremeGameModeManager.Instance.RoleSelector.UseCombRoleType)
		{
			byte combType = (byte)roleId;
			if (!opt.TryGetCategory(
					OptionTab.CombinationTab,
					ExtremeRoleManager.GetCombRoleGroupId(roleId),
					out var conbCate))
			{
				continue;
			}
			int spawnRate = conbCate.GetValue<RoleCommonOption, int>(RoleCommonOption.SpawnRate);
			int roleSet = conbCate.GetValue<RoleCommonOption, int>(RoleCommonOption.RoleNum);

			var role = ExtremeRoleManager.CombRole[combType];
			if (roleSet <= 0 || spawnRate <= 0)
			{
				continue;
			}

			int weight = conbCate.GetValue<RoleCommonOption, int>(RoleCommonOption.AssignWeight);
			bool isMultiAssign = conbCate.GetValue<CombinationRoleCommonOption, bool>(CombinationRoleCommonOption.IsMultiAssign);

			log.LogInfo($"Add Combination Role:{role} - SpawnRate:{spawnRate} - RoleSetNum:{roleSet}");
			combRole.Add(
				combType,
				new CombinationRoleSpawnData(
					role: role,
					spawnSetNum: roleSet,
					spawnRate: spawnRate,
					weight: weight,
					isMultiAssign: isMultiAssign));

			if (role is GhostAndAliveCombinationRoleManagerBase ghostComb)
			{
				log.LogInfo($"This combination role is GhostAndAliveCombinationRoleManagerBase, add to UseGhostCombRole List");
				ghostRole.Add((roleId, ghostComb));
			}
		}

		log.LogInfo("---- RoleSpawnDataManager - Phase2 : Collect using CombinationRole - END ----");
		log.LogInfo("---- RoleSpawnDataManager - Phase3 : Collect using SingleRole - START ----");

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
			int roleNum = roleCate.GetValue<RoleCommonOption, int>(RoleCommonOption.RoleNum);

			if (roleNum <= 0 || spawnRate <= 0)
			{
				continue;
			}

			int weight = roleCate.GetValue<RoleCommonOption, int>(RoleCommonOption.AssignWeight);
			log.LogInfo($"Add Single Role:{role.RoleName} - SpawnRate:{spawnRate} - RoleSetNum:{roleNum}");

			var team = role.Core.Team;
			CurrentSingleRoleSpawnData[team].Add(
				intedRoleId, new SingleRoleSpawnData(roleNum, spawnRate, weight));
			allRoleNum[team] += roleNum;
		}

		log.LogInfo("---- RoleSpawnDataManager - Phase3 : Collect using SingleRole - END ----");
		log.LogInfo("-------- RoleSpawnDataManager - Construct END --------");

		CurrentCombRoleSpawnData = combRole;
		UseGhostCombRole = ghostRole;
		CurrentSingleRoleUseNum = allRoleNum;
	}

	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.AppendLine("------ RoleSpawnInfo ------");
		builder.AppendLine("--- CombRole ---");
		foreach (var (combId, combRole) in this.CurrentCombRoleSpawnData)
		{
			builder.AppendLine($"CombRoleId:{combId} SpawnSetNum:{combRole.SpawnSetNum} SpawnRate:{combRole.SpawnRate} AssignWeight:{combRole.Weight}");
		}
		builder.AppendLine("--- SingleRole ---");
		builder.AppendLine("-- TeamNum --");
		foreach (var (team, roleNum) in this.CurrentSingleRoleUseNum)
		{
			builder.AppendLine($"Team:{team} RoleNum{roleNum}");
		}
		builder.AppendLine("-- Detail --");
		foreach (var (teamId, teamData) in this.CurrentSingleRoleSpawnData)
		{
			foreach (var (id, role) in teamData)
			{
				builder.AppendLine(
					$"Team:{teamId} RoleId:{id} SpawnSetNum:{role.SpawnSetNum} SpawnRate:{role.SpawnRate} AssignWeight:{role.Weight}");
			}
		}
		builder.Append("------ RoleSpawnInfo ------");

		return builder.ToString();
	}
}
