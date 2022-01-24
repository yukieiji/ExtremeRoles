using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Patches
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
            if (Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseAdmin) { return true; }

            __instance.GenericShow();
            __instance.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
            __instance.taskOverlay.Hide();
            __instance.HerePoint.enabled = false;

            DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
            
            return false;
        }
    }
}
