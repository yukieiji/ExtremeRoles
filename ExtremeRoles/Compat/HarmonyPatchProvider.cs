using BepInEx;
using HarmonyLib;

using ExtremeRoles.Compat.Interface;

namespace ExtremeRoles.Compat;

public class HarmonyPatchProvider : IHarmonyPatchProvider
{
	public IHarmonyPatch Get(PluginInfo plugin)
	{
		var harmony = new Harmony($"ExR.{plugin.Metadata.GUID}.Patch");
		return new HarmonyPatchWrapper(harmony);
	}
}
