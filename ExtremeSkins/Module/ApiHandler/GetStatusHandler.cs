using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;

using ExtremeRoles.Module.Interface;
using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Module.ApiHandler;

public sealed class GetStatusHandler : IRequestHandler
{
	private enum CurExSStatus
	{
		Booting,
		OK,
	}
	private enum ModuleStatus
	{
		Arrive,
		NotLoad,
		NoData,
	}

	private readonly record struct ExSStatus(CurExSStatus RowStatus, ModuleStatusData Module)
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public readonly CurExSStatus RowStatus = RowStatus;
		public string Status => RowStatus.ToString();
	}
	private readonly record struct ModuleStatusData(
		ModuleStatus ExCStatus,
		ModuleStatus ExHStatus,
		ModuleStatus ExVStatus,
		ModuleStatus ExNStatus)
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public readonly ModuleStatus ExCStatus = ExCStatus;
		public string ExtremeColor => ExCStatus.ToString();

		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public readonly ModuleStatus ExHStatus = ExHStatus;
		public string ExtremeHat => ExHStatus.ToString();

		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public readonly ModuleStatus ExVStatus = ExVStatus;
		public string ExtremeVisor => ExVStatus.ToString();

		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public readonly ModuleStatus ExNStatus = ExNStatus;
		public string ExtremeNamePlate => ExNStatus.ToString();
	}

	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);

		ModuleStatus excStatus = ModuleStatus.Arrive;
#if WITHHAT
		ModuleStatus exhStatus = ExtremeHatManager.HatData.Count == 0 ? ModuleStatus.NoData : ModuleStatus.Arrive;
#else
		ModuleStatus exhStatus = ModuleStatus.NotLoad;
#endif
#if WITHVISOR
		ModuleStatus exvStatus = ExtremeVisorManager.VisorData.Count == 0 ? ModuleStatus.NoData : ModuleStatus.Arrive;
#else
		ModuleStatus exvStatus = ModuleStatus.NotLoad;
#endif
#if WITHNAMEPLATE
		ModuleStatus exnStatus = ExtremeNamePlateManager.NamePlateData.Count == 0 ? ModuleStatus.NoData : ModuleStatus.Arrive;
#else
		ModuleStatus exnStatus = ModuleStatus.NotLoad;
#endif
		var moduleState = new ModuleStatusData(excStatus, exhStatus, exvStatus, exnStatus);
		var curState = new ExSStatus(
			Patches.AmongUs.SplashManagerUpdatePatch.IsSkinLoad ? CurExSStatus.Booting : CurExSStatus.OK,
			moduleState);

		IRequestHandler.Write(response, curState);
	}
}
