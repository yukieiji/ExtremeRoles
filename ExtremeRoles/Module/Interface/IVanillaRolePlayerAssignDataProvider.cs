using ExtremeRoles.Module.RoleAssign;
using System.Collections.Generic;

namespace ExtremeRoles.Module.Interface;

public interface IVanillaRolePlayerAssignDataProvider
{
	public IEnumerable<VanillaRolePlayerAssignData> Data { get; } 
}
