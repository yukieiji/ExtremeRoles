using AmongUs.GameOptions;
using ExtremeRoles.Module.Interface;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class VanillaRoleProvider : IVanillaRoleProvider
{
	public IReadOnlySet<RoleTypes> CrewmateRole { get; }

	public IReadOnlySet<RoleTypes> ImpostorRole { get; }

	public IReadOnlySet<RoleTypes> AllCrewmate { get; }

	public IReadOnlySet<RoleTypes> AllImpostor { get; }

	public VanillaRoleProvider()
	{
		var allCrew = new HashSet<RoleTypes>(6);
		var crewRole = new HashSet<RoleTypes>(6);

		var allImp = new HashSet<RoleTypes>(6);
		var impRole = new HashSet<RoleTypes>(6);

		foreach (var role in Enum.GetValues<RoleTypes>())
		{
			if (IsCrewmateRole(role))
			{
				allCrew.Add(role);
			}
			if (IsCrewmateAdditionalRole(role))
			{
				crewRole.Add(role);
			}
			if (IsImpostorRole(role))
			{
				allImp.Add(role);
			}
			if (IsImpostorAdditionalRole(role))
			{
				impRole.Add(role);
			}
		}

		AllCrewmate = allCrew;
		CrewmateRole = crewRole;

		ImpostorRole = impRole;
		AllImpostor = allImp;
	}

	public static bool IsCrewmateRole(RoleTypes role)
		=> IsDefaultCrewmateRole(role) || IsCrewmateAdditionalRole(role);
	public static bool IsDefaultCrewmateRole(RoleTypes role)
		=> role is RoleTypes.Crewmate;

	public static bool IsCrewmateAdditionalRole(RoleTypes role)
		=> role is
			RoleTypes.Engineer or
			RoleTypes.Scientist or
			RoleTypes.Noisemaker or
			RoleTypes.Tracker or
			RoleTypes.Detective;

	public static bool IsImpostorRole(RoleTypes role)
		=> IsDefaultImpostorRole(role) || IsImpostorAdditionalRole(role);

	public static bool IsDefaultImpostorRole(RoleTypes role)
		=> role is RoleTypes.Impostor;

	public static bool IsImpostorAdditionalRole(RoleTypes role)
		=> role is
			RoleTypes.Shapeshifter or
			RoleTypes.Phantom or
			RoleTypes.Viper;
}
