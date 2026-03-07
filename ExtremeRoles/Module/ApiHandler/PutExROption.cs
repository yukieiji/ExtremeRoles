using System;
using System.Collections.Generic;
using System.Net;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct ExROptionPutRequest(int TabId, int CategoryId, int OptionId, int Selection);
public readonly record struct UpdatedOptions(ExRCategoryDto UpdatedCategory, IReadOnlyList<ExROptionDto> ChainUpdatedOption);

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

		var tab = (OptionTab)newOptionSelection.TabId;
		OptionManager.Instance.Update(
			tab, newOptionSelection.CategoryId,
			newOptionSelection.OptionId,
			newOptionSelection.Selection);

		if (!OptionManager.Instance.TryGetCategory(tab, newOptionSelection.CategoryId, out var category))
		{
			response.StatusCode = (int)HttpStatusCode.Accepted;
			response.Close();
			return;
		}

		var updatedCategory = GetExrOption.CreateCategoryDto(category);

		IRequestHandler.Write(response, new UpdatedOptions(updatedCategory, []));
		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}
