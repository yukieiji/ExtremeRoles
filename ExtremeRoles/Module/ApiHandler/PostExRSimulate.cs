using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Update;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;


namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct SimulateOption(int Cycle, VanillaRolePlayerMockOption Option);
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

		using var scope = ExtremeRolesPlugin.Instance.Provider.CreateScope();
		var provider = scope.ServiceProvider;

		var option = provider.GetRequiredService<VanillaRolePlayerOption>();
		option.MockOption = simulateOption.Option;

		var builder = provider.GetRequiredService<IRoleAssignDataBuilder>();

		var result = Enumerable.Range(0, simulateOption.Cycle).Select(_ => {
			var rawData = builder.Build();
			return new SimulateResult(rawData.Select(x =>
			{
				byte playerId = x.PlayerId;
				var player = GameData.Instance.GetPlayerById(playerId);
				string playerName = player == null ? $"Unknown({playerId})" : player.PlayerName;
				return new AssignData(playerName, x);
			}).ToArray());
		}).ToArray();


		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, result);
	}
}
