using ExtremeRoles.GameMode;
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
        if (VoiceEngine.Instance == null ||
            chatText.StartsWith(Command.CommandManager.CmdChar)) { return; }

        VoiceEngine.Instance.AddQueue(chatText);
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatControllerSendChatPatch
{
    public static void Prefix(ChatController __instance)
    {
        if (VoiceEngine.Instance == null) { return; }
        
        VoiceEngine.Instance.WaitExecute(
            () => Command.CommandManager.Instance.ExcuteCmd(__instance.TextArea.text));
    }
}