using System;
using System.Linq;
using System.Net;

using InnerNet;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;


namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct OnlineInfo(int MaxPlayerNum, string Code, string Server);
public readonly record struct LobbyInfo(OnlineInfo? Online, string[] CurrentPlayerNames);

public sealed class GetLobby : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (AmongUsClient.Instance == null ||
			LobbyBehaviour.Instance == null ||
			GameManager.Instance == null ||
			GameManager.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.currentGameOptions == null ||
			GameOptionsManager.Instance.gameOptionsFactory == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, new LobbyInfo(
			AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ? 
			null : new OnlineInfo(
				GameOptionsManager.Instance.currentGameOptions.MaxPlayers,
				GameCode.IntToGameName(AmongUsClient.Instance.GameId),
				ServerManager.Instance.CurrentRegion == null ? "NOT FOUND" :
					TranslationController.Instance.GetStringWithDefault(ServerManager.Instance.CurrentRegion.TranslateName, ServerManager.Instance.CurrentRegion.Name)),
			GameData.Instance.AllPlayers.GetFastEnumerator().Select(x => x.DefaultOutfit.PlayerName).ToArray()));
	}
}
