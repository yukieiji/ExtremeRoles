using ExtremeRoles.Roles.API.Interface.Team;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Roles.API;

public readonly record struct RoleArgs(RoleCore Core, RoleProp Prop, ITeam? Team = null)
{
	public static RoleArgs BuildImpostor(ExtremeRoleId id, RoleProp prop = RolePropPresets.ImpostorDefault)
		=> new RoleArgs(RoleCore.BuildImpostor(id), prop);

	public static RoleArgs BuildCrewmate(ExtremeRoleId id, Color color, RoleProp prop = RolePropPresets.CrewmateDefault)
		=> new RoleArgs(RoleCore.BuildCrewmate(id, color), prop);

	public static RoleArgs BuildLiberalDove(ExtremeRoleId id, RoleProp prop = RolePropPresets.LiberalDoveDefault)
		=> new RoleArgs(RoleCore.BuildLiberal(id), prop);

	public static RoleArgs BuildLiberalMilitant(ExtremeRoleId id, RoleProp prop = RolePropPresets.LiberalMilitantDefault)
		=> new RoleArgs(RoleCore.BuildLiberal(id), prop);

	public static RoleArgs BuildNeutral(ExtremeRoleId id, Color color, RoleProp prop, ITeam? team = null)
	{
		var core = RoleCore.BuildNeutral(id, color);
		team ??= new DefaultNeutralTeam(core);

		return new RoleArgs(core, prop, team);
	}

	public static RoleArgs BuildDefaultTeamNeutral(ExtremeRoleId id, Color color, RoleProp prop)
		=> new RoleArgs(RoleCore.BuildNeutral(id, color), prop);
}
