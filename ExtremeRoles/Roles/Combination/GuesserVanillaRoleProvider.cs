using System;
using System.Collections.Generic;

using AmongUs.GameOptions;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserVanillaRoleProvider : IGuesserVanillaRoleProvider
{
	public void AddVanillaRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		bool includeNoneRole,
		Guesser.DefaultGuessRole defaultRole,
		bool liberalOn,
		bool militantOn)
	{
		if (!includeNoneRole)
		{
			return;
		}

		if (defaultRole.HasFlag(Guesser.DefaultGuessRole.Crewmate))
		{
			add(result, (ExtremeRoleId)RoleTypes.Crewmate, ExtremeRoleType.Crewmate);
		}
		if (defaultRole.HasFlag(Guesser.DefaultGuessRole.Impostor))
		{
			add(result, (ExtremeRoleId)RoleTypes.Impostor, ExtremeRoleType.Impostor);
		}

		if (!liberalOn)
		{
			return;
		}

		if (defaultRole.HasFlag(Guesser.DefaultGuessRole.Dove))
		{
			add(result, ExtremeRoleId.Dove, ExtremeRoleType.Liberal);
		}
		if (militantOn && defaultRole.HasFlag(Guesser.DefaultGuessRole.Militant))
		{
			add(result, ExtremeRoleId.Militant, ExtremeRoleType.Liberal);
		}
	}

	public void AddAmongUsRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId)
	{
		var roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;

		foreach (RoleTypes role in Enum.GetValues(typeof(RoleTypes)))
		{
			if (VanillaRoleProvider.IsDefaultCrewmateRole(role) ||
				VanillaRoleProvider.IsDefaultImpostorRole(role) ||
				role is
					RoleTypes.GuardianAngel or
					RoleTypes.CrewmateGhost or
					RoleTypes.ImpostorGhost or
					RoleTypes.SpiritGuide)
			{
				continue;
			}
			if (roleOptions.GetChancePerGame(role) > 0)
			{
				ExtremeRoleType team = ExtremeRoleType.Null;
				if (VanillaRoleProvider.IsCrewmateAdditionalRole(role))
				{
					team = ExtremeRoleType.Crewmate;
				}
				else if (VanillaRoleProvider.IsImpostorAdditionalRole(role))
				{
					team = ExtremeRoleType.Impostor;
				}
				add(result, (ExtremeRoleId)role, team);
				if (separatedRoleId.TryGetValue(team, out var list))
				{
					list.Add((ExtremeRoleId)role);
				}
			}
		}
	}

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
}
