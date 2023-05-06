using HarmonyLib;
using TMPro;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShowerStartPatch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(VersionShower __instance)
    {
        __instance.text.alignment = TextAlignmentOptions.TopLeft;
        __instance.text.text = string.Concat(
           __instance.text.text, "\n", ExtremeVoiceEnginePlugin.Instance.ToString());
    }
}
