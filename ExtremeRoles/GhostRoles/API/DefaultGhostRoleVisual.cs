using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Helper;

namespace ExtremeRoles.GhostRoles.API;

public sealed class DefaultGhostRoleVisual(GhostRoleCore core) : IGhostRoleVisual
{
	private readonly GhostRoleCore core = core;
	public string ColoredRoleName => GetDefaultColoredRoleName(this.core);

	public string GetDefaultColoredRoleName(GhostRoleCore core)
		=> Design.ColoredString(core.Color, Tr.GetString(core.Name));
}
