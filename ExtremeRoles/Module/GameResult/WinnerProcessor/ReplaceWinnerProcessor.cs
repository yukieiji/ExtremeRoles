using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtremeRoles.GameMode;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.GameResult.WinnerProcessor;

file sealed class NeutralWinnerProcessor(int winGameControlId, WinnerContainer winnerContainer, WinnerState state)
{
	private readonly int winGameControlId = winGameControlId;
	private readonly WinnerContainer winner = winnerContainer;
	private readonly IReadOnlyList<NeutralRoleInfo> neutralNoWinner = state.NeutralNoWinner;
	public void ReplaceWinnerToSpecificNeutralRolePlayer(params ExtremeRoleId[] roles)
		=> ReplaceWinnerToSpecificNeutralRolePlayer(isAlive: false, roles);

	public void CollectAllTargetRole(params ExtremeRoleId[] roles)
		=> collectAllTargetRole(isAlive: false, roles);

	public void ReplaceWinnerToSpecificNeutralRolePlayer(
		bool isAlive, params ExtremeRoleId[] roles)
	{
		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo("Clear Winner(Reason:Neautal Win)");
		this.winner.Clear();
		collectAllTargetRole(isAlive, roles);
	}

	public void ReplaceWinnerToSpecificRolePlayer(
		ExtremeRoleId roleId)
	{
		this.winner.Clear();

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

	private void addSpecificRoleToSameControlIdPlayer(in SingleRoleBase role, in NetworkedPlayerInfo player)
	{
		if (this.winGameControlId != int.MaxValue &&
			this.winGameControlId == role.GameControlId)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Win this role) : {player.PlayerName}");
			this.winner.Add(player);
		}
	}

	private void addSpecificNeutralRoleToSameControlIdPlayer(in SingleRoleBase role, in NetworkedPlayerInfo player)
	{
		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Win this role) : {player.PlayerName}");
			this.winner.Add(player);
		}
		else
		{
			addSpecificRoleToSameControlIdPlayer(role, player);
		}
	}

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
}

public sealed class ReplaceWinnerProcessor(int winGameControlId) : IWinnerProcessor
{
	private readonly int winGameControlId = winGameControlId;

	public void Process(WinnerContainer winner, WinnerState state)
	{
		var reason = (RoleGameOverReason)ExtremeRolesPlugin.ShipState.EndReason;
		
		// インポスター陣営などのインナークラスを使わない処理
		if (reason is 
				RoleGameOverReason.AssassinationMarin or 
				RoleGameOverReason.TeroristoTeroWithShip)
		{
			replaceWinnerToImpPlayer(winner);
			return;
		}
		else if (reason is RoleGameOverReason.LiberalRevolution)
		{
			replaceWinnerToLiberalPlayer(winner);
			return;
		}

		// 以下ニュートラル役職の処理
		var processor = new NeutralWinnerProcessor(this.winGameControlId, winner, state);
		switch ((RoleGameOverReason)ExtremeRolesPlugin.ShipState.EndReason)
		{
			case RoleGameOverReason.AliceKilledByImposter:
			case RoleGameOverReason.AliceKillAllOther:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Alice);
				break;
			case RoleGameOverReason.JackalKillAllOther:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick);
				break;
			case RoleGameOverReason.LoverKillAllOther:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.ShipFallInLove:
				processor.ReplaceWinnerToSpecificRolePlayer(
					ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.TaskMasterGoHome:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.TaskMaster);
				break;
			case RoleGameOverReason.MissionaryAllAgainstGod:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Missionary);
				break;
			case RoleGameOverReason.JesterMeetingFavorite:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Jester);
				break;
			case RoleGameOverReason.YandereKillAllOther:
			case RoleGameOverReason.YandereShipJustForTwo:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Yandere);
				break;
			case RoleGameOverReason.VigilanteKillAllOther:
			case RoleGameOverReason.VigilanteNewIdealWorld:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Vigilante);
				break;
			case RoleGameOverReason.MinerExplodeEverything:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Miner);
				break;
			case RoleGameOverReason.EaterAllEatInTheShip:
			case RoleGameOverReason.EaterAliveAlone:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Eater);
				break;
			case RoleGameOverReason.TraitorKillAllOther:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Traitor);
				break;
			case RoleGameOverReason.QueenKillAllOther:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Queen, ExtremeRoleId.Servant);
				break;
			case RoleGameOverReason.UmbrerBiohazard:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Umbrer);
				break;
			case RoleGameOverReason.KidsTooBigHomeAlone:
			case RoleGameOverReason.KidsAliveAlone:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Delinquent);
				break;
			case RoleGameOverReason.HatterEndlessTeaTime:
			case RoleGameOverReason.HatterTeaPartyTime:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Hatter);
				break;
			case RoleGameOverReason.ArtistShipToArt:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Artist);
				break;
			case RoleGameOverReason.TuckerShipIsExperimentStation:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Tucker, ExtremeRoleId.Chimera);
				break;
			case RoleGameOverReason.MonikaThisGameIsMine:
			case RoleGameOverReason.MonikaIamTheOnlyOne:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					true, ExtremeRoleId.Monika);
				break;
			case RoleGameOverReason.AllJackalWin:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Shepherd);
				processor.CollectAllTargetRole(
					ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick, ExtremeRoleId.Furry);
				break;
			case RoleGameOverReason.AllYandereWin:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Intimate);
				processor.CollectAllTargetRole(
					ExtremeRoleId.Yandere, ExtremeRoleId.Surrogator);
				break;
			case RoleGameOverReason.AllQueenWin:
				processor.ReplaceWinnerToSpecificNeutralRolePlayer(
					ExtremeRoleId.Knight);
				processor.CollectAllTargetRole(
					ExtremeRoleId.Queen, ExtremeRoleId.Servant, ExtremeRoleId.Pawn);
				break;
			default:
				break;
		}
	}

	private void replaceWinnerToImpPlayer(WinnerContainer winner)
	{
		ExtremeRolesPlugin.Logger.LogInfo("Clear Winner(Reason:Imp Special Win)");
		winner.Clear();
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
				role.IsImpostor())
			{
				winner.Add(player);
			}
		}
	}

	private void replaceWinnerToLiberalPlayer(WinnerContainer winner)
	{
		ExtremeRolesPlugin.Logger.LogInfo("Clear Winner(Reason:Liberal Win)");
		winner.Clear();
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) && role.IsLiberal())
			{
				winner.Add(player);
			}
		}
	}
}
