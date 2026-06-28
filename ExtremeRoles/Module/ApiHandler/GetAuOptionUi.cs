using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.ApiHandler;

public sealed class GetAuOptionUi : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		IRequestHandler.SetStatusOK(response);

		response.ContentType = "text/html";
		response.ContentEncoding = Encoding.UTF8;

		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("ExtremeRoles.Resources.settingui.index.html");

		if (stream == null)
		{
			response.StatusCode = (int)HttpStatusCode.NotFound;
			response.Close();
			return;
		}

		using var ms = new MemoryStream();
		stream.CopyTo(ms);
		byte[] buffer = ms.ToArray();

		response.Close(buffer, false);
	}
}
