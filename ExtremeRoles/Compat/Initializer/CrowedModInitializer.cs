using BepInEx;

using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using ExtremeRoles.Compat.ModIntegrator;

namespace ExtremeRoles.Compat.Initializer;

public sealed class CrowedModInitializer(PluginInfo plugin) : InitializerBase<CrowdedMod>(plugin)
{
	public int MaxPlayerNum { get; private set; }

	protected override void PatchAll(Harmony patch)
	{
		var meetingHud = GetClass("MeetingHudPagingBehaviour");
		var targets = AccessTools.Property(meetingHud, "Targets").GetGetMethod();

		var update = GetMethod("MeetingHudPagingBehaviour", "Update");
		var onPageChanged = GetMethod("MeetingHudPagingBehaviour", "OnPageChanged");

		var pluginClass = GetClass("CrowdedModPlugin");
		var maxPlayerField = pluginClass.GetField(
			"MaxPlayers",
			BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

		MaxPlayerNum = (int)maxPlayerField.GetValue(null);

		var monikaCheckPrefixMethod =
			 SymbolExtensions.GetMethodInfo(() => Patches.CrowedModPatch.IsNotMonikaMeeting());

		IEnumerable<PlayerVoteArea> ienum = null;
		var monikaSortPostfixMethod =
			 SymbolExtensions.GetMethodInfo(() => Patches.CrowedModPatch.SortMonikaTrash(ref ienum));

		patch.Patch(update, new HarmonyMethod(monikaCheckPrefixMethod));
		patch.Patch(onPageChanged, new HarmonyMethod(monikaCheckPrefixMethod));
		patch.Patch(targets, postfix: new HarmonyMethod(monikaSortPostfixMethod));
	}
}
