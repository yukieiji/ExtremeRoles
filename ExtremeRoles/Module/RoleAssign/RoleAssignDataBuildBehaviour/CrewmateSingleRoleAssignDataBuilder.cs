using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class CrewmateSingleRoleAssignDataBuilder(
	IVanillaRoleProvider roleProvider,
	ISingleRoleAssignHelper helper,
	IModLogger logger) : ICrewmateSingleRoleAssignDataBuilder
{
	private readonly IReadOnlySet<RoleTypes> vanillaCrewRoleType = roleProvider.CrewmateRole;

	public void Build(in PreparationData data)
	{
		logger.LogTrace("------------------------- SingleRoleAssign - Crewmate - Start -------------------------");
		helper.AddSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Crewmate,
			data.Assign.GetCanCrewmateAssignPlayer(),
			vanillaCrewRoleType);
		logger.LogTrace("------------------------- SingleRoleAssign - Crewmate - End -------------------------");
	}
}
