using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.Interface;

public interface IRoleParentOptionIdGenerator
{
	public int Get(ExtremeRoleId id);
	public int Get(ExtremeGhostRoleId id);
	public int Get(CombinationRoleType roleId);
}
