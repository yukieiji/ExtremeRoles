using System.Collections.Generic;


using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Neutral;

#nullable enable

namespace ExtremeRoles.Module.SpecialWinChecker;

internal sealed class MonikaAliveWinChecker : IWinChecker
{
	public RoleGameOverReason Reason => RoleGameOverReason.MonikaIamTheOnlyOne;

	private int aliveNum = 0;

	public void AddAliveRole(
		byte playerId, SingleRoleBase role)
	{
		++this.aliveNum;
	}

	public bool IsWin(
		PlayerStatistics statistics)
	{
		if (this.aliveNum > 1 ||
			this.aliveNum <= 0 ||
			!MonikaTrashSystem.TryGet(out var system))
		{
			return false;
		}

		int monika = 0;
		int aliveNum = statistics.TotalAlive;
		NetworkedPlayerInfo? alivePlayer = null;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player == null ||
				player.IsDead ||
				player.Disconnected)
			{
				continue;
			}
			// ゴミ箱のプレイヤー
			if (system.InvalidPlayer(player))
			{
				aliveNum--;
			}
			else if (
				ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
				role.Core.Id is ExtremeRoleId.Monika)
			{
				++monika;
			}
			else
			{
				alivePlayer = player;
			}
		}

		aliveNum -= monika;

		if (monika > 1)
		{
			return false;
		}
		// モニカしかいないのでモニカの強制勝利
		else if (aliveNum == 0)
		{
			return true; // 0
		}
		// 1人しかいないので会議を起こさずその人と強制勝利
		else if (aliveNum == 1 && alivePlayer != null)
		{
			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.SetWinPlayer))
			{
				caller.WriteInt(1);
				caller.WriteByte(alivePlayer.PlayerId);
			}
			ExtremeRolesPlugin.ShipState.AddWinner(alivePlayer);
			return true;
		}
		else
		{
			return false;
		}
	}
}
