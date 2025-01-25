using BepInEx;

using HarmonyLib;

using ExtremeRoles.Compat.ModIntegrator;

namespace ExtremeRoles.Compat.Initializer;

public class EmptyInitializer<T>(PluginInfo plugin) : InitializerBase<T>(plugin)
	where T : ModIntegratorBase
{
	protected override void PatchAll(Harmony patch)
	{
	}
}
