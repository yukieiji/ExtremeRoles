using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign.Model;
public sealed class RoleAssignFilterModel
{
    public AddRoleMenuModel AddRoleMenu { get; set; }
    public int FilterId { get; set; }
    public Dictionary<int, RoleFilterSetModel> FilterSet { get; set; }
}
