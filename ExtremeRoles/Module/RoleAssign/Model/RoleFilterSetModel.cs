using System.Collections.Generic;
using System.Runtime.Serialization;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.RoleAssign.Model;

[DataContract]
public sealed class RoleFilterSetModel
{
    [DataMember]
    public int AssignNum { get; set; } = 1;

    [DataMember]
    public Dictionary<int, ExtremeRoleId> FilterNormalId { get; set; }

    [DataMember]
    public Dictionary<int, CombinationRoleType> FilterCombinationId { get; set; }

    [DataMember] 
    public Dictionary<int, ExtremeGhostRoleId> FilterGhostRole { get; set; }
}
