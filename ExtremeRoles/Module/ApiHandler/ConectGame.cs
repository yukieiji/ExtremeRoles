using System;
using System.Net;
using System.Web;
using System.Collections;
using System.Text;

using InnerNet;

using BepInEx.Unity.IL2CPP.Utils;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.ApiHandler;

#pragma warning disable CA1852
internal class ConectGame : IRequestHandler
{
	public const string Path = "/au/game/";

	public Action<HttpListenerContext> Request => this.requestAction;

	private readonly record struct Data(string? Code, int? RowCode);

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (AmongUsClient.Instance == null ||
			!DestroyableSingleton<ServerManager>.InstanceExists ||
			AmongUsClient.Instance.mode != MatchMakerModes.None)
		{
			response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
			response.Abort();
			return;
		}

		var param = HttpUtility.ParseQueryString(context.Request.Url!.Query);

		string? strCode = param["Code"];
		string? rawCode = param["RawCode"];

		int code =
			!string.IsNullOrEmpty(rawCode) && int.TryParse(rawCode, out int parsedCode) ?
			parsedCode : GameCode.GameNameToInt(strCode);

		AmongUsClient.Instance.StopAllCoroutines();
		AmongUsClient.Instance.StartCoroutine(coJoin(code));

		response.ContentType = "text/html";
		response.ContentEncoding = Encoding.UTF8;

		string showPage = "<h3>Successfully sent room code to AmongUs!! Please close this tab.</h3>";

		byte[] buffer = Encoding.UTF8.GetBytes(showPage);

		IRequestHandler.SetStatusOK(response);
		response.Close(buffer, false);
	}

	private static IEnumerator coJoin(int code)
	{
		if (DestroyableSingleton<StoreMenu>.InstanceExists)
		{
			FastDestroyableSingleton<StoreMenu>.Instance.CloseEntirely();
		}
		while (!DestroyableSingleton<EOSManager>.InstanceExists)
		{
			yield return null;
		}
		yield return FastDestroyableSingleton<EOSManager>.Instance.WaitForLoginFlow();
		while (!DestroyableSingleton<ServerManager>.InstanceExists)
		{
			yield return null;
		}

		var sm = FastDestroyableSingleton<ServerManager>.Instance;
		if (!sm.IsExROnlyServer())
		{
			foreach (var region in sm.AvailableRegions)
			{
				if (region.IsExROnlyServer())
				{
					sm.SetRegion(region);
					break;
				}
			}
		}

		yield return FastDestroyableSingleton<ServerManager>.Instance.WaitForServers();
		yield return AmongUsClient.Instance.CoJoinOnlineGameFromCode(code);
	}
}
#pragma warning restore
