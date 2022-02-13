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
            for (int j = 0; j < __instance.ViewPorts.Length; j++)
            {
                __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                __instance.SabText[j].text = Helper.Translation.GetString("youDonotUse");
                __instance.SabText[j].gameObject.SetActive(true);
            }

            return false;
        }
    }
}
