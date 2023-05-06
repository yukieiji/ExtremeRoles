using HarmonyLib;
using TMPro;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTrackerUpdatePatch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(PingTracker __instance)
    {
        __instance.text.alignment = TextAlignmentOptions.TopRight;
        __instance.text.text = string.Concat(
            ExtremeVoiceEnginePlugin.Instance.ToString(),
            "\n", __instance.text.text);
    }
}
