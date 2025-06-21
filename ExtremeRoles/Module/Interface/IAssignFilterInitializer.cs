using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.Interface;

public interface IAssignFilterInitializer
{
    void Initialize(RoleAssignFilter filter, PreparationData data);
}
