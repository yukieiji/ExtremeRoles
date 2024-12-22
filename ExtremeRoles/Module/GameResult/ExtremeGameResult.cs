using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance.Il2Cpp;


using TempWinData = Il2CppSystem.Collections.Generic.List<CachedPlayerData>;
using Player = NetworkedPlayerInfo;

#nullable enable

namespace ExtremeRoles.Module.GameResult;

public sealed class ExtremeGameResultManager : NullableSingleton<ExtremeGameResultManager>
{
	public readonly record struct TaskInfo(int CompletedTask, int TotalTask);

	public WinnerTempData.Result Winner => winner.Convert();
	public List<FinalSummary.PlayerSummary> PlayerSummaries { get; init; }

	private readonly int winGameControlId;
	private readonly Dictionary<byte, TaskInfo> playerTaskInfo = new Dictionary<byte, TaskInfo>();
	private WinnerTempData winner;

	public ExtremeGameResultManager()
	{
		winner = new WinnerTempData();
		PlayerSummaries = new List<FinalSummary.PlayerSummary>();
		winGameControlId = ExtremeRolesPlugin.ShipState.WinGameControlId;
	}

	public void CreateTaskInfo()
	{
		foreach (Player playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			var (completedTask, totalTask) = GameSystem.GetTaskInfo(playerInfo);
			playerTaskInfo.Add(
				playerInfo.PlayerId,
				new TaskInfo(completedTask, totalTask));
			winner.AddPool(playerInfo);
		}
	}

	public void CreateEndGameManagerResult()
	{
		PlayerSummaries.Clear();
		winner.SetWinner();

		var logger = ExtremeRolesPlugin.Logger;

		int playerNum = GameData.Instance.AllPlayers.Count;

		PlayerSummaries.Capacity = playerNum;

		var neutralNoWinner = new List<Player>(playerNum);
		var modRole = new List<(Player, IRoleWinPlayerModifier)>(playerNum);
		var ghostWinCheckRole = new List<(Player, IGhostRoleWinable)>(playerNum);

		var roleData = ExtremeRoleManager.GameRole;
		var gameData = ExtremeRolesPlugin.ShipState;

		GameOverReason reason = gameData.EndReason;
		RoleGameOverReason modReason = (RoleGameOverReason)reason;

		string resonStr = Enum.IsDefined(modReason) ? modReason.ToString() : reason.ToString();
		logger.LogInfo($"GameEnd : {resonStr}");

		logger.LogInfo("---- Start: Creating Winner ----");

		foreach (Player playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			byte playerId = playerInfo.PlayerId;
			if (!ExtremeRoleManager.TryGetRole(playerId, out var role))
			{
				continue;
			}

			string playerName = playerInfo.PlayerName;
			if (role.IsNeutral())
			{
				if (ExtremeRoleManager.IsAliveWinNeutral(role, playerInfo))
				{
					logger.LogInfo($"AddPlusWinner(Reason:Alive Win) : {playerName}");
					winner.AddPlusWinner(playerInfo);
				}
				else
				{
					neutralNoWinner.Add(playerInfo);
				}
				logger.LogInfo($"Remove Winner(Reason:Neutral) : {playerName}");
				winner.Remove(playerInfo);
			}
			else if (role.Id == ExtremeRoleId.Xion)
			{
				logger.LogInfo($"Remove Winner(Reason:Xion Player) : {playerName}");
				winner.Remove(playerInfo);
			}

			if (role is IRoleWinPlayerModifier winModRole)
			{
				modRole.Add((playerInfo, winModRole));
			}

			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is IRoleWinPlayerModifier multiWinModRole)
			{
				modRole.Add((playerInfo, multiWinModRole));
			}

			if (ExtremeGhostRoleManager.GameRole.TryGetValue(
					playerId, out GhostRoleBase? ghostRole) &&
				ghostRole is not null &&
				ghostRole.IsNeutral() &&
				ghostRole is IGhostRoleWinable winCheckGhostRole)
			{
				ghostWinCheckRole.Add((playerInfo, winCheckGhostRole));
			}

			if (playerTaskInfo.TryGetValue(playerId, out var taskInfo))
			{
				var summary =
					FinalSummary.PlayerSummary.Create(
						playerInfo, role, ghostRole, taskInfo);

				PlayerSummaries.Add(summary);
			}
		}

		foreach (Player winner in winner.PlusedWinner)
		{
			logger.LogInfo($"Remove Winner(Dupe Player) : {winner.PlayerName}");
			this.winner.Remove(winner);
		}

		if (ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd)
		{
			addNeutralWinner();
		}

		replaceWinner(modReason, neutralNoWinner);

		logger.LogInfo($"-- Start: merge plused win player --");
		foreach (var player in winner.PlusedWinner)
		{
			logger.LogInfo($"marge to winner:{player.PlayerName}");
			winner.Add(player);
		}
		logger.LogInfo($"-- End: merge plused win player --");

		foreach (var (playerInfo, winCheckRole) in ghostWinCheckRole)
		{
			if (winCheckRole.IsWin(reason, playerInfo))
			{
				logger.LogInfo($"Add Winner(Reason:Ghost Role win) : {playerInfo.PlayerName}");
				winner.AddWithPlus(playerInfo);
			}
		}

		logger.LogInfo($"-- Start: modified win player --");
		foreach (var (playerInfo, winModRole) in modRole)
		{
			winModRole.ModifiedWinPlayer(
				playerInfo,
				gameData.EndReason,
				in winner);
		}
		logger.LogInfo($"-- End: modified win player --");


