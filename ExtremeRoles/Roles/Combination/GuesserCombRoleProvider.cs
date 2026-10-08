using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserCombRoleProvider : IGuesserCombRoleProvider
{
	public void AddExRCombRoles(
		IGuesserRoleInfoContainer container,
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

			ProcessCombRoleManager(container, id, roleMng, multiAssign, assignState);
		}
	}

	public static void ProcessCombRoleManager(
		IGuesserRoleInfoContainer container,
		byte id,
		CombinationRoleManagerBase roleMng,
		bool multiAssign,
		GuesserNormalRoleAssignState assignState)
	{
		bool isNotTraitor = id != (byte)CombinationRoleType.Traitor;

		if (roleMng is FlexibleCombinationRoleManagerBase flexMng && isNotTraitor)
		{
			ProcessFlexibleCombRole(container, flexMng, multiAssign, assignState);
		}
		else if (multiAssign && isNotTraitor)
		{
			ProcessMultiAssignCombRole(container, id, roleMng);
		}
		else
		{
			ProcessSingleAssignCombRole(container, id, roleMng);
		}
	}

	public static void ProcessFlexibleCombRole(
		IGuesserRoleInfoContainer container,
		FlexibleCombinationRoleManagerBase flexMng,
		bool multiAssign,
		GuesserNormalRoleAssignState assignState)
	{
		ExtremeRoleType team = flexMng.BaseRole.Core.Team;
		ExtremeRoleId baseRoleId = flexMng.BaseRole.Core.Id;

		if (multiAssign)
		{
			bool isAssignImp = flexMng.Loader.TryGetValue(
				CombinationRoleCommonOption.IsAssignImposter,
				out bool isImp) && isImp;

			ProcessFlexibleMultiAssign(container, baseRoleId, team, isAssignImp);
		}
		else
		{
			container.Add(baseRoleId, team);
		}

		if (assignState.IsJackalOn &&
			!assignState.IsJackalForceReplaceLover &&
			baseRoleId == ExtremeRoleId.Lover)
		{
			container.Add(baseRoleId, ExtremeRoleType.Neutral, ExtremeRoleId.Sidekick);
		}
	}

	public static void ProcessFlexibleMultiAssign(
		IGuesserRoleInfoContainer container,
		ExtremeRoleId baseRoleId,
		ExtremeRoleType team,
		bool isAssignImp)
	{
		if (isAssignImp)
		{
			container.ListAddTargetTeam(
				baseRoleId,
				ExtremeRoleType.Crewmate,
				ExtremeRoleType.Crewmate);
			container.ListAddTargetTeam(
				baseRoleId,
				ExtremeRoleType.Impostor,
				ExtremeRoleType.Impostor);
		}
		else
		{
			container.ListAddTargetTeam(baseRoleId, team, team);
		}
	}

	public static void ProcessMultiAssignCombRole(
		IGuesserRoleInfoContainer container,
		byte id,
		CombinationRoleManagerBase roleMng)
	{
		foreach (var role in roleMng.Roles)
		{
			ExtremeRoleType team = role.Core.Team;
			if (container.SeparatedRoleId.TryGetValue(team, out var list))
			{
				container.ListAdd(role.Core.Id, team, list);
			}
		}

		if (IsInvestigatorOffice(id))
		{
			if (container.SeparatedRoleId.TryGetValue(ExtremeRoleType.Crewmate, out var crewList))
			{
				container.ListAdd(ExtremeRoleId.InvestigatorApprentice, ExtremeRoleType.Crewmate, crewList);
			}
		}
	}

	public static void ProcessSingleAssignCombRole(
		IGuesserRoleInfoContainer container,
		byte id,
		CombinationRoleManagerBase roleMng)
	{
		foreach (var role in roleMng.Roles)
		{
			container.Add(role.Core.Id, role.Core.Team);
		}

		if (IsInvestigatorOffice(id))
		{
			container.Add(ExtremeRoleId.InvestigatorApprentice, ExtremeRoleType.Crewmate);
		}
	}

	public static bool IsInvestigatorOffice(byte checkId)
		=> checkId == (byte)CombinationRoleType.InvestigatorOffice;
}
