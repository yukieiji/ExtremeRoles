using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Extension.Controller;
using Il2CppObject = Il2CppSystem.Object;

namespace ExtremeRoles.Module.ApiHandler;

public readonly record struct GetTranslationRequest(JsonElement Key, JsonElement[]? Param);

public readonly record struct GetTranslationResponse(object Key, object[] Param, string Result);

public sealed class GetTranslation : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		GetTranslationRequest req;
		try
		{
			req = IRequestHandler.DeserializeJson<GetTranslationRequest>(context.Request);
		}
		catch (Exception)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		object keyObj;
		string translated = "";

		var parts = new List<Il2CppObject>();
		if (req.Param != null)
		{
			foreach (var p in req.Param)
			{
				// Convert to Il2CppObject.
				// Strings are implicitly converted to Il2CppSystem.String.
				// For numbers, we convert to string to be safe as seen in other parts of the codebase.
				parts.Add((Il2CppObject)p.ToString());
			}
		}

		var partsArray = parts.ToArray();

		if (req.Key.ValueKind == JsonValueKind.Number && req.Key.TryGetInt32(out int intKey))
		{
			keyObj = intKey;
			translated = TranslationController.Instance.GetString((StringNames)intKey, partsArray);
		}
		else
		{
			string strKey = req.Key.GetString() ?? "";
			keyObj = strKey;
			translated = Tr.GetString(strKey, partsArray);
		}

		var result = new GetTranslationResponse(
			keyObj,
			(req.Param ?? Array.Empty<JsonElement>()).Select(p =>
			{
				if (p.ValueKind == JsonValueKind.Number)
				{
					if (p.TryGetInt32(out int i))
					{
						return (object)i;
					}
					if (p.TryGetDouble(out double d))
					{
						return (object)d;
					}
				}
				if (p.ValueKind == JsonValueKind.True)
				{
					return (object)true;
				}
				if (p.ValueKind == JsonValueKind.False)
				{
					return (object)false;
				}
				return (object)(p.GetString() ?? p.ToString());
			}).ToArray(),
			translated
		);

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, result);
	}
}
