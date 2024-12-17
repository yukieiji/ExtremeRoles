using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Roles;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public sealed class OnemanMeetingSystemManager : IExtremeSystemType
{
	public byte Caller { get; private set; }
	public byte ExiledTarget { get; set; }
	private readonly Queue<(byte, Type)> meetingQueue = [];

	private IOnemanMeeting? meeting;

	public enum Type
	{
		Assassin
	}

	public static OnemanMeetingSystemManager CreateOrGet()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<OnemanMeetingSystemManager>(
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

	public bool TryGetOnemanMeeting([NotNullWhen(true)] out IOnemanMeeting? meeting)
	{
		meeting = this.meeting;
		return meeting is not null;
	}

	public bool TryGetOnemanMeeting<T>([NotNullWhen(true)] out T? meeting) where T : class, IOnemanMeeting
	{
		meeting = this.meeting as T;
		return meeting is not null;
	}

	public bool TryStartMeeting()
	{
		if (this.meetingQueue.Count == 0)
		{
			return false;
		}

		var (target, type) = this.meetingQueue.Dequeue();
		this.meeting = create(type);
		return this.meeting is not null && this.meeting.TryStartMeeting(target);
	}

	public bool TryGetGameEndReason(out RoleGameOverReason reason)
	{
		reason = RoleGameOverReason.UnKnown;
		return this.meeting is not null && this.meeting.TryGetGameEndReason(out reason);
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is not ResetTiming.ExiledEnd ||
			TryGetGameEndReason(out _))
		{
			return;
		}
		this.meeting = null;
		this.Caller = byte.MaxValue;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	private static IOnemanMeeting? create(Type type)
		=> null;
}
