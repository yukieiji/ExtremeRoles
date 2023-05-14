using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign.Model;
public sealed class RoleAssignFilterModel
{
    public AddRoleMenuModel AddRoleMenu { get; set; }
    public Dictionary<Guid, RoleFilterSetModel> FilterSet { get; set; }
}
