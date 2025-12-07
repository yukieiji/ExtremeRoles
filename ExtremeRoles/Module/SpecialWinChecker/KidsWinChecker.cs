using System.Collections.Generic;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;

namespace ExtremeRoles.Module.SpecialWinChecker;

internal sealed class KidsWinChecker : IWinChecker
{
	public RoleGameOverReason Reason => RoleGameOverReason.KidsTooBigHomeAlone;

	private readonly Dictionary<byte, Delinquent> aliveDelinquent = new Dictionary<byte, Delinquent>();

	public KidsWinChecker()
	{
		aliveDelinquent.Clear();
	}

	public void AddAliveRole(
		byte playerId, SingleRoleBase role)
	{
		aliveDelinquent.Add(playerId, (Delinquent)role);
	}

	public bool IsWin(PlayerStatistics statistics)
	{
		byte checkPlayerId = byte.MaxValue;
		float range = float.MinValue;
		Delinquent checkRole = null;
		foreach (var (playerId, role) in aliveDelinquent)
		{
			if (role.WinCheckEnable)
			{
				checkPlayerId = playerId;
				range = role.Range;
				checkRole = role;
				break;
			}
		}

		if (checkPlayerId == byte.MaxValue)
		{
			return false;
		}

		var player = Player.GetPlayerControlById(checkPlayerId);
		if (player == null)
		{
			return false;
		}

		List<PlayerControl> rangeInPlayer = Player.GetAllPlayerInRange(
			player, checkRole, range);

		int gameControlId = 
			ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin ? 
			PlayerStatistics.SameNeutralGameControlId : checkRole.GameControlId;

		int teamAlive = statistics.SeparatedNeutralAlive[
			new (NeutralSeparateTeam.Kids, gameControlId)];
		int allAlive = statistics.TotalAlive;

		// 全生存者からアサシンと範囲内にいるプレイヤー、生きている無敵のリーダー(1人しか存在しない)、同じチームの人を除く
		bool isWin = (allAlive - statistics.AssassinAlive - rangeInPlayer.Count - (statistics.LeaderIsBlockKill ? 1 : 0) - teamAlive) <= 0;
		if (isWin)
		{
			checkRole.BlockWispAssign();
		}

		var deadPlayerId = new HashSet<byte>();
		foreach (var target in rangeInPlayer)
		{
			byte targetId = target.PlayerId;
			if (!ExtremeRoleManager.TryGetRole(targetId, out var role))
			{
				continue;
			}

			// 既にキルした人、アサシン、無敵のリーダーは除く
			if (deadPlayerId.Contains(targetId) ||
				role.Core.Id is ExtremeRoleId.Assassin ||
				(role.Core.Id is ExtremeRoleId.Leader && statistics.LeaderIsBlockKill) ||
				targetId == checkPlayerId)
			{
				continue;
			}

			Player.RpcUncheckMurderPlayer(
				checkPlayerId, targetId, byte.MinValue);
			ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
				targetId, ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.Explosion);
			deadPlayerId.Add(targetId);
		}

		Player.RpcUncheckMurderPlayer(
			checkPlayerId, checkPlayerId, byte.MinValue);
		ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
			checkPlayerId, ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.Explosion);

		return isWin;
	}
}
