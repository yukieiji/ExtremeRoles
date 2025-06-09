using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.Interface;

public interface IRoleAssignDataBuildBehaviour
{
	public int Priority { get; }

	public void Build(in PreparationData data);
}
