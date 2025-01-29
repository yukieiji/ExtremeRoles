using BepInEx;

using HarmonyLib;

using ExtremeRoles.Compat.ModIntegrator;

namespace ExtremeRoles.Compat.Initializer;

public sealed class CrowedModInitializer(PluginInfo plugin) : InitializerBase<CrowdedMod>(plugin)
{
	protected override void PatchAll(Harmony patch)
	{
		var update = GetMethod("MeetingHudPagingBehaviour", "Update");
		var onPageChanged = GetMethod("MeetingHudPagingBehaviour", "OnPageChanged");

		var prefixMethod =
			 SymbolExtensions.GetMethodInfo(() => Patches.CrowedModPatch.IsNotMonikaMeeting());

		patch.Patch(update, new HarmonyMethod(prefixMethod));
		patch.Patch(onPageChanged, new HarmonyMethod(prefixMethod));
	}
}
