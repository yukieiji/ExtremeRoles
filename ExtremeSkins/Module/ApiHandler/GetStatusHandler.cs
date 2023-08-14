using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExtremeSkins.Module.ApiHandler;

public sealed class GetStatusHandler : ApiServer.IRequestHandler
{
	private enum CurExSStatus
	{
		Booting,
		OK,
	}

	private record struct ExSStatus(CurExSStatus RowStatus)
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public CurExSStatus RowStatus = RowStatus;
		public string Status => RowStatus.ToString();
	}

	public ApiServer.RequestType Type => ApiServer.RequestType.Get;

	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		ApiServer.IRequestHandler.SetStatusOK(response);
		ApiServer.IRequestHandler.SetContentsType(response);

		var curState = new ExSStatus(Patches.AmongUs.SplashManagerUpdatePatch.IsSkinLoad ? CurExSStatus.Booting : CurExSStatus.OK);

		using var stream = new MemoryStream();
		JsonSerializer.Serialize(stream, curState);
		stream.GetBuffer();

		// message の内容をバイト配列に変換してレスポンスを返す
		response.Close(stream.ToArray(), false);
	}
}
