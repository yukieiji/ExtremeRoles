using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public interface ISingleRoleAssignHelper
{
	void AddSingleExtremeRoleAssignDataFromTeamAndPlayer(
		in PreparationData data,
		ExtremeRoleType team,
		in IReadOnlyList<VanillaRolePlayerAssignData> targetPlayer,
		in IReadOnlySet<RoleTypes> vanilaTeams);

	IEnumerable<VanillaRolePlayerAssignData> GetAssignablePlayer(PlayerRoleAssignData assignData, ExtremeRoleType targetTeam);
}
