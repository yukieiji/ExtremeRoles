using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GhostRoles.API;

public interface IGhostRoleOptionProvider
{
	public Roles.API.GhostRoleCore Get(ExtremeGhostRoleId id);
}
