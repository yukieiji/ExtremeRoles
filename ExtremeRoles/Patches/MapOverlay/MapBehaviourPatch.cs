using HarmonyLib;

using UnityEngine;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

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
        public static bool Prefix()
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            return 
                Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseAdmin() ||
                MapCountOverlayUpdatePatch.IsAbilityUse();
        }
    }
}
