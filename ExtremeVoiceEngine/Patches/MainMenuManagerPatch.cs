using HarmonyLib;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManagerStartPatch
{
    public static void Postfix(MainMenuManager __instance)
    {
        ChatCurrentSettingPatch.Chated = false;
    }
}