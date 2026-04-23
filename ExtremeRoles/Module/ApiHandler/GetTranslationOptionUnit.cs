using System;
using System.Collections.Generic;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Extension.Controller;

namespace ExtremeRoles.Module.ApiHandler;

public sealed class GetTranslationOptionUnit : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		var units = Enum.GetValues<OptionUnit>();
		var results = new List<GetTranslationResponse>();

		foreach (var unit in units)
		{
			string key = unit.ToString();
			string translated;
			if (unit == OptionUnit.None)
			{
				translated = string.Empty;
			}
			else
			{
				translated = Tr.GetString(key);
				translated = translated.Replace("{0}", string.Empty);
			}

			results.Add(new GetTranslationResponse(key, Array.Empty<object>(), translated));
		}

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, results.ToArray());
	}
}
