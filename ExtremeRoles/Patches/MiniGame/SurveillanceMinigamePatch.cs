using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
    public class SurveillanceMinigameUpdatePatch
    {
        public static bool Prefix(SurveillanceMinigame __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].CanUseSecurity) { return true; }

            __instance.isStatic = true;
            for (int i = 0; i < __instance.ViewPorts.Length; ++i)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                __instance.SabText[i].text = Helper.Translation.GetString("youDonotUse");
                __instance.SabText[i].gameObject.SetActive(true);
            }

            return false;
        }
    }
}
