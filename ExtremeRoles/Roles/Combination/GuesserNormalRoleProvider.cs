using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Neutral.Jackal;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserNormalRoleProvider : IGuesserNormalRoleProvider
{
	public void AddExRNormalRoles(
		IGuesserRoleInfoContainer container,
		out GuesserNormalRoleAssignState assignState)
	{
		assignState = new GuesserNormalRoleAssignState();

		foreach (var (id, role) in ExtremeRoleManager.NormalRole)
		{
			var loader = role.Loader;

			int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.SpawnRate);
			int roleNum = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.RoleNum);

			if (spawnOptSel < 1 || roleNum <= 0)
			{
				continue;
			}

			ExtremeRoleId exId = (ExtremeRoleId)id;
			ExtremeRoleType team = role.Core.Team;

			ProcessNormalRole(container, exId, team, assignState);
		}

		ProcessJackalAndSidekick(container, assignState);
		ProcessQueenAndServant(container, assignState);
	}

	public static void ProcessNormalRole(
		IGuesserRoleInfoContainer container,
		ExtremeRoleId exId,
		ExtremeRoleType team,
		GuesserNormalRoleAssignState assignState)
	{
		// Queen and Jackal are added later in neutralized list
		if (exId != ExtremeRoleId.Queen && exId != ExtremeRoleId.Jackal)
		{
			container.Add(exId, team);
			container.AddSeparatedRoleId(team, exId);
		}

		switch (exId)
		{
			case ExtremeRoleId.Jackal:
				ProcessJackal(assignState);
				break;
			case ExtremeRoleId.Queen:
				ProcessQueen(assignState);
				break;
			case ExtremeRoleId.Hypnotist:
				ProcessHypnotist(container);
				break;
			case ExtremeRoleId.Jailer:
				ProcessJailer(container);
				break;
			case ExtremeRoleId.Tucker:
				ProcessTucker(container);
				break;
			default:
				break;
		}
	}

	public static void ProcessJackal(GuesserNormalRoleAssignState assignState)
	{
		assignState.IsJackalOn = true;
		assignState.IsJackalForceReplaceLover = OptionManager.Instance.TryGetCategory(
			OptionTab.NeutralTab,
			ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jackal),
			out var cate) && cate.GetValue<JackalRole.JackalOption, bool>(JackalRole.JackalOption.ForceReplaceLover);
	}

	public static void ProcessQueen(GuesserNormalRoleAssignState assignState)
	{
		assignState.IsQueenOn = true;
	}

	public static void ProcessHypnotist(IGuesserRoleInfoContainer container)
	{
		container.Add(ExtremeRoleId.Doll, ExtremeRoleType.Impostor);
		container.AddSeparatedRoleId(ExtremeRoleType.Neutral, ExtremeRoleId.Doll);
	}

	public static void ProcessJailer(IGuesserRoleInfoContainer container)
	{
		container.Add(ExtremeRoleId.Yardbird, ExtremeRoleType.Crewmate);
		if (OptionManager.Instance.TryGetCategory(
				OptionTab.CrewmateTab,
				ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jailer),
				out var jailer) &&
			!jailer.GetValue<Jailer.Option, bool>(Jailer.Option.IsMissingToDead))
		{
			container.Add(ExtremeRoleId.Lawbreaker, ExtremeRoleType.Neutral);
		}
	}

	public static void ProcessJailerDirect(IGuesserRoleInfoContainer container, bool isMissingToDead)
	{
		container.Add(ExtremeRoleId.Yardbird, ExtremeRoleType.Crewmate);
		if (!isMissingToDead)
		{
			container.Add(ExtremeRoleId.Lawbreaker, ExtremeRoleType.Neutral);
		}
	}

	public static void ProcessTucker(IGuesserRoleInfoContainer container)
	{
		container.Add(ExtremeRoleId.Chimera, ExtremeRoleType.Neutral);
		container.AddSeparatedRoleId(ExtremeRoleType.Neutral, ExtremeRoleId.Chimera);
	}

	public static void ProcessJackalAndSidekick(
		IGuesserRoleInfoContainer container,
		GuesserNormalRoleAssignState assignState)
	{
		if (!assignState.IsJackalOn)
		{
			return;
		}

		container.Add(ExtremeRoleId.Jackal, ExtremeRoleType.Neutral);
		container.Add(ExtremeRoleId.Sidekick, ExtremeRoleType.Neutral);

		container.AddSeparatedRoleId(ExtremeRoleType.Neutral, ExtremeRoleId.Jackal);
		container.AddSeparatedRoleId(ExtremeRoleType.Neutral, ExtremeRoleId.Sidekick);
	}

	public static void ProcessQueenAndServant(
		IGuesserRoleInfoContainer container,
		GuesserNormalRoleAssignState assignState)
	{
		if (!assignState.IsQueenOn)
		{
			return;
		}

		ExtremeRoleType queenTeam = ExtremeRoleType.Neutral;
		container.Add(ExtremeRoleId.Queen, queenTeam);

		ExtremeRoleId servantId = ExtremeRoleId.Servant;
		if (container.SeparatedRoleId.TryGetValue(queenTeam, out var neutralList) && neutralList.Count > 1)
		{
			container.Add(servantId, queenTeam);
		}

		container.ListAddTargetTeam(servantId, queenTeam, ExtremeRoleType.Crewmate);
		container.ListAddTargetTeam(servantId, queenTeam, ExtremeRoleType.Impostor);

		foreach (var (id, roleMng) in ExtremeRoleManager.CombRole)
		{
			var loader = roleMng.Loader;

			int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.SpawnRate);
			int roleNum = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.RoleNum);

			if (spawnOptSel < 1 || roleNum <= 0)
			{
				continue;
			}
			if (roleMng is FlexibleCombinationRoleManagerBase flexMng)
			{
				container.Add(flexMng.BaseRole.Core.Id, queenTeam, servantId);
			}
			else
			{
				foreach (var role in roleMng.Roles)
				{
					if (role.IsNeutral())
					{
						continue;
					}

					container.Add(role.Core.Id, queenTeam, servantId);
				}
			}

			if (IsInvestigatorOffice(id))
			{
				container.Add(ExtremeRoleId.InvestigatorApprentice, queenTeam, servantId);
			}
		}

		container.AddSeparatedRoleId(queenTeam, ExtremeRoleId.Queen);
		container.AddSeparatedRoleId(queenTeam, servantId);
	}

	public static bool IsInvestigatorOffice(byte checkId)
		=> checkId == (byte)CombinationRoleType.InvestigatorOffice;
}
