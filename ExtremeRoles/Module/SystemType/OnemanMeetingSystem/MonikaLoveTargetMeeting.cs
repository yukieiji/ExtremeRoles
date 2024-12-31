using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;

using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.GameResult.StatusOverrider;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public sealed class MonikaLoveTargetMeeting : IOnemanMeeting, IMeetingButtonInitialize
{
	public byte VoteTarget
	{
		get => this.winPlayer == null ? byte.MaxValue : this.winPlayer.PlayerId;
		set
		{
			if (value == byte.MaxValue)
			{
				return;
			}
			var voteTarget = this.target.GetVoteTarget(value);
			var notSelectPlayer = this.target.GetAnother(value);

			ExtremeRolesPlugin.ShipState.AddWinner(voteTarget);
			FinalSummaryBuilder.AddStatusOverride(
				(GameOverReason)RoleGameOverReason.MonikaThisGameIsMine,
				new MonikaMeetingResultStatusOverrider(
					voteTarget, notSelectPlayer));
			this.winPlayer = voteTarget;
			this.notSelectPlayer = notSelectPlayer;
			ExtremeRolesPlugin.Logger.LogInfo(
				$"Winner:{this.winPlayer.PlayerName} NoWinner:{this.notSelectPlayer.PlayerName}");
		}
	}

	private sealed class MeetingTarget
	{
		private NetworkedPlayerInfo? right;
		private NetworkedPlayerInfo? left;

		public NetworkedPlayerInfo GetVoteTarget(byte playerId)
		{
			NetworkedPlayerInfo? result;
			if (isReturnThis(playerId, this.left, out result) ||
				isReturnThis(playerId, this.right, out result))
			{
				return result;
			}
			throw new InvalidOperationException("Invalid meeting targets");
		}

		public NetworkedPlayerInfo GetAnother(byte playerId)
		{
			NetworkedPlayerInfo? result;
			if (isReturnThis(playerId, this.left, out result, this.right) ||
				isReturnThis(playerId, this.right, out result, this.left))
			{
				return result;
			}
			if (this.left == null || this.right == null)
			{
				throw new InvalidOperationException("Invalid meeting targets");
			}
			int index = RandomGenerator.Instance.Next(2);
			if (index == 0)
			{
				return this.left;
			}
			else
			{
				return this.right;
			}
		}
		
		public void Add(NetworkedPlayerInfo playerInfo)
		{
			if (tryAssign(ref this.left, playerInfo) ||
				tryAssign(ref this.right,  playerInfo))
			{
				return;
			}
			throw new InvalidOperationException("Meeting target always TWO!!!!");
		}

		public bool Contain(byte playerId)
			=> (
				(this.left != null && this.left.PlayerId == playerId) ||
				(this.right != null && this.right.PlayerId == playerId)
			);

		private bool tryAssign(ref NetworkedPlayerInfo? info,  NetworkedPlayerInfo playerInfo)
		{
			if (info == null)
			{
				info = playerInfo;
				return true;
			}
			return false;
		}

		private bool isReturnThis(
			byte playerId, NetworkedPlayerInfo? check,
			[NotNullWhen(true)] out NetworkedPlayerInfo? info,
			NetworkedPlayerInfo? anothor = null)
		{
			info = anothor == null ? check : anothor;
			return info != null && info.PlayerId == playerId;
		}
	}

	private sealed record MeetingTargetInfo(byte PlayerId, NetworkedPlayerInfo PlayerInfo);

	private NetworkedPlayerInfo? winPlayer;
	private NetworkedPlayerInfo? notSelectPlayer;

	private readonly MonikaTrashSystem system;
	private readonly MeetingTarget target;
	// 会議の投票先は常に2つ

	public MonikaLoveTargetMeeting()
	{
		if (!MonikaTrashSystem.TryGet(out var system))
		{
			throw new InvalidOperationException("Monika system can't found");
		}
		this.system = system;
		this.target = new MeetingTarget();
	}

	public IOnemanMeeting.ExiledInfo CreateExiledInfo()
	{
		if (this.winPlayer == null || this.notSelectPlayer == null)
		{
			return new IOnemanMeeting.ExiledInfo(true, "UNKNOWN MEETING PLAYER!!!");
		}

		string printStr = $"「〇〇」は「{this.winPlayer.PlayerName}」が好きだった\n(「{this.notSelectPlayer.PlayerName}」が除外された)";
		return new IOnemanMeeting.ExiledInfo(true, printStr);
	}

	public IOnemanMeeting.VoteResult CreateVoteResult(MeetingHud meeting, byte voteTarget)
	{
		var playerInfo = this.target.GetAnother(voteTarget);
		return new IOnemanMeeting.VoteResult(voteTarget, playerInfo);
	}

	public string GetTitle(byte caller)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return "Invalid Meeting";
		}
		byte localPlayerId = localPlayer.PlayerId;
		if (localPlayerId == caller)
		{
			return "好きだった人を選んでください";
		}
		else if (this.target.Contain(localPlayerId))
		{
			return "あなた達は私に選ばれるべき器なのかしら？";
		}
		else
		{
			return "外野は静かに運命を見守りなさい";
		}
	}

	public VoteAreaState GetVoteAreaState(NetworkedPlayerInfo player)
		=> VoteAreaState.None;

	public bool CanChatPlayer(PlayerControl target)
		=>
		!(
			target.Data == null ||
			target.Data.IsDead ||
			target.Data.Disconnected ||
			this.system.InvalidPlayer(target)
		);

	public bool IsDefaultForegroundForDead(MeetingHud _, byte caller)
		=> PlayerControl.LocalPlayer.PlayerId != caller;

	public bool IsValidShowChatPlayer(PlayerControl _)
		=> true;

	public bool TryGetGameEndReason(out RoleGameOverReason reason)
	{
		reason = RoleGameOverReason.UnKnown;
		if (this.winPlayer != null)
		{
			reason = RoleGameOverReason.MonikaThisGameIsMine;
			return true;
		}
		return false;
	}

	public bool TryStartMeeting(byte target)
		=> false;

	public void InitializeButon(Vector3 origin, Vector2 offset, PlayerVoteArea[] pvas)
	{
		int index = 0;

		foreach (var pva in pvas)
		{
			var player = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
			bool meetingTargetPlayer = !(
				player == null ||
				player.IsDead ||
				player.Disconnected ||
				this.system.InvalidPlayer(player) ||
				(
					ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
					role.Id is ExtremeRoleId.Monika
				) // モニカは投票権を持つが会議の投票先には表示さない
			);

			if (meetingTargetPlayer)
			{
				this.target.Add(player!);
				pva.transform.position = new Vector3(
					offset.x * (index == 0 ? 0.75f : -0.75f), 0.0f,
					origin.z - 0.9f);
				index += 1;
			}
			else
			{
				// 無茶苦茶遠くにおいておく
				pva.transform.position = new Vector3(1000.0f, 1000.0f, 1000.0f);
			}
		}
	}
}
