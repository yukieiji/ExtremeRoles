using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class RoleOptionCategoryIdGenerator : IRoleOptionCategoryIdGenerator
{
	public int Get(ExtremeRoleId id)
		=> ExtremeRoleManager.GetRoleGroupId(id);

	public int Get(ExtremeGhostRoleId id)
		=> ExtremeGhostRoleManager.GetRoleGroupId(id);

	public int Get(CombinationRoleType roleId)
		=> ExtremeRoleManager.GetCombRoleGroupId(roleId);
}
