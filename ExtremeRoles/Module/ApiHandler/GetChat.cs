using System;
using System.Linq;
using System.Net;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ApiHandler;

public readonly record struct ChatSenderInfo(
	PlayerNameInfo PlayerName,
	PlayerCosmicInfo CosmicInfo,
	bool isSameSender,
	bool isDead,
	bool isVoted)
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

public readonly record struct ChatInfo(ChatSenderInfo sender, string body)
{
	public static ChatInfo Create(ChatBubble chatBubble)
		=> new ChatInfo(
			ChatSenderInfo.Create(chatBubble),
			chatBubble.TextArea.text);
}

public readonly record struct GetChatResult(bool isMeeting, ChatInfo[] AllChat);

public sealed class GetChat : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (!DestroyableSingleton<HudManager>.InstanceExists ||
			FastDestroyableSingleton<HudManager>.Instance.Chat == null)
		{
			response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
			response.Abort();
			return;
		}

		var curChat = FastDestroyableSingleton<HudManager>.Instance.Chat.chatBubblePool.activeChildren;
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
