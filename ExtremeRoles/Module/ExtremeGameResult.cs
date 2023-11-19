using ExtremeRoles.GameMode;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TempWinData = Il2CppSystem.Collections.Generic.List<WinningPlayerData>;
using Player = GameData.PlayerInfo;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;


#nullable enable

namespace ExtremeRoles.Module;

public sealed class ExtremeGameResult
{
	public readonly record struct WinnerResult(
		IReadOnlyList<WinningPlayerData> Winner,
		IReadOnlyList<Player> PlusedWinner);

	public class WinnerTempData
	{
		public TempWinData DefaultWinPlayer { get; init; }

		private List<WinningPlayerData> finalWinPlayer;
		private List<Player> plusWinPlayr;

		public WinnerTempData()
		{
			this.DefaultWinPlayer = TempData.winners;
			this.finalWinPlayer = new List<WinningPlayerData>(this.DefaultWinPlayer.ToArray());
			this.plusWinPlayr = ExtremeRolesPlugin.ShipState.GetPlusWinner();
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
			this.plusWinPlayr.Remove(playerInfo);
			WinningPlayerData wpd = new WinningPlayerData(playerInfo);
			this.finalWinPlayer.Remove(wpd);
		}

		public void Remove(WinningPlayerData player)
		{
			this.finalWinPlayer.Remove(player);
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
	private readonly int winGameControlId;
	public WinnerResult Winner => this.winner.Convert();
	public List<FinalSummary.PlayerSummary> PlayerSummaries { get; init; }

	private WinnerTempData winner;

	public ExtremeGameResult()
	{
		this.PlayerSummaries = new List<FinalSummary.PlayerSummary>();
		this.winner = new WinnerTempData();
		this.winGameControlId = ExtremeRolesPlugin.ShipState.WinGameControlId;
		this.Build();
	}

	public void Build()
	{
		this.PlayerSummaries.Clear();

		int playerNum = GameData.Instance.AllPlayers.Count;

		var noWinner = new List<Player>(playerNum);
		var modRole = new List<(Player, IRoleWinPlayerModifier)>(playerNum);
		var ghostWinCheckRole = new List<(Player, IGhostRoleWinable)>(playerNum);

		var roleData = ExtremeRoleManager.GameRole;
		var gameData = ExtremeRolesPlugin.ShipState;

		foreach (Player playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{

			var role = roleData[playerInfo.PlayerId];

			if (role.IsNeutral())
			{
				if (ExtremeRoleManager.IsAliveWinNeutral(role, playerInfo))
				{
					gameData.AddWinner(playerInfo);
				}
				else
				{
					noWinner.Add(playerInfo);
				}
			}
			else if (role.Id == ExtremeRoleId.Xion)
			{
				noWinner.Add(playerInfo);
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

			this.PlayerSummaries.Add(
				FinalSummary.PlayerSummary.Create(
					playerInfo, role, ghostRole));
		}

		List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>(playerNum);
		List<Player> plusWinner = gameData.GetPlusWinner();
		foreach (WinningPlayerData winner in this.winner.DefaultWinPlayer.GetFastEnumerator())
		{
			if (noWinner.Any(x => x.PlayerName == winner.PlayerName) ||
				plusWinner.Any(x => x.PlayerName == winner.PlayerName))
			{
				this.winner.Remove(winner);
			}
		}

		if (ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd)
		{
			addNeutralWinner();
		}

		GameOverReason reason = gameData.EndReason;

		switch ((RoleGameOverReason)reason)
		{
			case RoleGameOverReason.AssassinationMarin:
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
					noWinner, ExtremeRoleId.Alice);
				break;
			case RoleGameOverReason.JackalKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Jackal, ExtremeRoleId.Sidekick);
				break;
			case RoleGameOverReason.LoverKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.ShipFallInLove:
				replaceWinnerToSpecificRolePlayer(
					ExtremeRoleId.Lover);
				break;
			case RoleGameOverReason.TaskMasterGoHome:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.TaskMaster);
				break;
			case RoleGameOverReason.MissionaryAllAgainstGod:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Missionary);
				break;
			case RoleGameOverReason.JesterMeetingFavorite:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Jester);
				break;
			case RoleGameOverReason.YandereKillAllOther:
			case RoleGameOverReason.YandereShipJustForTwo:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Yandere);
				break;
			case RoleGameOverReason.VigilanteKillAllOther:
			case RoleGameOverReason.VigilanteNewIdealWorld:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Vigilante);
				break;
			case RoleGameOverReason.MinerExplodeEverything:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Miner);
				break;
			case RoleGameOverReason.EaterAllEatInTheShip:
			case RoleGameOverReason.EaterAliveAlone:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Eater);
				break;
			case RoleGameOverReason.TraitorKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Traitor);
				break;
			case RoleGameOverReason.QueenKillAllOther:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Queen, ExtremeRoleId.Servant);
				break;
			case RoleGameOverReason.UmbrerBiohazard:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Umbrer);
				break;
			case RoleGameOverReason.KidsTooBigHomeAlone:
			case RoleGameOverReason.KidsAliveAlone:
				replaceWinnerToSpecificNeutralRolePlayer(
					noWinner, ExtremeRoleId.Delinquent);
				break;
			default:
				break;
		}

		foreach (var player in gameData.GetPlusWinner())
		{
			this.winner.Add(player);
		}

		foreach (var (playerInfo, winCheckRole) in ghostWinCheckRole)
		{
			if (winCheckRole.IsWin(reason, playerInfo))
			{
				this.winner.AddPlusWinner(playerInfo);
			}
		}

		foreach (var (playerInfo, winModRole) in modRole)
		{
			winModRole.ModifiedWinPlayer(
				playerInfo,
				gameData.EndReason,
				ref this.winner);
		}
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
		List<Player> noWinner, params ExtremeRoleId[] roles)
	{
		this.winner.Clear();

		foreach (var player in noWinner)
		{
			var role = ExtremeRoleManager.GameRole[player.PlayerId];

			if (roles.Contains(role.Id))
			{
				addSpecificNeutralRoleToSameControlIdPlayer(role, player);
			}
			else if (
				role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole != null &&
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
				checkAndAddWinRole(
					multiAssignRole.AnotherRole,
						playerInfo, ref winRole))
			{
				continue;
			}
			checkAndAddWinRole(role, playerInfo, ref winRole);
		}
	}

	private bool checkAndAddWinRole(
		SingleRoleBase role,
		Player playerInfo,
		ref List<(ExtremeRoleId, int)> winRole)
	{
		int gameControlId = role.GameControlId;

		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			gameControlId = PlayerStatistics.SameNeutralGameControlId;
		}

		if (winRole.Contains((role.Id, gameControlId)))
		{
			this.winner.Add(playerInfo);
			return true;
		}
		else if (role.IsNeutral() && role.IsWin)
		{
			winRole.Add((role.Id, gameControlId));
			this.winner.Add(playerInfo);
			return true;
		}
		return false;
	}

	private void addSpecificRoleToSameControlIdPlayer(SingleRoleBase role, Player player)
	{
		if (this.winGameControlId != int.MaxValue &&
			this.winGameControlId == role.GameControlId)
		{
			this.winner.Add(player);
		}
	}

	private void addSpecificNeutralRoleToSameControlIdPlayer(SingleRoleBase role, Player player)
	{
		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			this.winner.Add(player);
		}
		else if (
			(this.winGameControlId != int.MaxValue) &&
			(this.winGameControlId == role.GameControlId))
		{
			this.winner.Add(player);
		}
	}
}
