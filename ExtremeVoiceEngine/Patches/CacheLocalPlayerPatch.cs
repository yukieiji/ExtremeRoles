using HarmonyLib;

using ExtremeRoles.Patches.Player;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Controller;

using ExtremeVoiceEngine.Extension;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(SetCachedLocalPlayerControl), nameof(SetCachedLocalPlayerControl.SetLocalPlayer))]
public static class ChatCurrentSettingPatch
{
    public static bool Chated { get; set; } = false;

    public static void Postfix()
    {
        if (Chated ||
            HudManager.Instance == null ||
            VoiceEngine.Instance == null ||
            !PlayerControl.LocalPlayer) { return; }

        VoiceEngine.Instance.WaitExecute(
            () =>
            {
                Chated = true;
                HudManager.Instance.Chat.AddLocalChat(
                    TranslationControllerExtension.GetString(
                        "pringCurState", VoiceEngine.Instance.ToString()));
            });
    }
}