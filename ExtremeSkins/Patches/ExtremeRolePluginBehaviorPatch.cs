using HarmonyLib;
using UnityEngine;

using ExtremeRoles;
using ExtremeRoles.Helper;
using ExtremeSkins.Patches.AmongUs;

namespace ExtremeSkins.Patches
{
    [HarmonyPatch(
        typeof(ExtremeRolePluginBehavior),
        nameof(ExtremeRolePluginBehavior.Update))]
    public static class ExtremeRolePluginBehaviorPatch
    {
        public static void Postfix()
        {
            if (Key.IsAltDown() &&
                Input.GetKeyDown(KeyCode.F12))
            {
                CreatorModeManager.Instance.SwitchMode();
                VersionShowerStartPatch.UpdateText();
            }
        }
    }
}
