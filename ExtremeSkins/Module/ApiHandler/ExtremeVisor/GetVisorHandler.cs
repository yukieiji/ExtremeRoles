﻿using System;
using System.Linq;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeSkins.Core.API;

namespace ExtremeSkins.Module.ApiHandler.ExtremeVisor;
#if WITHVISOR

public sealed class GetVisorHandler : IRequestHandler
{

	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);

		var curData = CosmicStorage<CustomVisor>.GetAll().Select(
			x => new ExportData(x.Id, x.Name, x.Author));

		IRequestHandler.Write(response, curData);
	}
}
#endif