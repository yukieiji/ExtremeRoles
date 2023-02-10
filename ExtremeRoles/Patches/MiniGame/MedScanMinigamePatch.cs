using HarmonyLib;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
    public static class MedScanMinigameFixedUpdatePatch
    {
        public static void Prefix(MedScanMinigame __instance)
        {
            if (ExtremeGameModeManager.Instance.ShipOption.IsAllowParallelMedbayScan)
            {
                __instance.medscan.CurrentUser = CachedPlayerControl.LocalPlayer.PlayerId;
                __instance.medscan.UsersList.Clear();
            }
        }
    }
}
