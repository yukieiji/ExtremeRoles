using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;

using Player = NetworkedPlayerInfo;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Module.GameResult;

#nullable enable

public sealed class WinnerBuilder : IDisposable
{
	private readonly WinnerTempData tempData;
	private readonly List<Player> neutralNoWinner = [];
	private readonly List<(Player, IRoleWinPlayerModifier)> modRole = [];
	private readonly List<(Player, IGhostRoleWinable)> ghostWinCheckRole = [];
	private readonly FinalSummaryBuilder finalSummaryBuilder;
	private readonly int winGameControlId;

	private readonly GameOverReason gameOverReason;
	private readonly RoleGameOverReason roleGameOverReason;

	public WinnerBuilder(
		int winGameControlId,
		WinnerTempData tempData,
		IReadOnlyDictionary<byte, ExtremeGameResultManager.TaskInfo> taskInfo)
	{
		this.winGameControlId = winGameControlId;
		this.tempData = tempData;

		var state = ExtremeRolesPlugin.ShipState;
		this.gameOverReason = state.EndReason;
		this.roleGameOverReason = (RoleGameOverReason)this.gameOverReason;

		this.finalSummaryBuilder = new FinalSummaryBuilder(
			this.gameOverReason,
			state.DeadPlayerInfo,
			taskInfo);

		int playerNum = GameData.Instance.AllPlayers.Count;

		this.neutralNoWinner.Capacity = playerNum;
		this.modRole.Capacity = playerNum;
		this.ghostWinCheckRole.Capacity = playerNum;
	}

	public IReadOnlyList<FinalSummary.PlayerSummary> Build()
	{
		var logger = ExtremeRolesPlugin.Logger;

		string resonStr = Enum.IsDefined(this.roleGameOverReason) ?
			this.roleGameOverReason.ToString() : this.gameOverReason.ToString();
		logger.LogInfo($"GameEnd : {resonStr}");

		if (this.roleGameOverReason is RoleGameOverReason.UmbrerBiohazard)
		{
			// アンブレ用のやつを追加
		}

		logger.LogInfo("---- Start: Creating Winner ----");

		var summaries = this.initialize();

		removeAddPlusWinner();
		addNeutralWiner();
		replaceWinner();
		mergeWinner();
		addGhostRoleWinner();
		modifiedWinner();

		logger.LogInfo("--- End: Creating Winner ----");

#if DEBUG
		logger.LogInfo(this.tempData.ToString());
#endif
		return summaries;
	}

	private List<FinalSummary.PlayerSummary> initialize()
	{
		var summaries = new List<FinalSummary.PlayerSummary>(this.neutralNoWinner.Capacity);
		var logger = ExtremeRolesPlugin.Logger;

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
					this.tempData.AddPlusWinner(playerInfo);
				}
				else
				{
					this.neutralNoWinner.Add(playerInfo);
				}
				logger.LogInfo($"Remove Winner(Reason:Neutral) : {playerName}");
				this.tempData.Remove(playerInfo);
			}
			else if (role.Id == ExtremeRoleId.Xion)
			{
				logger.LogInfo($"Remove Winner(Reason:Xion Player) : {playerName}");
				this.tempData.Remove(playerInfo);
			}

			if (role is IRoleWinPlayerModifier winModRole)
			{
				this.modRole.Add((playerInfo, winModRole));
			}

			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is IRoleWinPlayerModifier multiWinModRole)
			{
				this.modRole.Add((playerInfo, multiWinModRole));
			}

			if (ExtremeGhostRoleManager.GameRole.TryGetValue(
					playerId, out GhostRoleBase? ghostRole) &&
				ghostRole is not null &&
				ghostRole.IsNeutral() &&
				ghostRole is IGhostRoleWinable winCheckGhostRole)
			{
				this.ghostWinCheckRole.Add((playerInfo, winCheckGhostRole));
			}

			var summary = this.finalSummaryBuilder.Create(
				playerInfo, role, ghostRole);
			if (summary.HasValue)
			{
				summaries.Add(summary.Value);
			}
		}

		return summaries;
	}

	private void removeAddPlusWinner()
	{
		foreach (Player winner in this.tempData.PlusedWinner)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Remove Winner(Dupe Player) : {winner.PlayerName}");
			this.tempData.Remove(winner);
		}
	}

	private void addNeutralWiner()
	{
		if (!ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd)
		{
			return;
		}

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
			this.tempData.Add(playerInfo);
			return true;
		}
		else if (role.IsNeutral() && role.IsWin)
		{
			winRole.Add(item);

			logger.LogInfo($"Add Winner(Reason:Additional Neutral Win) : {playerInfo.PlayerName}");
			this.tempData.Add(playerInfo);
			return true;
		}
		return false;
	}

	private void replaceWinner()
	{
		switch (this.roleGameOverReason)
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
			default:
				break;
		}
	}

	private void replaceWinnerToSpecificNeutralRolePlayer(
		params ExtremeRoleId[] roles)
	{
		ExtremeRolesPlugin.Logger.LogInfo("Clear Winner(Reason:Neautal Win)");
		this.tempData.Clear();

		foreach (var player in this.neutralNoWinner)
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

	private void addSpecificRoleToSameControlIdPlayer(in SingleRoleBase role, in Player player)
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

	private void addSpecificNeutralRoleToSameControlIdPlayer(in SingleRoleBase role, in Player player)
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

	private void mergeWinner()
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo($"-- Start: merge plused win player --");
		foreach (var player in this.tempData.PlusedWinner)
		{
			logger.LogInfo($"marge to winner:{player.PlayerName}");
			this.tempData.Add(player);
		}
		logger.LogInfo($"-- End: merge plused win player --");
	}

	private void addGhostRoleWinner()
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo($"-- Start: add ghostrole win player --");

		foreach (var (playerInfo, winCheckRole) in this.ghostWinCheckRole)
		{
			if (winCheckRole.IsWin(this.gameOverReason, playerInfo))
			{
				ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Ghost Role win) : {playerInfo.PlayerName}");
				this.tempData.AddWithPlus(playerInfo);
			}
		}
		logger.LogInfo($"-- End: add ghostrole win player --");
	}

	private void modifiedWinner()
	{
		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo($"-- Start: modified win player --");
		foreach (var (playerInfo, winModRole) in modRole)
		{
			winModRole.ModifiedWinPlayer(
				playerInfo,
				 ExtremeRolesPlugin.ShipState.EndReason, // 更新され続けるため、新しいのを常に渡す
				in this.tempData);
		}
		logger.LogInfo($"-- End: modified win player --");
	}

	public void Dispose()
	{
		this.finalSummaryBuilder.Dispose();
	}
}
