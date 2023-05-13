using System.Collections.Generic;

using ExtremeRoles.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Module.RoleAssign.Model;

public sealed class AddRoleMenuModel
{
    public RoleFilterSetProperty Property { get; set; }
    public RoleFilterSetModel Filter { get; set; }
    public List<int> Id { get; set; }

    public Dictionary<int, ExtremeRoleId> NormalRole { get; set; }

    public Dictionary<int, CombinationRoleType> CombRole { get; set; }

    public Dictionary<int, ExtremeGhostRoleId> GhostRole { get; set; }
}
