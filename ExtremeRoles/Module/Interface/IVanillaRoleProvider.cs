using AmongUs.GameOptions;
using System.Collections.Generic;

namespace ExtremeRoles.Module.Interface;

public interface IVanillaRoleProvider
{
	public IReadOnlySet<RoleTypes> CrewmateRole { get; }
	public IReadOnlySet<RoleTypes> ImpostorRole { get; }
}
