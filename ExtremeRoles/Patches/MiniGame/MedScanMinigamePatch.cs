using HarmonyLib;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
    public static class MedScanMinigameFixedUpdatePatch
    {
        public static void Prefix(MedScanMinigame __instance)
        {
            if (ExtremeGameManager.Instance.ShipOption.IsAllowParallelMedbayScan)
            {
                __instance.medscan.CurrentUser = CachedPlayerControl.LocalPlayer.PlayerId;
                __instance.medscan.UsersList.Clear();
            }
        }
    }
}
