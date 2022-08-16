using HarmonyLib;
using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Patches.MiniGame
{

    [HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
    public static class SecurityLogGameUpdatePatch
    {
        public static bool Prefix(SecurityLogGame __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity()) { return true; }

            __instance.EntryPool.ReclaimAll();
            __instance.SabText.text = Helper.Translation.GetString("youDonotUse");
            __instance.SabText.gameObject.SetActive(true);

            return false;
        }
        public static void Postfix(SurveillanceMinigame __instance)
        {
            SecurityHelper.PostUpdate(__instance);
        }
    }
}
