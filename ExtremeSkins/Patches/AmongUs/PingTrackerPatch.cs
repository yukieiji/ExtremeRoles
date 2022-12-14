using HarmonyLib;
using TMPro;

namespace ExtremeSkins.Patches.AmongUs
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerUpdatePatch
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.alignment = TextAlignmentOptions.TopRight;
            __instance.text.text = string.Concat(
                __instance.text.text, "\n",
                CreatorModeManager.Instance.StatusString);
        }
    }
}
