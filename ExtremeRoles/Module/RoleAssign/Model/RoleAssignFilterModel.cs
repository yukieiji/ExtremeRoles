using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign.Model;
public struct RoleAssignFilterModel
{
    public int FilterId { get; set; }
    public Dictionary<int, RoleFilterSetModel> FilterSet { get; set; }
}
