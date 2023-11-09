using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(SkinLayer), nameof(SkinLayer.IsPlayingRunAnim))]
public static class SkinLayerIsPlayingRunAnimPatch
{
	public static bool Prefix(SkinLayer __instance, ref bool __result)
	{
		__result = false;
		return __instance.skin != null && __instance.animator != null;
	}
}