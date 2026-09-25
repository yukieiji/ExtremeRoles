using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class CrewmateSingleRoleAssignDataBuilder(IVanillaRoleProvider roleProvider) : ICrewmateSingleRoleAssignDataBuilder
{
	private readonly IReadOnlySet<RoleTypes> vanillaCrewRoleType = roleProvider.CrewmateRole;

	public void Build(in PreparationData data)
	{
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Crewmate - Start -------------------------");
		SingleRoleAssignHelper.AddSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Crewmate,
			data.Assign.GetCanCrewmateAssignPlayer(),
			vanillaCrewRoleType);
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Crewmate - End -------------------------");
	}
}
