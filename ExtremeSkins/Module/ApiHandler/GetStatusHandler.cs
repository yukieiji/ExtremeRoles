using System;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeSkins.Core.API;

namespace ExtremeSkins.Module.ApiHandler;

public sealed class GetStatusHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);

		ModuleStatus excStatus = ModuleStatus.Arrive;
#if WITHHAT
		ModuleStatus exhStatus = CosmicStorage<CustomHat>.IsEmpty ? ModuleStatus.NoData : ModuleStatus.Arrive;
#else
		ModuleStatus exhStatus = ModuleStatus.NotLoad;
#endif
#if WITHVISOR
		ModuleStatus exvStatus = CosmicStorage<CustomVisor>.IsEmpty ? ModuleStatus.NoData : ModuleStatus.Arrive;
#else
		ModuleStatus exvStatus = ModuleStatus.NotLoad;
#endif
#if WITHNAMEPLATE
		ModuleStatus exnStatus = CosmicStorage<CustomNamePlate>.IsEmpty ? ModuleStatus.NoData : ModuleStatus.Arrive;
#else
		ModuleStatus exnStatus = ModuleStatus.NotLoad;
#endif
		var moduleState = new ModuleStatusData(excStatus, exhStatus, exvStatus, exnStatus);
		var curState = new StatusData(
			Patches.AmongUs.SplashManagerUpdatePatch.IsSkinLoad ? ExSStatus.Booting : ExSStatus.OK,
			moduleState);

		IRequestHandler.Write(response, curState);
	}
}
