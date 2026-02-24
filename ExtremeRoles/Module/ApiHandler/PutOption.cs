using System;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public sealed class PutOption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		var request = IRequestHandler.DeserializeJson<PutOptionRequest>(context.Request);

		var (tab, categoryId, optionId) = OptionData.FromGlobalId(request.ID);

		if (OptionManager.Instance.TryGetCategory(tab, categoryId, out var category))
		{
			OptionManager.Instance.Update(category, optionId, request.Value);

			IRequestHandler.SetStatusOK(response);
			IRequestHandler.SetContentsType(response);
			IRequestHandler.Write(response, OptionData.GetCurrentOptions());
			return;
		}

		IRequestHandler.SetStatusNG(response);
		response.Abort();
	}
}
