using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles.API.Interface;

public interface IGhostRoleCoreProvider
{
	public IEnumerable<KeyValuePair<ExtremeGhostRoleId, GhostRoleCore>> All { get; }

	public GhostRoleCore Get(ExtremeGhostRoleId id);
}
