using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
    public class MedScanMinigameFixedUpdatePatch
    {
        public static void Prefix(MedScanMinigame __instance)
        {
            if (OptionHolder.Ship.AllowParallelMedBayScan)
            {
                __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
                __instance.medscan.UsersList.Clear();
            }
        }
    }
}
