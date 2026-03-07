using System;
using System.Net;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct ExROptionPutRequest(int TabId, int CategoryId, int OptionId, int Selection);

public sealed class PutExROption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{


		var response = context.Response;

		if (AmongUsClient.Instance == null ||
			!AmongUsClient.Instance.AmHost ||
			LobbyBehaviour.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.CurrentGameOptions == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var newOptionSelection = IRequestHandler.DeserializeJson<ExROptionPutRequest>(context.Request);

		OptionManager.Instance.Update(
			(OptionTab)newOptionSelection.TabId,
			newOptionSelection.CategoryId,
			newOptionSelection.OptionId,
			newOptionSelection.Selection);

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}

