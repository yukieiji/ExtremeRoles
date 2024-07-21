using System;
using System.Linq;
using System.Net;


using ExtremeRoles.Module.Interface;
using ExtremeSkins.Core.API;
using ExtremeSkins.Loader;


namespace ExtremeSkins.Module.ApiHandler.ExtremeHat;

#if WITHHAT
public sealed class GetHatHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);

		var curData = CosmicStorage<CustomHat>.GetAll().Select(
			x => new ExportData(x.Id, x.Name, x.Author)).ToArray();

		IRequestHandler.Write(response, curData);
	}
}
#endif