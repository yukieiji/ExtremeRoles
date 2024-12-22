﻿using System;
using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Roles;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtremeRoles.Patches.Controller;


#nullable enable

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public sealed class OnemanMeetingSystemManager : IExtremeSystemType
{
	public enum Ops
	{
		SetTarget,
	}

	public byte Caller { get; private set; }

	public static bool IsActive
		=> ExtremeSystemTypeManager.Instance.TryGet<OnemanMeetingSystemManager>(systemType, out var system) &&
		system.meeting is not null;

	private const ExtremeSystemType systemType = ExtremeSystemType.OnemanMeetingSystem;

	private readonly Queue<(byte, Type)> meetingQueue = [];

	private IOnemanMeeting? meeting;

	public enum Type
	{
		Assassin
	}

	public static OnemanMeetingSystemManager CreateOrGet()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<OnemanMeetingSystemManager>(
			systemType);

	public static bool TryGetSystem([NotNullWhen(true)] out OnemanMeetingSystemManager? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(systemType, out system);

	public static bool TryGetActiveSystem([NotNullWhen(true)] out OnemanMeetingSystemManager? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(systemType, out system) && system.meeting is not null;

	public static bool TryGetOnemanMeetingName([NotNullWhen(true)] out string name)
	{
		bool result =
			ExtremeSystemTypeManager.Instance.TryGet<OnemanMeetingSystemManager>(systemType, out var system) &&
			system.TryGetOnemanMeeting(out var meeting);
		name = nameof(meeting);
		return result;
	}

	public void AddQueue(byte playerId, Type meetingType)
	{
		this.meetingQueue.Enqueue((playerId, meetingType));
	}

	public void Start(PlayerControl caller, Type meetingType, PlayerControl? reporter = null)
	{
		// 変なことにならないようにとりあえず作って、値入れてからセット
		var meeting = create(meetingType);
		if (meeting is null)
		{
			throw new ArgumentException($"Invalid meeting type: {meetingType}");
		}
		meeting.VoteTarget = byte.MaxValue;

		this.Caller = caller.PlayerId;
		this.meeting = meeting;

		if (reporter == null || caller.PlayerId == reporter.PlayerId)
		{
			// チェックポイント
			OnemanMeetingCheckpoint.RpcCheckpoint(caller.PlayerId);
		}
		else
		{
			// 発動者を通報で発動
			reporter.ReportDeadBody(caller.Data);
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
		return
			this.meeting is not null &&
			this.meeting.TryGetGameEndReason(out reason);
	}

	public bool CanChatPlayer(PlayerControl target)
	{
		if (this.meeting is null)
		{
			return true;
		}
		return this.meeting.CanChatPlayer(target);
	}

	public bool IsValidShowChatPlayer(PlayerControl source)
	{
		if (this.meeting is null)
		{
			return true;
		}
		return this.meeting.IsValidShowChatPlayer(source);
	}

	public bool TryGetMeetingTitle(out string title)
	{
		if (this.meeting is null)
		{
			title = string.Empty;
			return false;
		}
		title = this.meeting.GetTitle(this.Caller);
		return true;
	}

	public void OverrideExileControllerBegin(ExileController controller)
	{
		controller.initData.confirmImpostor = true;
		controller.initData.voteTie = false;

		ExileControllerBeginePatch.SetExiledTarget(controller);

		var info = this.meeting?.CreateExiledInfo();
		if (!info.HasValue)
		{
			return;
		}

		if (controller.Player && !info.Value.IsShowPlayer)
		{
			controller.Player.gameObject.SetActive(false);
		}
		controller.completeString = info.Value.Text;
		controller.ImpostorText.text = string.Empty;

		controller.StartCoroutine(controller.Animate());
	}

	public void OverrideMeetingHudCheckForEndVoting(MeetingHud meeting)
	{
		if (this.meeting is null ||
			!tryGetCallerVote(meeting, out byte voteFor))
		{
			return;
		}

		var voteResult = new Il2CppStructArray<MeetingHud.VoterState>(
			meeting.playerStates.Length);

		var logger = ExtremeRolesPlugin.Logger;
		var builder = new StringBuilder();
		builder
			.AppendLine("---　Oneman Meeting Target Player Info ---")
			.Append(" - PlayerId:").Append(voteFor).AppendLine();
		logger.LogInfo(builder.ToString());

		var result = this.meeting.CreateVoteResult(meeting, voteFor);

		byte overridedVoteFor = result.VoteFor;

		ExtremeSystemTypeManager.RpcUpdateSystem(
			systemType, x => {
				x.Write(overridedVoteFor);
			});

		for (int i = 0; i < meeting.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = meeting.playerStates[i];
			if (playerVoteArea.TargetPlayerId == this.Caller)
			{
				playerVoteArea.VotedFor = overridedVoteFor;
			}
			else
			{
				playerVoteArea.VotedFor = 254;
			}
			meeting.SetDirtyBit(1U);

			voteResult[i] = new MeetingHud.VoterState
			{
				VoterId = playerVoteArea.TargetPlayerId,
				VotedForId = playerVoteArea.VotedFor
			};

		}
		meeting.RpcVotingComplete(
			voteResult,
			result.ExiledTarget,
			true);
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
		var ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.SetTarget:
				byte target = msgReader.ReadByte();
				if (this.meeting is null)
				{
					return;
				}
				this.meeting.VoteTarget = target;
				break;
			default:
				break;
		}
	}

	private static IOnemanMeeting? create(Type type)
		=> type switch
		{
			Type.Assassin => new AssassinAssassinateTargetMeeting(),
			_ => null,
		};

	private bool tryGetCallerVote(MeetingHud meeting, out byte voteFor)
	{
		bool result = false;
		voteFor = byte.MaxValue;

		foreach (PlayerVoteArea playerVoteArea in meeting.playerStates)
		{
			if (playerVoteArea.TargetPlayerId == this.Caller)
			{
				result = playerVoteArea.DidVote;
				voteFor = playerVoteArea.VotedFor;
				break;
			}
		}

		return result;
	}
}