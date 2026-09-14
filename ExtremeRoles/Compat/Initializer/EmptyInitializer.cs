using BepInEx;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using HarmonyLib;

namespace ExtremeRoles.Compat.Initializer;

public class EmptyInitializer<T>(PluginInfo plugin, IAccessTool accessTool, IHarmonyPatch patch) : InitializerBase<T>(plugin, accessTool, patch)
	where T : ModIntegratorBase
{
	protected override void PatchAll(IAccessTool accessTool, IHarmonyPatch patch)
	{
	}
}
