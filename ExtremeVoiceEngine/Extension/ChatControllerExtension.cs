using ExtremeRoles.Performance;

namespace ExtremeVoiceEngine.Extension;

public static class ChatControllerExtension
{
    public static void AddLocalChat(this ChatController chat, string text)
    {
        chat.AddChat(CachedPlayerControl.LocalPlayer, text);
    }
}
