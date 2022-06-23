using HarmonyLib;

using UnityEngine;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.MapOverlay
{
    [HarmonyPatch(typeof(MapBehaviour), "IsOpenStopped", MethodType.Getter)]
    public static class MapBehaviourIsOpenStoppedPatch
    {
        public static bool Prefix(
            MapBehaviour __instance,
            ref bool __result)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var admin = Roles.ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
                Roles.Solo.Crewmate.Supervisor>();
            
            if (admin == null) { return true; }
            if (!admin.Button.IsAbilityActive()) { return true; }

            __result = false;
            return false;
        }
    }

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
            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseAdmin) { return true; }

            __instance.GenericShow();
            __instance.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
            __instance.taskOverlay.Hide();
            __instance.HerePoint.enabled = false;

            FastDestroyableSingleton<HudManager>.Instance.SetHudActive(false);
            
            return false;
        }
    }
}
