using HarmonyLib;

namespace ExtremeRoles.Patches.Button
{
    [HarmonyPatch(typeof(ToggleButtonBehaviour), nameof(ToggleButtonBehaviour.UpdateText))]
    public class ToggleButtonBehaviourUpdateTextPatch
    {
        public static void Postfix(
            ToggleButtonBehaviour __instance,
            [HarmonyArgument(0)] bool on)
        {
            if (__instance.BaseText != StringNames.SettingsStreamerMode) { return; }

            OptionHolder.Client.StreamerMode = on;

        }
    }
}
