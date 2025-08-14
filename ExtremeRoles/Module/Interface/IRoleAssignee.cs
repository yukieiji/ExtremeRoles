using ExtremeRoles.Module.RoleAssign;
using System.Collections;

namespace ExtremeRoles.Module.Interface;

public interface IRoleAssignee
{
	public PreparationData PreparationData { get; }

	public IEnumerator CoRpcAssign();
}
