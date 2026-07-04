using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct SimulateOption(int Cycle, VanillaRolePlayerMockOption Option, List<string>? MockPlayerNames = null);
public readonly record struct AssignData(string PlayerName, IPlayerToExRoleAssignData RawData);
public readonly record struct SimulateResult(AssignData[] CycleData);

public sealed class PostExRSimulate : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (AmongUsClient.Instance == null ||
			!AmongUsClient.Instance.AmHost ||
			LobbyBehaviour.Instance == null ||
			GameManager.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.currentGameOptions == null ||
			GameOptionsManager.Instance.gameOptionsFactory == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var simulateOption = IRequestHandler.DeserializeJson<SimulateOption>(context.Request);

		// プレイヤーネームを適当に補完する
		int curPlayerCount = GameData.Instance.AllPlayers.Count;
		int simulatePlayerNum = simulateOption.Option.PlayerNum;
		List<string> playerNames = simulateOption.MockPlayerNames ?? new List<string>();
		if (simulatePlayerNum > curPlayerCount)
		{
			int delta = simulateOption.Option.PlayerNum - curPlayerCount;
			if (playerNames.Count == 0)
			{
				playerNames = randomName.Take(delta).ToList();
			}
			else if (playerNames.Count < delta)
			{
				var additionalNames = randomName.Take(delta - playerNames.Count).ToList();
				playerNames.AddRange(additionalNames);
			}
		}
		playerNames = playerNames.OrderBy(_ => RandomGenerator.Instance.Next()).ToList();

		// DIスコープを作成して、必要なサービスの設定を行う
		using var scope = ExtremeRolesPlugin.Instance.Provider.CreateScope();
		var provider = scope.ServiceProvider;

		var option = provider.GetRequiredService<VanillaRolePlayerOption>();
		option.MockOption = simulateOption.Option;

		var builder = provider.GetRequiredService<IRoleAssignDataBuilder>();

		// シミュレーションを実行して結果を取得する
		int index = 0;
		var result = Enumerable.Range(0, simulateOption.Cycle).Select(_ => {
			var rawData = builder.Build();
			return new SimulateResult(rawData.Select(x =>
			{
				byte playerId = x.PlayerId;
				var player = GameData.Instance.GetPlayerById(playerId);

				string playerName;
				if (player == null)
				{
					playerName = index < playerNames.Count ? playerNames[index] : $"Unknown({playerId})";
				}
				else
				{
					playerName = player.DefaultOutfit.PlayerName;
				}
				index++;
				return new AssignData(playerName, x);
			}).ToArray());
		}).ToArray();


		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, result);
	}

	private static IEnumerable<string> randomName => new List<string>
	{
		"yukieiji",
		"zunda",
		"88659",
		"nanozoku",
		"exr",
		"exs",
		"exv",
		"ninchi",
		"tukuyomi",
		"yachiyo8000",
		"anko",
		"mikan",
		"impostor",
		"crewmate",
		"aoi-",
		"seyana-",
		"sorena-",
		"wakaru-",
		"arena-",
		"irop-",
		"kaguyaho-",
		"kiritanpo",
		"HelloWorld",
		"hoge",
		"sudo",
		"root",
		"AmongUS",
		"NoName",
		"名前を入れて下さい",
		"ああああ",
		"あああい",
		"あああう",
		"ExtremeRoles",
		"ExtremeSkins",
		"ExtremeHat",
		"ExtremeVisor",
		"DMZ",
	}.OrderBy(x => RandomGenerator.Instance.Next());
}
