using System.Collections.Generic;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserCombRoleProvider : IGuesserCombRoleProvider
{
	public void AddExRCombRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		GuesserNormalRoleAssignState assignState)
	{
		foreach (var (id, roleMng) in ExtremeRoleManager.CombRole)
		{
			var loader = roleMng.Loader;

			int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.SpawnRate);
			int roleNum = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.RoleNum);

			bool multiAssign = loader.GetValue<CombinationRoleCommonOption, bool>(
				CombinationRoleCommonOption.IsMultiAssign);

			if (spawnOptSel < 1 || roleNum <= 0)
			{
				continue;
			}

			bool isNotTraitor = id != (byte)CombinationRoleType.Traitor;

			if (roleMng is FlexibleCombinationRoleManagerBase flexMng &&
				isNotTraitor)
			{
				ExtremeRoleType team = flexMng.BaseRole.Core.Team;
				ExtremeRoleId baseRoleId = flexMng.BaseRole.Core.Id;

				if (multiAssign)
				{
					if (loader.TryGetValue(
							CombinationRoleCommonOption.IsAssignImposter,
							out bool isImp) && isImp)
					{
						listAddTargetTeam(
							result,
							separatedRoleId,
							baseRoleId,
							ExtremeRoleType.Crewmate,
							ExtremeRoleType.Crewmate);
						listAddTargetTeam(
							result,
							separatedRoleId,
							baseRoleId,
							ExtremeRoleType.Impostor,
							ExtremeRoleType.Impostor);
					}
					else
					{
						listAddTargetTeam(result, separatedRoleId, baseRoleId, team, team);
					}
				}
				else
				{
					add(result, baseRoleId, team);
				}
				if (assignState.IsJackalOn &&
					!assignState.IsJackalForceReplaceLover &&
					baseRoleId == ExtremeRoleId.Lover)
				{
					add(result, baseRoleId, ExtremeRoleType.Neutral, ExtremeRoleId.Sidekick);
				}
			}
			else if (multiAssign && isNotTraitor)
			{
				foreach (var role in roleMng.Roles)
				{
					ExtremeRoleType team = role.Core.Team;
					listAdd(result, role.Core.Id, team, separatedRoleId[team]);
				}
				// 見習い捜査官の追加
				if (isInvestigatorOffice(id))
				{
					listAdd(result, ExtremeRoleId.InvestigatorApprentice, ExtremeRoleType.Crewmate, separatedRoleId[ExtremeRoleType.Crewmate]);
				}
			}
			else
			{
				foreach (var role in roleMng.Roles)
				{
					add(result, role.Core.Id, role.Core.Team);
				}

				// 見習い捜査官の追加
				if (isInvestigatorOffice(id))
				{
					add(result, ExtremeRoleId.InvestigatorApprentice, ExtremeRoleType.Crewmate);
				}
			}
		}
	}

	private static bool isInvestigatorOffice(byte checkId)
		=> checkId == (byte)CombinationRoleType.InvestigatorOffice;

	private static void add(
		List<GuessBehaviour.RoleInfo> result,
		ExtremeRoleId id,
		ExtremeRoleType team,
		ExtremeRoleId another = ExtremeRoleId.Null)
	{
		result.Add(
			new GuessBehaviour.RoleInfo()
			{
				Id = id,
				AnothorId = another,
				Team = team,
			});
	}

	private static void listAdd(
		List<GuessBehaviour.RoleInfo> result,
		ExtremeRoleId baseId,
		ExtremeRoleType team,
		List<ExtremeRoleId> list)
	{
		foreach (var roleId in list)
		{
			add(result, baseId, team, roleId);
		}
	}

	private static void listAddTargetTeam(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		ExtremeRoleId baseId,
		ExtremeRoleType team,
		ExtremeRoleType targetType)
	{
		listAdd(result, baseId, team, separatedRoleId[targetType]);
	}
}
