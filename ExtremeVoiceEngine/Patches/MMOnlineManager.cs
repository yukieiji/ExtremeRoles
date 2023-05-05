using HarmonyLib;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
public static class MMOnlineManagerPatch
{
    public static void Postfix()
    {
        ChatCurrentSettingPatch.Chated = false;
    }
}