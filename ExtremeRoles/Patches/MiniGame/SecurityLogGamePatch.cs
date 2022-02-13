using System;
using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{

    [HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
    public class SecurityLogGameUpdatePatch
    {
        public static bool Prefix(SecurityLogGame __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseSecurity) { return true; }

            __instance.EntryPool.ReclaimAll();
            __instance.SabText.text = Helper.Translation.GetString("youDonotUse");
            __instance.SabText.gameObject.SetActive(true);

            return false;
        }
    }
}
