using AmongUs.Data;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;



#nullable enable

namespace ExtremeRoles.Patches.Controller;

public record PlayerDataContext(PlayerControl Control, SingleRoleBase Role)
{
	public NetworkedPlayerInfo Data { get; } = Control.Data;
	public bool IsDead { get; } = Control.Data.IsInValid();
	public byte Id { get; } = Control.PlayerId;

	public static bool TryCreate(PlayerControl target, IGameContext ctx, [NotNullWhen(true)] out PlayerDataContext? playerDataContext)
	{
		if (ctx.Roles.TryGetRole(target.PlayerId, out var role))
		{
			playerDataContext = new PlayerDataContext(target, role);
			return true;
		}
		else
		{
			playerDataContext = null;
			return false;
		}
	}
}

public class ChatControllerAddChatPatchBody(IModLogger logger, IGameProgress progress, IGameRuntime runtime)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;
	private readonly IModLogger _logger = logger;

	public bool Prefix(
		ChatController __instance,
		PlayerControl sourcePlayer,
		string chatText,
		bool censor)
	{
		if (!(_progress.IsGameNow && _runtime.TryGetGameContext(out var ctx)))
		{
			return true;
		}

		if (sourcePlayer == null ||
			sourcePlayer.Data == null ||
			PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.Data == null)
		{
			return false;
		}

		if (!PlayerDataContext.TryCreate(PlayerControl.LocalPlayer, ctx, out var local) ||
			!PlayerDataContext.TryCreate(sourcePlayer, ctx, out var source))
		{
			return true;
		}

		bool isOneMan = OnemanMeetingSystemManager.TryGetActiveSystem(out var system);
		bool isMonikaOn = MonikaTrashSystem.TryGet(out var monikaSystem);

		if ((
				!isOneMan && (source.IsDead && !local.IsDead)
			)
			||
			(
				isOneMan && !system!.IsValidShowChatPlayer(sourcePlayer) &&
				// モニカがいてゴミ箱の人が参加できるのはおかしいので・・・・
				(!isMonikaOn || !monikaSystem!.CanChatBetween(source.Data, local.Data))
			)
			||
			(
				!isOneMan && isMonikaOn && !monikaSystem!.CanChatBetween(source.Data, local.Data)
			))

		{
			return false;
		}


		var chatBubble = __instance.GetPooledBubble();

		try
		{
			ForceAddToChatBuble(chatBubble, __instance, local, source, chatText, censor);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex);
			__instance.chatBubblePool.Reclaim(chatBubble);
		}
		return false;
	}

	private static void ForceAddToChatBuble(
		ChatBubble chatBubble,
		ChatController __instance,
		PlayerDataContext local,
		PlayerDataContext source,
		string chatText,
		bool censor)
	{
		chatBubble.transform.SetParent(__instance.scroller.Inner);
		chatBubble.transform.localScale = Vector3.one;
		bool isSamePlayer = source.Id == local.Id;
		if (isSamePlayer)
		{
			chatBubble.SetRight();
		}
		else
		{
			chatBubble.SetLeft();
		}

		Color seeColor = isSamePlayer ?
			local.Role.GetNameColor(local.IsDead) :
			local.Role.GetTargetRoleSeeColor(source.Role, source.Id);

		bool didVote = MeetingHud.Instance != null && MeetingHud.Instance.DidVote(source.Id);

		chatBubble.SetCosmetics(source.Data);

		__instance.SetChatBubbleName(
			chatBubble, source.Data, source.IsDead,
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
				__instance.messageSound, false, 1f).pitch = 0.5f + (float)source.Id / 15f;
			__instance.chatNotification.SetUp(source.Control, chatText);
		}
	}
}

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
	private static ChatControllerAddChatPatchBody? _body;

	public static bool Prefix(
        ChatController __instance,
        [HarmonyArgument(0)] PlayerControl sourcePlayer,
        [HarmonyArgument(1)] string chatText,
		[HarmonyArgument(2)] bool censor)
    {
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<ChatControllerAddChatPatchBody>();
		}
		return _body.Prefix(__instance, sourcePlayer, chatText, censor);
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
