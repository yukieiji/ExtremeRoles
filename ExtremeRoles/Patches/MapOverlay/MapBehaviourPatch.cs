using HarmonyLib;

namespace ExtremeRoles.Patches.MapOverlay;

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.GenericShow))]
public static class MapBehaviourShowNormalMapPatch
{
    static void Prefix()
    {
		InfoOverlay.Instance.Hide();
	}
}
