using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult.WinnerProcessor;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;


namespace ExtremeRoles.Module.GameResult;

#nullable enable

public sealed class WinnerBuilder : IDisposable
{
	private readonly int winGameControlId;
	private readonly WinnerInitializer initializer;

	public WinnerBuilder(
		int winGameControlId,
		IReadOnlyDictionary<byte, ExtremeGameResultManager.TaskInfo> taskInfo)
	{
		this.winGameControlId = winGameControlId;

		var state = ExtremeRolesPlugin.ShipState;
		var finalSummaryBuilder = new PlayerSummaryBuilder(
			state.EndReason,
			state.DeadPlayerInfo,
			taskInfo);

		this.initializer = new WinnerInitializer(finalSummaryBuilder);
	}

	public IReadOnlyList<FinalSummary.PlayerSummary> Build(WinnerContainer tempData)
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo("---- Start: Creating Winner ----");

		var result = this.initializer.Initialize(tempData);

		new RemoveAddPlusWinnerProcessor().Process(tempData, result.Winner);
		new AddNeutralWinerProcessor().Process(tempData, result.Winner);

		replaceWinner();

		new MergeWinnerProcessor().Process(tempData, result.Winner);
		new AddGhostRoleWinnerProcessor().Process(tempData, result.Winner);
		new ModifiedWinnerProcessor().Process(tempData, result.Winner);

