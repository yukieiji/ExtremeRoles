﻿using System;
using System.Linq;
using System.Net;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ApiHandler;

public readonly record struct ChatSenderInfo(
	PlayerNameInfo PlayerName,
	PlayerCosmicInfo CosmicInfo,
	bool IsSameSender,
	bool IsDead,
	bool IsVoted)
{
	public static ChatSenderInfo Create(ChatBubble chatBubble)
	{
		Color32 color = chatBubble.NameText.color;
		var playerName = new PlayerNameInfo(chatBubble.NameText.text, color);

		var playerData = chatBubble.playerInfo.DefaultOutfit;
		var cosmicInfo = PlayerCosmicInfo.Create(playerData);

		return new ChatSenderInfo(
			playerName,
			cosmicInfo,
			chatBubble.Player.cosmetics.FlipX,
			chatBubble.Xmark.enabled,
			chatBubble.votedMark.enabled);
	}
}

public readonly record struct ChatInfo(ChatSenderInfo Sender, string Body)
{
	public static ChatInfo Create(ChatBubble chatBubble)
		=> new ChatInfo(
			ChatSenderInfo.Create(chatBubble),
			chatBubble.TextArea.text);
}

public readonly record struct GetChatResult(bool IsMeeting, ChatInfo[] AllChat);

public sealed class GetChat : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (!HudManager.InstanceExists ||
			HudManager.Instance.Chat == null)
		{
			response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
			response.Abort();
			return;
		}

		var curChat = HudManager.Instance.Chat.chatBubblePool.activeChildren;
		var chatInfo = curChat
			.ToArray()
			.Where(x => x != null && x.TryCast<ChatBubble>() != null)
			.Select(x => ChatInfo.Create(x.Cast<ChatBubble>()));

		var result = new GetChatResult(MeetingHud.Instance != null, chatInfo.ToArray());

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, result);
	}
}
