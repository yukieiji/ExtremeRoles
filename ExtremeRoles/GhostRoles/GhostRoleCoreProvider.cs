using ExtremeRoles.GhostRoles.API;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleCoreProvider(IReadOnlyDictionary<ExtremeGhostRoleId, GhostRoleCore> core) : IGhostRoleCoreProvider
{
	private readonly IReadOnlyDictionary<ExtremeGhostRoleId, GhostRoleCore> core = core;
	public IEnumerable<KeyValuePair<ExtremeGhostRoleId, GhostRoleCore>> All => this.core;

	public GhostRoleCore Get(ExtremeGhostRoleId id) => this.core[id];
}
