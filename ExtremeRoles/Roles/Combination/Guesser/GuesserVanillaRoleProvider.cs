using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination.Guesser;

public sealed class GuesserVanillaRoleProvider : IGuesserVanillaRoleProvider
{
	public void AddVanillaRoles(
		IGuesserRoleInfoContainer container,
		bool includeNoneRole,
		Guesser.DefaultGuessRole defaultRole,
		bool liberalOn,
		bool militantOn)
	{
		if (!includeNoneRole)
		{
			return;
		}

		AddCrewmateDefaultRole(container, defaultRole);
		AddImpostorDefaultRole(container, defaultRole);

		if (!liberalOn)
		{
			return;
		}

		AddDoveDefaultRole(container, defaultRole);
		AddMilitantDefaultRole(container, defaultRole, militantOn);
	}

	public void AddAmongUsRoles(IGuesserRoleInfoContainer container)
	{
		var roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;

		foreach (RoleTypes role in Enum.GetValues(typeof(RoleTypes)))
		{
			if (ShouldSkipVanillaRole(role))
			{
				continue;
			}

			if (roleOptions.GetChancePerGame(role) > 0)
			{
				ProcessAmongUsRole(container, role);
			}
		}
	}

	public static bool ShouldSkipVanillaRole(RoleTypes role)
	{
		return VanillaRoleProvider.IsDefaultCrewmateRole(role) ||
			VanillaRoleProvider.IsDefaultImpostorRole(role) ||
			role is
				RoleTypes.GuardianAngel or
				RoleTypes.CrewmateGhost or
				RoleTypes.ImpostorGhost or
				RoleTypes.SpiritGuide;
	}

	public static void ProcessAmongUsRole(IGuesserRoleInfoContainer container, RoleTypes role)
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

		container.Add((ExtremeRoleId)role, team);
		if (team != ExtremeRoleType.Null)
		{
			container.AddSeparatedRoleId(team, (ExtremeRoleId)role);
		}
	}

	public static void AddCrewmateDefaultRole(
		IGuesserRoleInfoContainer container,
		Guesser.DefaultGuessRole defaultRole)
	{
		if (defaultRole.HasFlag(Guesser.DefaultGuessRole.Crewmate))
		{
			container.Add((ExtremeRoleId)RoleTypes.Crewmate, ExtremeRoleType.Crewmate);
		}
	}

	public static void AddImpostorDefaultRole(
		IGuesserRoleInfoContainer container,
		Guesser.DefaultGuessRole defaultRole)
	{
		if (defaultRole.HasFlag(Guesser.DefaultGuessRole.Impostor))
		{
			container.Add((ExtremeRoleId)RoleTypes.Impostor, ExtremeRoleType.Impostor);
		}
	}

	public static void AddDoveDefaultRole(
		IGuesserRoleInfoContainer container,
		Guesser.DefaultGuessRole defaultRole)
	{
		if (defaultRole.HasFlag(Guesser.DefaultGuessRole.Dove))
		{
			container.Add(ExtremeRoleId.Dove, ExtremeRoleType.Liberal);
		}
	}

	public static void AddMilitantDefaultRole(
		IGuesserRoleInfoContainer container,
		Guesser.DefaultGuessRole defaultRole,
		bool militantOn)
	{
		if (militantOn && defaultRole.HasFlag(Guesser.DefaultGuessRole.Militant))
		{
			container.Add(ExtremeRoleId.Militant, ExtremeRoleType.Liberal);
		}
	}
}
