using HarmonyLib;

using ExtremeRoles.Extension.Controller;

using ExtremeVoiceEngine.Extension;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(PlayerControl._Start_d__82), nameof(PlayerControl._Start_d__82.MoveNext))]
public static class ChatCurrentSettingPatch
{
    public static bool Chated { get; set; } = false;

    public static void Postfix(ref bool __result)
    {
        if (__result ||
			Chated ||
			HudManager.Instance == null ||
			VoiceEngine.Instance == null ||
			HudManager.Instance.Chat == null)
		{
			return;
		}

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