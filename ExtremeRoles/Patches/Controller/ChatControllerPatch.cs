using System;

using UnityEngine;

using HarmonyLib;

using AmongUs.Data;

using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.GameMode;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
public static class ChatControllerAddChatNotePatch
{
	public static bool Prefix(
		[HarmonyArgument(0)] NetworkedPlayerInfo srcPlayer,
		[HarmonyArgument(1)] ChatNoteTypes noteType)
	{
		return !(OnemanMeetingSystemManager.IsActive || noteType is ChatNoteTypes.DidVote);
	}
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatControllerAddChatPatch
{
    public static bool Prefix(
        ChatController __instance,
        [HarmonyArgument(0)] PlayerControl sourcePlayer,
        [HarmonyArgument(1)] string chatText,
		[HarmonyArgument(2)] bool censor)
    {
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

		if (sourcePlayer == null ||
			sourcePlayer.Data == null ||
			PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.Data == null)
		{
			return false;
		}

		var localPlayerData = PlayerControl.LocalPlayer.Data;
		var sourcePlayerData = sourcePlayer.Data;
		byte localPlayerId = localPlayerData.PlayerId;
		byte sourcePlayerId = sourcePlayerData.PlayerId;

		if (!ExtremeRoleManager.TryGetRole(localPlayerId , out var localPlayerRole) ||
			!ExtremeRoleManager.TryGetRole(sourcePlayerId, out var sourcePlayerRole))
		{
			return true;
		}

		bool assassinMeeting = ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger;
		bool islocalPlayerDead = localPlayerData.IsDead;
		bool isSourcePlayerDead = sourcePlayerData.IsDead;

		if ((
				!assassinMeeting && (isSourcePlayerDead && !islocalPlayerDead)
			)
			||
			(
				assassinMeeting && (!localPlayerRole.IsImpostor() || !sourcePlayerRole.IsImpostor())
			))

		{
			return false;
		}

		ChatBubble chatBubble = __instance.GetPooledBubble();
		try
		{
			chatBubble.transform.SetParent(__instance.scroller.Inner);
			chatBubble.transform.localScale = Vector3.one;
			bool isSamePlayer = sourcePlayerId == localPlayerId;
			if (isSamePlayer)
			{
				chatBubble.SetRight();
			}
			else
			{
				chatBubble.SetLeft();
			}

			Color seeColor = isSamePlayer ?
				localPlayerRole.GetNameColor(islocalPlayerDead) :
				localPlayerRole.GetTargetRoleSeeColor(sourcePlayerRole, sourcePlayerId);

			bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayerId);

			chatBubble.SetCosmetics(sourcePlayerData);

			__instance.SetChatBubbleName(
				chatBubble, sourcePlayerData, isSourcePlayerDead,
				didVote, seeColor, null);

			chatBubble.SetText(
				censor && DataManager.Settings.Multiplayer.CensorChat ?
                    BlockedWords.CensorWords(chatText) : chatText);

			chatBubble.AlignChildren();
			__instance.AlignAllBubbles();

			if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
			{
				__instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
			}
			if (!isSamePlayer)
			{
				SoundManager.Instance.PlaySound(
					__instance.messageSound, false, 1f).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
				__instance.chatNotification.SetUp(sourcePlayer, chatText);
			}
		}
		catch (Exception ex)
		{
			ExtremeRolesPlugin.Logger.LogError(ex);
			__instance.chatBubblePool.Reclaim(chatBubble);
		}
		return false;
	}
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatControllerSendChatPatch
{
	public static void Prefix(ChatController __instance)
	{
        if (ExtremeGameModeManager.Instance.EnableXion)
        {
			Roles.Solo.Host.Xion.ParseCommand(
				__instance.freeChatField.Text);
        }
	}
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.GetPooledBubble))]
public static class ChatControllerSetVisiblePatch
{
	public static void Prefix(ChatController __instance)
	{
		if (!ChatWebUI.IsExist ||
			__instance.chatBubblePool.NotInUse != 0)
		{
			return;
		}

		var webUi = ChatWebUI.Instance;

		if (__instance.chatBubblePool.activeChildren.Count > 0)
		{
			webUi.RemoveOldChat();
		}
		else
		{
			webUi.ResetChat();
		}
	}
}
