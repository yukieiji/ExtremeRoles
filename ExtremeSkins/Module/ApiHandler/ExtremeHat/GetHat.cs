using ExtremeRoles.Module.Interface;

using System;
using System.Linq;
using System.Net;

using ExtremeSkins.SkinManager;


namespace ExtremeSkins.Module.ApiHandler.ExtremeHat;

public sealed class GetHatHandler : IRequestHandler
{
	private readonly record struct HatExportData(string Id, string Name, string Author);
	private readonly record struct ExHData(HatExportData[] AllHats);

	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);

		var curState = new ExHData(
			ExtremeHatManager.HatData.Values.Select(
				x => new HatExportData(x.Id, x.Name, x.Author)).ToArray());

		IRequestHandler.Write(response, curState);
	}
}