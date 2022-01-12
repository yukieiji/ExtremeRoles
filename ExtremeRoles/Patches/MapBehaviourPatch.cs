using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.GenericShow))]
    class MapBehaviourShowNormalMapPatch
    {
        static void Prefix(MapBehaviour __instance)
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();
        }
    }
}
