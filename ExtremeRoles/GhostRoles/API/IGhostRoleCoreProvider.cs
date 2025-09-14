using ExtremeRoles.Roles.API;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles.API;

public interface IGhostRoleCoreProvider
{
	public IEnumerable<KeyValuePair<ExtremeGhostRoleId, GhostRoleCore>> All { get; }

	public GhostRoleCore Get(ExtremeGhostRoleId id);
}
