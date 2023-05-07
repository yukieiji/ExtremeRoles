using HarmonyLib;

using ExtremeRoles.Patches;
using ExtremeRoles.Performance;
using ExtremeVoiceEngine.Extension;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(CacheLocalPlayerPatch), nameof(CacheLocalPlayerPatch.SetLocalPlayer))]
public static class ChatCurrentSettingPatch
{
    public static bool Chated { get; set; } = false;

    public static void Postfix()
    {
        if (Chated ||
            FastDestroyableSingleton<HudManager>.Instance == null ||
            VoiceEngine.Instance == null ||
            !CachedPlayerControl.LocalPlayer) { return; }

        VoiceEngine.Instance.WaitExecute(
            () =>
            {
                Chated = true;
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddLocalChat(
                    TranslationControllerExtension.GetString(
                        "pringCurState", VoiceEngine.Instance.ToString()));
            });
    }
}