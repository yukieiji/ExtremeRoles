using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign.Model;
public struct RoleAssignFilterModel
{
    public int CurCount { get; set; }
    public List<RoleFilterSetModel> FilterSet { get; set; }
}
