using ExtremeRoles.Roles;
using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign;

public interface IRoleAssignDataChecker
{
    public IReadOnlySet<ExtremeRoleId> GetNgData(in PreparationData data);
}
