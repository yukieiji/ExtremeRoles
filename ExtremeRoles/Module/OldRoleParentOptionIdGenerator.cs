using ExtremeRoles.Extension;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Rewired.Utils.Classes.Data;
using System;

namespace ExtremeRoles.Module;

public sealed class OldRoleParentOptionIdGenerator : IRoleParentOptionIdGenerator
{
	public int Get(ExtremeRoleId id)
		=> ExtremeRoleManager.GetRoleGroupId(id);

	public int Get(ExtremeGhostRoleId id)
		=> ExtremeGhostRoleManager.GetRoleGroupId(id);

	public int Get(CombinationRoleType roleId)
		=> ExtremeRoleManager.GetCombRoleGroupId(roleId);
}
