using ExtremeRoles.Compat.Initializer;

#nullable enable

namespace ExtremeRoles.Compat.ModIntegrator;

public sealed class CrowdedMod(CrowedModInitializer init) : ModIntegratorBase(init)
{
	public int MaxPlayerNum { get; } = init.MaxPlayerNum;
	public const string Guid = "xyz.crowdedmods.crowdedmod";
}