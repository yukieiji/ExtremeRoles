using HarmonyLib;
using TMPro;

namespace ExtremeSkins.Patches.AmongUs
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShowerStartPatch
    {
        private static TextMeshPro versionShowText;
        private static string defaultString;

        public static void Postfix(VersionShower __instance)
        {
            versionShowText = __instance.text;
            defaultString = __instance.text.text;
            versionShowText.alignment = TextAlignmentOptions.TopLeft;
            UpdateText();
        }
        public static void UpdateText()
        {
            string stateString = CreatorModeManager.Instance.StatusString;
            if (versionShowText != null &&
                stateString != string.Empty)
            {
                versionShowText.text = string.Concat(
                    defaultString, "\n", stateString);
            }
        }
    }
}
