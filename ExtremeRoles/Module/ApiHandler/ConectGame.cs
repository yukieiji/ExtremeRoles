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

public readonly record struct ServerInfo(string Name, StringNames TransName);

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
			AmongUsClient.Instance.mode is not MatchMakerModes.None)
		{
			response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
			response.Abort();
			return;
		}

		string query = context.Request.Url!.Query;
		// 後で暗号化処理等する
		var param = HttpUtility.ParseQueryString(query);

		string? strCode = param["Code"];
		string? rawCode = param["RawCode"];
		string? server = param["server"];

		var serverInfo =
			Enum.TryParse<StringNames>(server, out var stringsServer) ?
			new ServerInfo(server, stringsServer) :
			new ServerInfo(IRegionInfoExtension.ExROfficialServerTokyoManinName, StringNames.NoTranslation);

		int code =
			!string.IsNullOrEmpty(rawCode) && int.TryParse(rawCode, out int parsedCode) ?
			parsedCode : GameCode.GameNameToInt(strCode);

		AmongUsClient.Instance.StopAllCoroutines();
		AmongUsClient.Instance.StartCoroutine(coJoin(code, serverInfo));

		response.ContentType = "text/html";
		response.ContentEncoding = Encoding.UTF8;

		string showPage = "<h3>Successfully sent room code to AmongUs!! Please close this tab.</h3>";

		byte[] buffer = Encoding.UTF8.GetBytes(showPage);

		IRequestHandler.SetStatusOK(response);
		response.Close(buffer, false);
	}

	private static IEnumerator coJoin(int code, ServerInfo serverInfo)
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

		var sm = FastDestroyableSingleton<ServerManager>.Instance;
		if (sm.CurrentRegion == null ||
			sm.CurrentRegion.TranslateName != serverInfo.TransName ||
			!isValidCustomServer(sm.CurrentRegion, serverInfo))
		{
			foreach (var region in sm.AvailableRegions)
			{
				if (region.TranslateName == serverInfo.TransName &&
					(serverInfo.TransName is not StringNames.NoTranslation || isValidCustomServer(region, serverInfo)))
				{
					sm.SetRegion(region);
					break;
				}
			}
		}

		yield return FastDestroyableSingleton<ServerManager>.Instance.WaitForServers();
		yield return AmongUsClient.Instance.CoJoinOnlineGameFromCode(code);
	}

	private static bool isValidCustomServer(IRegionInfo target, ServerInfo serverInfo)
		=> serverInfo.TransName is StringNames.NoTranslation && target.Name == serverInfo.Name;
}
#pragma warning restore
