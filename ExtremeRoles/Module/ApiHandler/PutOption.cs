using System;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct OptionPutRequest(int TabId, int CategoryId, int OptionId, int Selection);

public sealed class PutOption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		var newOptionSelection = IRequestHandler.DeserializeJson<OptionPutRequest>(context.Request);

		OptionManager.Instance.Update(
			(OptionTab)newOptionSelection.TabId,
			newOptionSelection.CategoryId,
			newOptionSelection.OptionId,
			newOptionSelection.Selection);

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}
