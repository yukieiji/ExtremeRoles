using System.Collections.Generic;

using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.Interface;

public interface IRoleAssignDataBuilder
{
	public IReadOnlyList<IPlayerToExRoleAssignData> Build(in PreparationData prepareData);
}