		logger.LogInfo("--- End: Creating Winner ----");

#if DEBUG
		logger.LogInfo(tempData.ToString());
#endif
		return result.Summary;
	}

	private void replaceWinner()
	{
		switch ((RoleGameOverReason)ExtremeRolesPlugin.ShipState.EndReason)
		{
			case RoleGameOverReason.AssassinationMarin:
			case RoleGameOverReason.TeroristoTeroWithShip:
				this.tempData.Clear();
				foreach (Player player in GameData.Instance.AllPlayers.GetFastEnumerator())
				{
					if (ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
						role.IsImpostor())
					{
						this.tempData.Add(player);
					}
				}
				break;
			case RoleGameOverReason.AliceKilledByImposter:
			case RoleGameOverReason.AliceKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Alice);
				break;
			case RoleGameOverReason.JackalKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick);
				break;
			case RoleGameOverReason.LoverKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.ShipFallInLove:
				replaceWinnerToSpecificRolePlayer(
					ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.TaskMasterGoHome:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.TaskMaster);
				break;
			case RoleGameOverReason.MissionaryAllAgainstGod:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Missionary);
				break;
			case RoleGameOverReason.JesterMeetingFavorite:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Jester);
				break;
			case RoleGameOverReason.YandereKillAllOther:
			case RoleGameOverReason.YandereShipJustForTwo:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Yandere);
				break;
			case RoleGameOverReason.VigilanteKillAllOther:
			case RoleGameOverReason.VigilanteNewIdealWorld:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Vigilante);
				break;
			case RoleGameOverReason.MinerExplodeEverything:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Miner);
				break;
			case RoleGameOverReason.EaterAllEatInTheShip:
			case RoleGameOverReason.EaterAliveAlone:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Eater);
				break;
			case RoleGameOverReason.TraitorKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Traitor);
				break;
			case RoleGameOverReason.QueenKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Queen, ExtremeRoleId.Servant);
				break;
			case RoleGameOverReason.UmbrerBiohazard:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Umbrer);
				break;
			case RoleGameOverReason.KidsTooBigHomeAlone:
			case RoleGameOverReason.KidsAliveAlone:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Delinquent);
				break;
			case RoleGameOverReason.HatterEndlessTeaTime:
			case RoleGameOverReason.HatterTeaPartyTime:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Hatter);
				break;
			case RoleGameOverReason.ArtistShipToArt:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Artist);
				break;
			case RoleGameOverReason.TuckerShipIsExperimentStation:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Tucker, ExtremeRoleId.Chimera);
				break;
			case RoleGameOverReason.MonikaThisGameIsMine:
			case RoleGameOverReason.MonikaIamTheOnlyOne:
				replaceWinnerToSpecificNeutralRolePlayer(
					true, ExtremeRoleId.Monika);
				break;
			case RoleGameOverReason.AllJackalWin:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Shepherd);
				collectAllTargetRole(
					ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick, ExtremeRoleId.Furry);
				break;
			case RoleGameOverReason.AllYandereWin:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Intimate);
				collectAllTargetRole(
					ExtremeRoleId.Yandere, ExtremeRoleId.Surrogator);
				break;
			case RoleGameOverReason.AllQueenWin:
				replaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Knight);
				collectAllTargetRole(
					ExtremeRoleId.Queen, ExtremeRoleId.Servant, ExtremeRoleId.Pawn);
				break;
			case RoleGameOverReason.LiberalRevolution:
				replaceWinnerToLiberalPlayers();
				break;
			default:
				break;
		}
	}

	private void replaceWinnerToSpecificNeutralRolePlayer(params ExtremeRoleId[] roles)
		=> replaceWinnerToSpecificNeutralRolePlayer(isAlive: false, roles);

	private void replaceWinnerToSpecificNeutralRolePlayer(
		bool isAlive, params ExtremeRoleId[] roles)
	{
		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo("Clear Winner(Reason:Neautal Win)");
		this.tempData.Clear();
		collectAllTargetRole(isAlive, roles);
	}

	private void replaceWinnerToLiberalPlayers()
	{
		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo("Clear Winner(Reason:Liberal Win)");
		this.tempData.Clear();
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) && role.IsLiberal())
			{
				this.tempData.Add(player);
			}
		}
	}

	private void addSpecificRoleToSameControlIdPlayer(in SingleRoleBase role, in NetworkedPlayerInfo player)
	{
		if (this.winGameControlId != int.MaxValue &&
			this.winGameControlId == role.GameControlId)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Win this role) : {player.PlayerName}");
			this.tempData.Add(player);
		}
	}

	private void replaceWinnerToSpecificRolePlayer(
		ExtremeRoleId roleId)
	{
		this.tempData.Clear();

		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role))
			{
				continue;
			}

			if (role.Core.Id == roleId)
			{
				addSpecificRoleToSameControlIdPlayer(role, player);
			}
			else if (
				role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is not null &&
				multiAssignRole.AnotherRole.Core.Id == roleId)
			{
				addSpecificRoleToSameControlIdPlayer(multiAssignRole.AnotherRole, player);
			}
		}
	}

	private void addSpecificNeutralRoleToSameControlIdPlayer(in SingleRoleBase role, in NetworkedPlayerInfo player)
	{
		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Win this role) : {player.PlayerName}");
			this.tempData.Add(player);
		}
		else
		{
			addSpecificRoleToSameControlIdPlayer(role, player);
		}
	}

	private void collectAllTargetRole(params ExtremeRoleId[] roles)
		=> collectAllTargetRole(isAlive: false, roles);

	private void collectAllTargetRole(bool isAlive, params ExtremeRoleId[] roles)
	{
		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo($"Collect Neautral Winner:(alive only:{isAlive})");

		var builder = new StringBuilder();
		builder
			.AppendLine("--- SearchInfo ---")
			.Append("TargetContainer size:").Append(this.neutralNoWinner.Count).AppendLine()
			.Append("Specific role:");
		foreach (var role in roles)
		{
			builder.Append($"{role},");
		}
		logger.LogInfo(builder.ToString());

		foreach (var (player, role) in this.neutralNoWinner)
		{
			if (isAlive && (player.IsDead || player.Disconnected))
			{
				logger.LogInfo($"Player:{player.PlayerName} checking skip, this player not alive");
				continue;
			}

			var id = role.Core.Id;
			logger.LogInfo($"checking.... Player:{player.PlayerName}, Role:{id}");

			if (roles.Contains(id))
			{
				addSpecificNeutralRoleToSameControlIdPlayer(role, player);
			}
			else if (
				role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is not null &&
				roles.Contains(multiAssignRole.AnotherRole.Core.Id))
			{
				addSpecificNeutralRoleToSameControlIdPlayer(role, player);
			}
		}
	}

	public void Dispose()
	{
		this.initializer.Dispose();
	}
}
