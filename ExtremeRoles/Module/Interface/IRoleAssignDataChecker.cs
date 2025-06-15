using ExtremeRoles.Roles;
using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign;

public interface IRoleAssignDataChecker
{
    public IReadOnlyList<ExtremeRoleId> GetNgData(in PreparationData data);
}
