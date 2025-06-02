using AmongUs.GameOptions;
using ExtremeRoles.Module.Interface;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class VanillaRoleProvider : IVanillaRoleProvider
{
	public IReadOnlySet<RoleTypes> CrewmateRole => new HashSet<RoleTypes> { RoleTypes.Engineer, RoleTypes.Scientist, RoleTypes.Noisemaker, RoleTypes.Tracker };

	public IReadOnlySet<RoleTypes> ImpostorRole => new HashSet<RoleTypes> { RoleTypes.Shapeshifter, RoleTypes.Phantom };

	public IReadOnlySet<RoleTypes> AllCrewmate => new HashSet<RoleTypes>(CrewmateRole) { RoleTypes.Crewmate };

	public IReadOnlySet<RoleTypes> AllImpostor => new HashSet<RoleTypes>(ImpostorRole) { RoleTypes.Impostor };
}
