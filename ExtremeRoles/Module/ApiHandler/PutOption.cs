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

		if (request.Id is null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		var parts = request.Id.Split('_');
		if (parts.Length == 3 &&
			int.TryParse(parts[0], out var tabInt) &&
			int.TryParse(parts[1], out var categoryId) &&
			int.TryParse(parts[2], out var optionId))
		{
			var tab = (OptionTab)tabInt;
			if (OptionManager.Instance.TryGetCategory(tab, categoryId, out var category))
			{
				OptionManager.Instance.Update(category, optionId, request.Value);

				IRequestHandler.SetStatusOK(response);
				IRequestHandler.SetContentsType(response);
				IRequestHandler.Write(response, OptionData.GetCurrentOptions());
				return;
			}
		}

		IRequestHandler.SetStatusNG(response);
		response.Abort();
	}
}
