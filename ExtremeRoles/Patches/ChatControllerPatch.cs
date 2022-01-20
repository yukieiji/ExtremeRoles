using System;

using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Roles;


namespace ExtremeRoles.Patches
{
	
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    public static class ChatControllerAddChatPatch
    {
        public static bool Prefix(
            ChatController __instance,
            [HarmonyArgument(0)] PlayerControl sourcePlayer,
            [HarmonyArgument(1)] string chatText)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

			if (!sourcePlayer || !PlayerControl.LocalPlayer)
			{
				return false;
			}

			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			GameData.PlayerInfo data2 = sourcePlayer.Data;

			var role = ExtremeRoleManager.GameRole[data.PlayerId];
			var role2 = ExtremeRoleManager.GameRole[data2.PlayerId];

			if (data2 == null || data == null || (data2.IsDead && !data.IsDead))
			{
				return false;
			}
			
			if (__instance.chatBubPool.NotInUse == 0)
			{
				__instance.chatBubPool.ReclaimOldest();
			}
			ChatBubble chatBubble = __instance.chatBubPool.Get<ChatBubble>();
			try
			{
				chatBubble.transform.SetParent(__instance.scroller.Inner);
				chatBubble.transform.localScale = Vector3.one;
				bool flag = sourcePlayer == PlayerControl.LocalPlayer;
				if (flag)
				{
					chatBubble.SetRight();
				}
				else
				{
					chatBubble.SetLeft();
				}

				Color seeColor;

				if (data.PlayerId == data2.PlayerId)
                {
					seeColor = role.NameColor;
                }
				else
                {
					seeColor = role.GetTargetRoleSeeColor(role2, data2.PlayerId);
				}

				bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
				
				chatBubble.SetCosmetics(data2);
				
				__instance.SetChatBubbleName(
					chatBubble, data2, data2.IsDead,
					didVote, seeColor, null);
				if (SaveManager.CensorChat)
				{
					chatText = BlockedWords.CensorWords(chatText);
				}
				chatBubble.SetText(chatText);
				chatBubble.AlignChildren();
				__instance.AlignAllBubbles();
				if (!__instance.IsOpen && __instance.notificationRoutine == null)
				{
					__instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
				}
				if (!flag)
				{
					SoundManager.Instance.PlaySound(
						__instance.MessageSound, false, 1f).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
				}
				return false;
			}
			catch (Exception ex)
			{
				ExtremeRolesPlugin.Logger.LogError(ex);
				__instance.chatBubPool.Reclaim(chatBubble);
				return false;
			}

		}
    }
}
