using System.Collections.Generic;

namespace ExtremeRoles.Module.Interface;

public interface IRoleAssignDataBuilder
{
	public IReadOnlyList<IPlayerToExRoleAssignData> Build();
}
