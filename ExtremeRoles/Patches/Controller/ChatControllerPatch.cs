using System;

using UnityEngine;

using HarmonyLib;

using AmongUs.Data;

using ExtremeRoles.GameMode;
using ExtremeRoles.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
public static class ChatControllerAddChatNotePatch
{
	public static bool Prefix(
		[HarmonyArgument(0)] GameData.PlayerInfo srcPlayer,
		[HarmonyArgument(1)] ChatNoteTypes noteType)
	{
		return !ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger || noteType != ChatNoteTypes.DidVote;
	}
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatControllerAddChatPatch
{
    public static bool Prefix(
        ChatController __instance,
        [HarmonyArgument(0)] PlayerControl sourcePlayer,
        [HarmonyArgument(1)] string chatText)
    {
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

		if (!sourcePlayer || !CachedPlayerControl.LocalPlayer)
		{
			return false;
		}

		GameData.PlayerInfo localPlayerData = CachedPlayerControl.LocalPlayer.Data;
		GameData.PlayerInfo sourcePlayerData = sourcePlayer.Data;

		bool assassinMeeting = ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger;

		if (!assassinMeeting)
		{
			if (localPlayerData == null || sourcePlayerData == null ||
				(sourcePlayerData.IsDead && !localPlayerData.IsDead))
			{
				return false;
			}
		}

		var roleDict = ExtremeRoleManager.GameRole;

		byte localPlayerId = localPlayerData.PlayerId;
		byte sourcePlayerId = sourcePlayerData.PlayerId;

		if (!roleDict.TryGetValue(localPlayerId, out var role) ||
			!roleDict.TryGetValue(sourcePlayerId, out var role2))
		{
			return true;
		}

		if (assassinMeeting && (!role.IsImpostor() || !role2.IsImpostor()))
        {
			return false;
        }

		ChatBubble chatBubble = __instance.GetPooledBubble();
		try
		{
			chatBubble.transform.SetParent(__instance.scroller.Inner);
			chatBubble.transform.localScale = Vector3.one;
			bool isLocalPlayer = sourcePlayer.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId;
			if (isLocalPlayer)
			{
				chatBubble.SetRight();
			}
			else
			{
				chatBubble.SetLeft();
			}

			Color seeColor = isLocalPlayer ?
				role.GetNameColor(CachedPlayerControl.LocalPlayer.Data.IsDead) :
				role.GetTargetRoleSeeColor(role2, sourcePlayerId);

			bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayerId);

			chatBubble.SetCosmetics(sourcePlayerData);

			__instance.SetChatBubbleName(
				chatBubble, sourcePlayerData, sourcePlayerData.IsDead,
				didVote, seeColor, null);
			chatBubble.SetText(
				DataManager.Settings.Multiplayer.CensorChat ?
                    BlockedWords.CensorWords(chatText) : chatText);
			chatBubble.AlignChildren();
			__instance.AlignAllBubbles();
			if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
			{
				__instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
			}
			if (!isLocalPlayer)
			{
				SoundManager.Instance.PlaySound(
					__instance.messageSound, false, 1f).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
			}
			return false;
		}
		catch (Exception ex)
		{
			ExtremeRolesPlugin.Logger.LogError(ex);
			__instance.chatBubblePool.Reclaim(chatBubble);
			return false;
		}

	}
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatControllerSendChatPatch
{
	public static void Prefix(ChatController __instance)
	{
        if (ExtremeGameModeManager.Instance.RoleSelector.IsCanUseAndEnableXion())
        {
			Roles.Solo.Host.Xion.ParseCommand(
				__instance.freeChatField.Text);
        }
	}
}
