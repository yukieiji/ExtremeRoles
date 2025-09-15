using System;
using System.Collections.Generic;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ExtremeShipStatus;

public sealed partial class ExtremeShipStatus
{
	public enum PlayerStatus
	{
		LoveYou = 0,
		Alive,
		Exiled,
		Dead,
		Killed,

		Suicide,
		MissShot,
		Retaliate,
		Departure,
		Martyrdom,
		Eatting,
		Clashed,

		Explosion,

		Assassinate,
		DeadAssassinate,
		Surrender,
		Zombied,

		Disconnected,
	}

	public IReadOnlyDictionary<byte, DeadInfo> DeadPlayerInfo => this.deadPlayerInfo;

	private readonly Dictionary<byte, DeadInfo> deadPlayerInfo = new Dictionary<byte, DeadInfo>();

	public void AddDeadInfo(
		PlayerControl deadPlayer,
		DeathReason reason,
		PlayerControl killer)
	{

		if (this.deadPlayerInfo.ContainsKey(deadPlayer.PlayerId))
		{
			return;
		}

		PlayerStatus newReson = PlayerStatus.Dead;

		switch (reason)
		{
			case DeathReason.Exile:
				newReson = PlayerStatus.Exiled;
				break;
			case DeathReason.Disconnect:
				newReson = PlayerStatus.Disconnected;
				break;
			case DeathReason.Kill:
				newReson = PlayerStatus.Killed;
				if (killer.PlayerId == deadPlayer.PlayerId)
				{
					newReson = PlayerStatus.Suicide;
				}
				break;
			default:
				break;

		}

		this.deadPlayerInfo.Add(
			deadPlayer.PlayerId,
			new DeadInfo(newReson, DateTime.UtcNow, killer));
	}

	public void RemoveDeadInfo(byte targetPlayerId)
	{
		this.deadPlayerInfo.Remove(targetPlayerId);
	}

	public void RpcReplaceDeadReason(
		byte playerId, PlayerStatus newReason)
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.ReplaceDeadReason))
		{
			caller.WriteByte(playerId);
			caller.WriteByte((byte)newReason);
		}
		ReplaceDeadReason(playerId, newReason);
	}

	public void ReplaceDeadReason(
		byte playerId, PlayerStatus newReason)
	{
		if (!this.deadPlayerInfo.TryGetValue(playerId, out var old))
		{
			return;
		}
		this.deadPlayerInfo[playerId] = new DeadInfo(newReason, old.DeadTime, old.Killer);
	}

	private void resetDeadPlayerInfo()
	{
		this.deadPlayerInfo.Clear();
	}

	public readonly record struct DeadInfo(PlayerStatus Reason, DateTime DeadTime, PlayerControl Killer);
}
