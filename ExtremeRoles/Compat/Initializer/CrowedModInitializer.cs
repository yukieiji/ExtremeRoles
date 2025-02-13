using BepInEx;

using System.Reflection;

using HarmonyLib;

using ExtremeRoles.Compat.ModIntegrator;

namespace ExtremeRoles.Compat.Initializer;

public sealed class CrowedModInitializer(PluginInfo plugin) : InitializerBase<CrowdedMod>(plugin)
{
	public int MaxPlayerNum { get; private set; }

	protected override void PatchAll(Harmony patch)
	{
		var update = GetMethod("MeetingHudPagingBehaviour", "Update");
		var onPageChanged = GetMethod("MeetingHudPagingBehaviour", "OnPageChanged");

		var pluginClass = GetClass("CrowdedModPlugin");
		var maxPlayerField = pluginClass.GetField(
			"MaxPlayers",
			BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

		MaxPlayerNum = (int)maxPlayerField.GetValue(null);

		var prefixMethod =
			 SymbolExtensions.GetMethodInfo(() => Patches.CrowedModPatch.IsNotMonikaMeeting());

		patch.Patch(update, new HarmonyMethod(prefixMethod));
		patch.Patch(onPageChanged, new HarmonyMethod(prefixMethod));
	}
}
