using HarmonyLib;

namespace ExtremeVoiceEngine.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatControllerAddChatPatch
{
    public static void Postfix(
        ChatController __instance,
        [HarmonyArgument(0)] PlayerControl sourcePlayer,
        [HarmonyArgument(1)] string chatText)
    {
        if (VoiceEngine.Instance == null) { return; }

        VoiceEngine.Instance.AddQueue(chatText);
    }
}