		logger.LogInfo("--- End: Creating Winner ----");
#if DEBUG
		logger.LogInfo(winner.ToString());
#endif
	}

	private void replaceWinner(RoleGameOverReason reason, in IReadOnlyList<Player> neutralNoWinner)
	{
		switch (reason)
		{
			case RoleGameOverReason.AssassinationMarin:
			case RoleGameOverReason.TeroristoTeroWithShip:
				winner.Clear();
				foreach (Player player in GameData.Instance.AllPlayers.GetFastEnumerator())
				{
					if (ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
						role.IsImpostor())
					{
						winner.Add(player);
					}
				}
				break;
			case RoleGameOverReason.AliceKilledByImposter:
			case RoleGameOverReason.AliceKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Alice);
				break;
			case RoleGameOverReason.JackalKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick);
				break;
			case RoleGameOverReason.LoverKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.ShipFallInLove:
				replaceWinnerToSpecificRolePlayer(
					ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.TaskMasterGoHome:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.TaskMaster);
				break;
			case RoleGameOverReason.MissionaryAllAgainstGod:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Missionary);
				break;
			case RoleGameOverReason.JesterMeetingFavorite:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Jester);
				break;
			case RoleGameOverReason.YandereKillAllOther:
			case RoleGameOverReason.YandereShipJustForTwo:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Yandere);
				break;
			case RoleGameOverReason.VigilanteKillAllOther:
			case RoleGameOverReason.VigilanteNewIdealWorld:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Vigilante);
				break;
			case RoleGameOverReason.MinerExplodeEverything:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Miner);
				break;
			case RoleGameOverReason.EaterAllEatInTheShip:
			case RoleGameOverReason.EaterAliveAlone:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Eater);
				break;
			case RoleGameOverReason.TraitorKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Traitor);
				break;
			case RoleGameOverReason.QueenKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Queen, ExtremeRoleId.Servant);
				break;
			case RoleGameOverReason.UmbrerBiohazard:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Umbrer);
				break;
			case RoleGameOverReason.KidsTooBigHomeAlone:
			case RoleGameOverReason.KidsAliveAlone:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Delinquent);
				break;
			case RoleGameOverReason.HatterEndlessTeaTime:
			case RoleGameOverReason.HatterTeaPartyTime:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Hatter);
				break;
			case RoleGameOverReason.ArtistShipToArt:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Artist);
				break;
			case RoleGameOverReason.TuckerShipIsExperimentStation:
				replaceWinnerToSpecificNeutralRolePlayer(
					neutralNoWinner, ExtremeRoleId.Tucker, ExtremeRoleId.Chimera);
				break;
			default:
				break;
		}
	}

	private void replaceWinnerToSpecificRolePlayer(
		ExtremeRoleId roleId)
	{
		winner.Clear();

		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role))
			{
				continue;
			}

			if (role.Id == roleId)
			{
				addSpecificRoleToSameControlIdPlayer(role, player);
			}
			else if (
				role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is not null &&
				multiAssignRole.AnotherRole.Id == roleId)
			{
				addSpecificRoleToSameControlIdPlayer(multiAssignRole.AnotherRole, player);
			}
		}
	}

	private void replaceWinnerToSpecificNeutralRolePlayer(
		in IReadOnlyList<Player> neutralNoWinner, params ExtremeRoleId[] roles)
	{
		ExtremeRolesPlugin.Logger.LogInfo("Clear Winner(Reason:Neautal Win)");
		winner.Clear();

		foreach (var player in neutralNoWinner)
		{
			if (!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role))
			{
				continue;
			}

			if (roles.Contains(role.Id))
			{
				addSpecificNeutralRoleToSameControlIdPlayer(role, player);
			}
			else if (
				role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is not null &&
				roles.Contains(multiAssignRole.AnotherRole.Id))
			{
				addSpecificNeutralRoleToSameControlIdPlayer(role, player);
			}
		}
	}

	private void addNeutralWinner()
	{
		HashSet<(ExtremeRoleId, int)> winRole = new HashSet<(ExtremeRoleId, int)>();

		foreach (Player playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (!ExtremeRoleManager.TryGetRole(playerInfo.PlayerId, out var role))
			{
				continue;
			}

			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is not null &&
				tryAddWinRole(
					multiAssignRole.AnotherRole,
						playerInfo, in winRole))
			{
				continue;
			}
			tryAddWinRole(role, playerInfo, in winRole);
		}
	}

	private bool tryAddWinRole(
		in SingleRoleBase role,
		in Player playerInfo,
		in HashSet<(ExtremeRoleId, int)> winRole)
	{
		int gameControlId = role.GameControlId;

		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			gameControlId = PlayerStatistics.SameNeutralGameControlId;
		}

		var logger = ExtremeRolesPlugin.Logger;
		var item = (role.Id, gameControlId);

		if (winRole.Contains(item))
		{
			logger.LogInfo($"Add Winner(Reason:Additional Neutral Win) : {playerInfo.PlayerName}");
			winner.Add(playerInfo);
			return true;
		}
		else if (role.IsNeutral() && role.IsWin)
		{
			winRole.Add(item);

			logger.LogInfo($"Add Winner(Reason:Additional Neutral Win) : {playerInfo.PlayerName}");
			winner.Add(playerInfo);
			return true;
		}
		return false;
	}

	private void addSpecificRoleToSameControlIdPlayer(in SingleRoleBase role, in Player player)
	{
		if (winGameControlId != int.MaxValue &&
			winGameControlId == role.GameControlId)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Win this role) : {player.PlayerName}");
			winner.Add(player);
		}
	}

	private void addSpecificNeutralRoleToSameControlIdPlayer(in SingleRoleBase role, in Player player)
	{
		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Win this role) : {player.PlayerName}");
			winner.Add(player);
		}
		else
		{
			addSpecificRoleToSameControlIdPlayer(role, player);
		}
	}
}
