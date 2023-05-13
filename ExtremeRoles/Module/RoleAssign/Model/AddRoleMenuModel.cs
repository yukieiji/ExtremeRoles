using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.RoleAssign.Model;

public sealed class AddRoleMenuModel
{
    public RoleFilterSetModel Filter { get; set; }
    public List<int> Id { get; set; }

    public Dictionary<int, ExtremeRoleId> NormalRole { get; set; }

    public Dictionary<int, CombinationRoleType> CombRole { get; set; }

    public Dictionary<int, ExtremeGhostRoleId> GhostRole { get; set; }
}
