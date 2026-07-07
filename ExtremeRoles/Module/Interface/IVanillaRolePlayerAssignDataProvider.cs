using System.Collections.Generic;

using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.Interface;

public interface IVanillaRolePlayerAssignDataProvider
{
	public IEnumerable<VanillaRolePlayerAssignData> Data { get; } 
}
