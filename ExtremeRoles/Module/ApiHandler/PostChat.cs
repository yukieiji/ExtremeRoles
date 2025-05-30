﻿using System;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ApiHandler;

public sealed class PostChat : IRequestHandler
{
	public const string Path = "/au/chat/";

	public readonly record struct Data(string Body);

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

		Data chatData = IRequestHandler.DeserializeJson<Data>(context.Request);

		PlayerControl.LocalPlayer.RpcSendChat(chatData.Body);

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}
