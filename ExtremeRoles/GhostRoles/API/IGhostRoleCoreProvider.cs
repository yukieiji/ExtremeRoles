using ExtremeRoles.Roles.API;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles.API;

public interface IGhostRoleCoreProvider
{
	public IEnumerable<KeyValuePair<ExtremeGhostRoleId, GhostRoleCore>> Core { get; }
}
