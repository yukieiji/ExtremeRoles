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


using TempWinData = Il2CppSystem.Collections.Generic.List<WinningPlayerData>;
using Player = GameData.PlayerInfo;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class ExtremeGameResult : NullableSingleton<ExtremeGameResult>
{
	public readonly record struct WinnerResult(
		IReadOnlyList<WinningPlayerData> Winner,
		IReadOnlyList<Player> PlusedWinner);

	public readonly record struct TaskInfo(int CompletedTask, int TotalTask);
	public class WinnerTempData
	{
		public TempWinData DefaultWinPlayer { get; init; }

		public IReadOnlyList<Player> PlusedWinner => this.plusWinPlayr;

		private readonly List<WinningPlayerData> finalWinPlayer;
		private readonly List<Player> plusWinPlayr;

		public WinnerTempData()
		{
			this.DefaultWinPlayer = TempData.winners;
			this.finalWinPlayer = new List<WinningPlayerData>(this.DefaultWinPlayer.ToArray());
			this.plusWinPlayr = ExtremeRolesPlugin.ShipState.GetPlusWinner();
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder
				.AppendLine("---- Current Win data ----")
				.AppendLine("--- Default Winner ---");

			foreach (var winner in this.DefaultWinPlayer)
			{
				builder.AppendLine($"PlayerName:{winner.PlayerName}");
			}

			builder.AppendLine("--- Final Winner ---");
			foreach (var winner in this.finalWinPlayer)
			{
				builder.AppendLine($"PlayerName:{winner.PlayerName}");
			}

			builder.AppendLine("--- Plus Winner ---");
			foreach (var winner in this.plusWinPlayr)
			{
				builder.AppendLine($"PlayerName:{winner.PlayerName}");
			}

			return builder.ToString();
		}

		public WinnerResult Convert() => new WinnerResult(this.finalWinPlayer, plusWinPlayr);

		public void AllClear()
		{
			this.finalWinPlayer.Clear();
			this.plusWinPlayr.Clear();
		}

		public void Clear()
		{
			this.finalWinPlayer.Clear();
		}

		public void RemoveAll(Player playerInfo)
		{
			this.plusWinPlayr.RemoveAll(x => x.PlayerName == playerInfo.PlayerName);
			Remove(playerInfo);
		}
		public void Remove(WinningPlayerData player)
		{
			this.finalWinPlayer.RemoveAll(x => x.PlayerName == player.PlayerName);
		}
		public void Remove(Player player)
		{
			this.finalWinPlayer.RemoveAll(x => x.PlayerName == player.PlayerName);
		}

		public void AddWithPlus(Player playerInfo)
		{
			this.Add(playerInfo);
			this.AddPlusWinner(playerInfo);
		}

		public void Add(Player playerInfo)
		{
			WinningPlayerData wpd = new WinningPlayerData(playerInfo);
			this.finalWinPlayer.Add(wpd);
		}
		public void AddPlusWinner(Player player)
		{
			this.plusWinPlayr.Add(player);
		}

		public bool Contains(string name)
		{
			foreach (var win in this.finalWinPlayer)
			{
				if (win.PlayerName == name)
				{
					return true;
				}
			}

			foreach (var win in this.plusWinPlayr)
			{
				if (win.PlayerName == name)
				{
					return true;
				}
			}
			return false;
		}
	}

	public WinnerResult Winner => this.winner.Convert();
	public List<FinalSummary.PlayerSummary> PlayerSummaries { get; init; }

	private readonly int winGameControlId;
	private readonly Dictionary<byte, TaskInfo> playerTaskInfo = new Dictionary<byte, TaskInfo>();
	private WinnerTempData winner;

	public ExtremeGameResult()
	{
		this.winner = new WinnerTempData();
		this.PlayerSummaries = new List<FinalSummary.PlayerSummary>();
		this.winGameControlId = ExtremeRolesPlugin.ShipState.WinGameControlId;
	}

	public void CreateTaskInfo()
	{
		foreach (Player playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			var (completedTask, totalTask) = GameSystem.GetTaskInfo(playerInfo);
			this.playerTaskInfo.Add(
				playerInfo.PlayerId,
				new TaskInfo(completedTask, totalTask));
		}
	}

	public void CreateEndGameManagerResult()
	{
		this.PlayerSummaries.Clear();

		var logger = ExtremeRolesPlugin.Logger;

		int playerNum = GameData.Instance.AllPlayers.Count;

		this.PlayerSummaries.Capacity = playerNum;

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
			var role = roleData[playerInfo.PlayerId];

			string playerName = playerInfo.PlayerName;
			if (role.IsNeutral())
			{
				if (ExtremeRoleManager.IsAliveWinNeutral(role, playerInfo))
				{
					logger.LogInfo($"AddPlusWinner(Reason:Alive Win) : {playerName}");
					this.winner.AddPlusWinner(playerInfo);
				}
				else
				{
					neutralNoWinner.Add(playerInfo);
				}
				logger.LogInfo($"Remove Winner(Reason:Neutral) : {playerName}");
				this.winner.Remove(playerInfo);
			}
			else if (role.Id == ExtremeRoleId.Xion)
			{
				logger.LogInfo($"Remove Winner(Reason:Xion Player) : {playerName}");
				this.winner.Remove(playerInfo);
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
					playerInfo.PlayerId, out GhostRoleBase? ghostRole) &&
				ghostRole is not null &&
				ghostRole.IsNeutral() &&
				ghostRole is IGhostRoleWinable winCheckGhostRole)
			{
				ghostWinCheckRole.Add((playerInfo, winCheckGhostRole));
			}

			if (this.playerTaskInfo.TryGetValue(
					playerInfo.PlayerId, out var taskInfo))
			{
				var summary =
					FinalSummary.PlayerSummary.Create(
						playerInfo, role, ghostRole, taskInfo);

				this.PlayerSummaries.Add(summary);
			}
		}

		foreach (Player winner in this.winner.PlusedWinner)
		{
			logger.LogInfo($"Remove Winner(Dupe Player) : {winner.PlayerName}");
			this.winner.Remove(winner);
		}

		if (ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd)
		{
			addNeutralWinner();
		}

		switch (modReason)
		{
			case RoleGameOverReason.AssassinationMarin:
			case RoleGameOverReason.TeroristoTeroWithShip:
				this.winner.Clear();
				foreach (Player player in GameData.Instance.AllPlayers.GetFastEnumerator())
				{
					if (ExtremeRoleManager.GameRole[player.PlayerId].IsImpostor())
					{
						this.winner.Add(player);
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
			default:
				break;
		}

		logger.LogInfo($"-- Start: merge plused win player --");
		foreach (var player in this.winner.PlusedWinner)
		{
			logger.LogInfo($"marge to winner:{player.PlayerName}");
			this.winner.Add(player);
		}
		logger.LogInfo($"-- End: merge plused win player --");

		foreach (var (playerInfo, winCheckRole) in ghostWinCheckRole)
		{
			if (winCheckRole.IsWin(reason, playerInfo))
			{
				logger.LogInfo($"Add Winner(Reason:Ghost Role win) : {playerInfo.PlayerName}");
				this.winner.AddPlusWinner(playerInfo);
			}
		}

		logger.LogInfo($"-- Start: modified win player --");
		foreach (var (playerInfo, winModRole) in modRole)
		{
			winModRole.ModifiedWinPlayer(
				playerInfo,
				gameData.EndReason,
				ref this.winner);
		}
		logger.LogInfo($"-- End: modified win player --");


		logger.LogInfo("--- End: Creating Winner ----");
#if DEBUG
		logger.LogInfo(this.winner.ToString());
#endif
	}

	private void replaceWinnerToSpecificRolePlayer(
		ExtremeRoleId roleId)
	{
		this.winner.Clear();

		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			var role = ExtremeRoleManager.GameRole[player.PlayerId];

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
		this.winner.Clear();

		foreach (var player in neutralNoWinner)
		{
			var role = ExtremeRoleManager.GameRole[player.PlayerId];

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
		List<(ExtremeRoleId, int)> winRole = new List<(ExtremeRoleId, int)>();

		foreach (Player playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			var role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];

			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is not null &&
				tryAddWinRole(
					multiAssignRole.AnotherRole,
						playerInfo, ref winRole))
			{
				continue;
			}
			tryAddWinRole(role, playerInfo, ref winRole);
		}
	}

	private bool tryAddWinRole(
		in SingleRoleBase role,
		in Player playerInfo,
		ref List<(ExtremeRoleId, int)> winRole)
	{
		int gameControlId = role.GameControlId;

		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			gameControlId = PlayerStatistics.SameNeutralGameControlId;
		}

		var logger = ExtremeRolesPlugin.Logger;

		if (winRole.Contains((role.Id, gameControlId)))
		{
			logger.LogInfo($"Add Winner(Reason:Additional Neutral Win) : {playerInfo.PlayerName}");
			this.winner.Add(playerInfo);
			return true;
		}
		else if (role.IsNeutral() && role.IsWin)
		{
			winRole.Add((role.Id, gameControlId));

			logger.LogInfo($"Add Winner(Reason:Additional Neutral Win) : {playerInfo.PlayerName}");
			this.winner.Add(playerInfo);
			return true;
		}
		return false;
	}

	private void addSpecificRoleToSameControlIdPlayer(in SingleRoleBase role, in Player player)
	{
		if (this.winGameControlId != int.MaxValue &&
			this.winGameControlId == role.GameControlId)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Win this role) : {player.PlayerName}");
			this.winner.Add(player);
		}
	}

	private void addSpecificNeutralRoleToSameControlIdPlayer(in SingleRoleBase role, in Player player)
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
}
