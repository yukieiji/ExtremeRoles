using ExtremeRoles.GhostRoles.API;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles;

public sealed class GhostRoleOptionBuilderProvider(
	IReadOnlyDictionary<ExtremeGhostRoleId, IGhostRoleOptionBuilder> builder) : IGhostRoleOptionBuilderProvider
{
	private readonly IReadOnlyDictionary<ExtremeGhostRoleId, IGhostRoleOptionBuilder> builder = builder;
	public IGhostRoleOptionBuilder Get(ExtremeGhostRoleId id) => builder[id];
}
