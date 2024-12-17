using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class OnemanMeetingSystem : IExtremeSystemType
{
	public byte Caller { get; private set; }
	public byte ExiledTarget { get; set; }

	private readonly Queue<(byte, Type)> meetingQueue = [];

	public enum Type
	{
		Assassin
	}

	public static OnemanMeetingSystem CreateOrGet()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<OnemanMeetingSystem>(
			ExtremeSystemType.OnemanMeetingSystem);

	public void AddQueue(byte playerId, Type meetingType)
	{
		this.meetingQueue.Enqueue((playerId, meetingType));
	}

	public void Start(PlayerControl caller, Type meetingType, PlayerControl? reportTarget = null)
	{
		this.Caller = caller.PlayerId;

		if (reportTarget == null || caller.PlayerId == reportTarget.PlayerId)
		{
			// チェックポイント
			OnemanMeetingCheckpoint.RpcCheckpoint(caller.PlayerId);
		}
		else
		{
			// 発動者を通報で発動
			reportTarget.ReportDeadBody(caller.Data);
		}
	}

	public bool TryStartMeeting()
	{
		if (this.meetingQueue.Count == 0)
		{
			return false;
		}

		var (target, type) = this.meetingQueue.Dequeue();
		return false;
	}

	public bool TryGetGameendReason(out RoleGameOverReason reason)
	{
		reason = RoleGameOverReason.UnKnown;
		return false;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is not ResetTiming.ExiledEnd ||
			TryGetGameendReason(out _))
		{
			return;
		}

		// ストラテジー解除 => null代入

		this.Caller = byte.MaxValue;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		throw new System.NotImplementedException();
	}
}
