using ExtremeRoles.Module;
using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
public static class ChatBubbleSetTextPatch
{
	public static void Postfix(ChatBubble __instance)
	{
		if (ChatWebUI.IsExist)
		{
			ChatWebUI.Instance.AddChatToWebUI(__instance);
		}
	}
}