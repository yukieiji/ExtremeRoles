using ExtremeRoles.Roles.API;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles.API.Interface;

public interface IGhostRoleInfoContainer
{
	public IReadOnlyDictionary<ExtremeGhostRoleId, GhostRoleCore> Core { get; }
	public IReadOnlyDictionary<ExtremeGhostRoleId, Type> OptionBuilder { get; }

	public IReadOnlyDictionary<ExtremeGhostRoleId, Type> Role { get; }
}
