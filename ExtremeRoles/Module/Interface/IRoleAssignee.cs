using System.Collections;

namespace ExtremeRoles.Module.Interface;

public interface IRoleAssignee
{
	public IEnumerator CoRpcAssign();

	public void RpcAssignToExRole();
}
