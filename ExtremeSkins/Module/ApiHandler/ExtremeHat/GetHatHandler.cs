using System;
using System.Linq;
using System.Net;


using ExtremeRoles.Module.Interface;
using ExtremeSkins.Core.API;
using ExtremeSkins.SkinLoader;
using ExtremeSkins.SkinManager;


namespace ExtremeSkins.Module.ApiHandler.ExtremeHat;

public sealed class GetHatHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);

		var curData = SkinContainer<CustomHat>.GetValues().Select(
			x => new ExportData(x.Id, x.Name, x.Author)).ToArray();

		IRequestHandler.Write(response, curData);
	}
}