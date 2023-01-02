using HarmonyLib;

using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Patches.MapOverlay
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.GenericShow))]
    public static class MapBehaviourShowNormalMapPatch
    {
        static void Prefix(MapBehaviour __instance)
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();
        }
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowCountOverlay))]
    public static class MapBehaviourShowCountOverlayPatch
    {
        public static bool Prefix(MapBehaviour __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }
            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseAdmin() ||
                MapCountOverlayUpdatePatch.IsAbilityUse()) { return true; }

            __instance.ShowNormalMap();

            return false;
        }
    }
}
