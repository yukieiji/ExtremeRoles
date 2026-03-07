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

	}
}
