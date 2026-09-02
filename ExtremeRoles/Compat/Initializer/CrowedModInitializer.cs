using BepInEx;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace ExtremeRoles.Compat.Initializer;

public sealed class CrowedModInitializer(PluginInfo plugin, IAccessTool accessTool, IHarmonyPatch patch) : InitializerBase<CrowdedMod>(plugin, accessTool, patch)
{
	public int MaxPlayerNum { get; private set; }

	protected override void PatchAll(IAccessTool accessTool, IHarmonyPatch patch)
	{
		var meetingHud = GetClass("MeetingHudPagingBehaviour");
		var targets = accessTool.GetProperty(meetingHud, "Targets").GetGetMethod();

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
		var sortPostfixMethod =
			 SymbolExtensions.GetMethodInfo(() => Patches.CrowedModPatch.Sort(ref ienum));

		patch.Patch(update, new HarmonyMethod(monikaCheckPrefixMethod));
		patch.Patch(onPageChanged, new HarmonyMethod(monikaCheckPrefixMethod));
		patch.Patch(targets, postfix: new HarmonyMethod(sortPostfixMethod));
	}
}
