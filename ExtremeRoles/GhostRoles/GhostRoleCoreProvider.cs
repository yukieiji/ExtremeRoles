using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleCoreProvider(IGhostRoleInfoContainer info) : IGhostRoleCoreProvider
{
	private readonly IReadOnlyDictionary<ExtremeGhostRoleId, GhostRoleCore> core = info.Core;
	public IEnumerable<KeyValuePair<ExtremeGhostRoleId, GhostRoleCore>> All => this.core;

	public GhostRoleCore Get(ExtremeGhostRoleId id) => this.core[id];
}
