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

public sealed class MonikaLoveTargetMeeting : IOnemanMeeting, IVoterShiftor, IVoterValidtor
{
	public bool SkipButtonActive => false;

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
			var notSelectPlayer = this.target.GetAnother(value).ExiledTarget!;

			ExtremeRolesPlugin.ShipState.AddWinner(voteTarget);
			PlayerSummaryBuilder.AddStatusOverride(
				(GameOverReason)RoleGameOverReason.MonikaThisGameIsMine,
				new MonikaMeetingResultStatusOverrider(
					voteTarget, notSelectPlayer));
			this.winPlayer = voteTarget;
			this.notSelectPlayer = notSelectPlayer;
			ExtremeRolesPlugin.Logger.LogInfo(
				$"Winner:{this.winPlayer.PlayerName} NoWinner:{this.notSelectPlayer.PlayerName}");
		}
	}

	public IEnumerable<byte> ValidPlayer
	{
		get
		{
			foreach (var player in GameData.Instance.AllPlayers)
			{

				if (!(
					player == null ||
					player.IsDead ||
					player.Disconnected ||
					this.system.InvalidPlayer(player) ||
					(
						ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
						role.Core.Id is ExtremeRoleId.Monika
					) // モニカは投票権を持つが会議の投票先には表示さない)
				))
				{
					// このメソッドは開始時一回しか呼ばれないことが保証されているため
					this.target.Add(player);
					yield return player.PlayerId;
				}
			}
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

		public IOnemanMeeting.VoteResult GetAnother(byte playerId)
		{
			NetworkedPlayerInfo? result;
			if (isReturnThis(playerId, this.left, out result, this.right) ||
				isReturnThis(playerId, this.right, out result, this.left))
			{
				return new(playerId, result);
			}
			if (this.left == null || this.right == null) //　NULLになることはありえないが一応のチェック
			{
				throw new InvalidOperationException("Invalid meeting targets");
			}
			int index = RandomGenerator.Instance.Next(2);
			if (index == 0)
			{
				return new(this.right.PlayerId, this.left);
			}
			else
			{
				return new(this.left.PlayerId, this.right);
			}
		}

		public void Add(NetworkedPlayerInfo playerInfo)
		{
			if (tryAssign(ref this.left, playerInfo) ||
				tryAssign(ref this.right,  playerInfo))
			{
				return;
			}
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
			[NotNullWhen(true)] out NetworkedPlayerInfo? result,
			NetworkedPlayerInfo? anothor = null)
		{
			result = anothor == null ? check : anothor;
			return result != null && check != null && check.PlayerId == playerId;
		}
	}

	private sealed record MeetingTargetInfo(byte PlayerId, NetworkedPlayerInfo PlayerInfo);

	private NetworkedPlayerInfo? winPlayer;
	private NetworkedPlayerInfo? notSelectPlayer;

	private readonly MonikaTrashSystem system;
	private readonly MeetingTarget target;
	// 会議の投票先は常に2つ
	private const float xOffset = 0.75f;

	public MonikaLoveTargetMeeting()
	{
		if (!MonikaTrashSystem.TryGet(out var system))
		{
			throw new InvalidOperationException("Monika system can't found");
		}
		this.system = system;
		this.target = new MeetingTarget();
	}

	public IOnemanMeeting.ExiledInfo CreateExiledInfo(byte caller)
	{
		var monikaPlayer = GameData.Instance.GetPlayerById(caller);

		if (monikaPlayer == null ||
			this.winPlayer == null ||
			this.notSelectPlayer == null)
		{
			return new IOnemanMeeting.ExiledInfo(true, "UNKNOWN MEETING PLAYER!!!");
		}

		string printStr = Tr.GetString(
			"MonikaMeetingExiled",
			monikaPlayer.PlayerName,
			this.winPlayer.PlayerName,
			this.notSelectPlayer.PlayerName);
		// ここで勝利したWinGameControllIdを入れる => ニュートラルなのでコントロールIdがいる
		if (ExtremeRoleManager.TryGetRole(caller, out var role))
		{
			ExtremeRolesPlugin.ShipState.SetWinControlId(role.GameControlId);
		}
		return new IOnemanMeeting.ExiledInfo(true, printStr);
	}

	public IOnemanMeeting.VoteResult CreateVoteResult(MeetingHud meeting, byte voteTarget)
		=> this.target.GetAnother(voteTarget);

	public string GetTitle(byte caller)
		=> Tr.GetString(getTitleKey(caller));

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
					role.Core.Id is ExtremeRoleId.Monika
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
	private string getTitleKey(byte caller)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return "InvalidMeeting";
		}
		byte localPlayerId = localPlayer.PlayerId;
		if (localPlayerId == caller)
		{
			return "MonikaMeetingSelectLover";
		}
		else if (this.target.Contain(localPlayerId))
		{
			return "MonikaMeetingSelectTarget";
		}
		else
		{
			return "MonikaMeetingOther";
		}
	}

	public void Shift(Vector3 origin, Vector2 offset, PlayerVoteArea[] pvas)
	{
		int index = 0;
		foreach (var pva in pvas)
		{
			pva.transform.position = new Vector3(
				offset.x * (index == 0 ? xOffset : -xOffset), 0.0f,
				origin.z - 0.9f);
			index++;
		}
	}
}
