using ExtremeRoles.GhostRoles.API;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleCoreProvider(IReadOnlyDictionary<ExtremeGhostRoleId, GhostRoleCore> core) : IGhostRoleCoreProvider
{
	public IEnumerable<KeyValuePair<ExtremeGhostRoleId, GhostRoleCore>> Core { get; } = core;
}
