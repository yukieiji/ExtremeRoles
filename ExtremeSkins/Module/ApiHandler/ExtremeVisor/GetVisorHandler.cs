using System;
using System.Linq;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Module.ApiHandler.ExtremeVisor;

public sealed class GetVisorHandler : IRequestHandler
{
	private readonly record struct VisorExportData(string Id, string Name, string Author);
	private readonly record struct ExVData(VisorExportData[] AllHats);

	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);

		var curState = new ExVData(
			ExtremeVisorManager.VisorData.Values.Select(
				x => new VisorExportData(x.Id, x.Name, x.Author)).ToArray());

		IRequestHandler.Write(response, curState);
	}
}