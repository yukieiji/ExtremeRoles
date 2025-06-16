using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.Interface;

public interface IRoleAssignValidator
{
    bool IsReBuild(in PreparationData data);
}
