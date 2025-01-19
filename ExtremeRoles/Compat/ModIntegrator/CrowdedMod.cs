using ExtremeRoles.Compat.Interface;

#nullable enable

namespace ExtremeRoles.Compat.ModIntegrator;

public sealed class CrowdedMod(IInitializer init) : ModIntegratorBase(init)
{
	public const string Guid = "CrowdedMod";
}