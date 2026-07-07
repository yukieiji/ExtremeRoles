using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using AmongUs.GameOptions;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct RoleAssignData<T>(T RoleId, ExtremeRoleType Team);
public readonly record struct SimulateOption(int Cycle, VanillaRolePlayerMockOption Option, List<string>? MockPlayerNames = null);
public readonly record struct AssignData(string PlayerName, string RoleName, ExtremeRoleType Team);
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
			GameData.Instance == null ||
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
		int playerNums = playerNames.Count;
		if (simulatePlayerNum > curPlayerCount)
		{
			int delta = simulateOption.Option.PlayerNum - curPlayerCount;
			if (playerNums == 0)
			{
				playerNames = randomName.Take(delta).ToList();
			}
			else if (playerNums < delta)
			{
				var additionalNames = randomName.Take(delta - playerNums).ToList();
				playerNames.AddRange(additionalNames);
			}
		}

		// DIスコープを作成して、必要なサービスの設定を行う
		using var scope = ExtremeRolesPlugin.Instance.Provider.CreateScope();
		var provider = scope.ServiceProvider;

		var option = provider.GetRequiredService<VanillaRolePlayerOption>();
		option.MockOption = simulateOption.Option;

		OptionManager.Load();

		// シミュレーションを実行して結果を取得する
		var result = Enumerable.Range(0, simulateOption.Cycle).Select(_ => {

			var rawData = provider.GetRequiredService<IRoleAssignDataBuilder>().Build();

			playerNames = playerNames.OrderBy(_ => RandomGenerator.Instance.Next()).ToList();
			Dictionary<byte, string> playerIdToName = new Dictionary<byte, string>();
			int index = 0;
			return new SimulateResult(rawData.Select(x =>
			{
				byte playerId = x.PlayerId;
				var player = GameData.Instance.GetPlayerById(playerId);

				string? playerName;
				if (player == null)
				{
					if (!playerIdToName.TryGetValue(playerId, out playerName) || string.IsNullOrEmpty(playerName))
					{
						playerName = index < playerNames.Count ? playerNames[index] : $"Unknown({playerId})";
						playerIdToName[playerId] = playerName;
						index++;
					}
				}
				else
				{
					playerName = player.DefaultOutfit.PlayerName;
				}

				ExtremeRoleType team;
				string roleName;
				if (Enum.IsDefined(typeof(RoleTypes), Convert.ToUInt16(x.RoleId)))
				{
					var roleId = ((RoleTypes)x.RoleId);
					roleName = roleId.ToString();
					team = VanillaRoleProvider.IsImpostorRole(roleId) ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate;
				}
				else
				{
					var data = getExRAssignData(x);
					roleName = data.RoleId.ToString();
					team = data.Team;
				}

				return new AssignData(playerName, roleName, team);
			}).ToArray());
		}).ToArray();


		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, result);
	}

	private static RoleAssignData<ExtremeRoleId> getExRAssignData(IPlayerToExRoleAssignData data)
	{
		var roleId = (ExtremeRoleId)data.RoleId;

		if (roleId is ExtremeRoleId.Xion)
		{
			return new RoleAssignData<ExtremeRoleId>(roleId, ExtremeRoleType.Null);
		}
		else if (roleId is ExtremeRoleId.Leader or ExtremeRoleId.Dove or ExtremeRoleId.Militant)
		{
			return new RoleAssignData<ExtremeRoleId>(roleId, ExtremeRoleType.Liberal);
		}
		var team = data is PlayerToCombRoleAssignData combData ? 
			ExtremeRoleManager.CombRole[combData.CombTypeId].GetRole(combData.RoleId, (RoleTypes)combData.AmongUsRoleId).Core.Team :
			ExtremeRoleManager.NormalRole[data.RoleId].Core.Team;
		
		return new RoleAssignData<ExtremeRoleId>(roleId, team);
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
	}.OrderBy(_ => RandomGenerator.Instance.Next());
}
