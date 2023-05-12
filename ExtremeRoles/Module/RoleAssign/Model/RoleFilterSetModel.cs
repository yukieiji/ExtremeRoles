using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.RoleAssign.Model;

public struct RoleFilterSetModel
{
    public int Id { get; set; }
    public Dictionary<int, ExtremeRoleId> FilterNormalId { get; set; }
    public Dictionary<int, CombinationRoleType> FilterCombinationId { get; set; }
    public Dictionary<int, ExtremeGhostRoleId> FilterGhostRole { get; set; }
}
