using HarmonyLib;

namespace ExtremeRoles.Patches.MapOverlay;

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.GenericShow))]
public static class MapBehaviourShowNormalMapPatch
{
    static void Prefix(MapBehaviour __instance)
    {
        ExtremeRolesPlugin.Info.HideInfoOverlay();
    }
}
