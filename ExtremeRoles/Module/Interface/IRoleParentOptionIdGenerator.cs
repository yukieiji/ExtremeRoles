using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.Interface;

public interface IRoleParentOptionIdGenerator
{
	public int Get(ExtremeRoleId id);
	public int Get(ExtremeGhostRoleId id);
	public int Get(CombinationRoleType roleId);
}
