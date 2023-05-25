using HarmonyLib;

using ExtremeRoles.Performance;

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
            chatText.StartsWith(Command.CommandManager.CmdChar) ||
			!CachedPlayerControl.LocalPlayer.Data.IsDead && sourcePlayer.Data.IsDead)
		{
			return;
		}

        VoiceEngine.Instance.AddQueue(chatText);
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatControllerSendChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        if (VoiceEngine.Instance == null) { return true; }

        bool isRpcSend = true;
        VoiceEngine.Instance.WaitExecute(
            () => isRpcSend = !Command.CommandManager.Instance.ExcuteCmd(__instance.TextArea.text));
        if (isRpcSend)
        {
            return true;
        }
        __instance.TextArea.Clear();
        return false;
    }
}