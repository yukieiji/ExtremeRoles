using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GhostRoles.API;

public sealed class GhostTeam(ExtremeRoleType team) : IGhostTeam
{
	public ExtremeRoleType Id { get; } = team;

	public bool IsCrewmate() => Id == ExtremeRoleType.Crewmate;

	public bool IsImpostor() => Id == ExtremeRoleType.Impostor;

	public bool IsNeutral() => Id == ExtremeRoleType.Neutral;
}
