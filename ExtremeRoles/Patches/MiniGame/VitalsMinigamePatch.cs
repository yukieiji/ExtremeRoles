using HarmonyLib;


namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    public class VitalsMinigameUpdatePatch
    {
        public static bool Prefix(VitalsMinigame __instance)
        {

            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            if (Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseVital) { return true; }

            __instance.SabText.text = Helper.Translation.GetString("youDonotUse");

            __instance.SabText.gameObject.SetActive(true);
            for (int j = 0; j < __instance.vitals.Length; j++)
            {
                __instance.vitals[j].gameObject.SetActive(false);
            }

            return false;
        }
    }
}
