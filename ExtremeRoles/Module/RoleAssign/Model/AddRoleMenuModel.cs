using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.RoleAssign.Model;

public sealed class AddRoleMenuModel
{
    public int FilterId { get; set; }
    public List<int> Id { get; set; }

    public Dictionary<int, ExtremeRoleId> NormalRole { get; set; }

    public Dictionary<int, CombinationRoleType> CombRole { get; set; }

    public Dictionary<int, ExtremeGhostRoleId> GhostRole { get; set; }
}
