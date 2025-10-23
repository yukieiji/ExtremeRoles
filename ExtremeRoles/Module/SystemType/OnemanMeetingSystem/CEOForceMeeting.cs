using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using System.Collections.Generic;

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public sealed class CEOForceMeeting : IOnemanMeeting, IVoterValidtor
{
	public bool SkipButtonActive => true;

	public byte VoteTarget { get; set; }

	public IEnumerable<byte> ValidPlayer
	{
		get
		{
			foreach (var player in GameData.Instance.AllPlayers)
			{
				// CEOと死んだ人は非表示にする
				if (!(
					player == null ||
					player.IsDead ||
					player.Disconnected
				))
				{
					yield return player.PlayerId;
				}
			}
		}
	}

	public IOnemanMeeting.ExiledInfo CreateExiledInfo(byte _)
	{
		var player = GameData.Instance.GetPlayerById(this.VoteTarget);
		if (player == null)
		{
			return new IOnemanMeeting.ExiledInfo(false, "CEOはスキップを選択した");
		}
		else
		{
			return new IOnemanMeeting.ExiledInfo(true, $"CEOの権限により「{player.PlayerName}」が追放された");
		}
	}

	public IOnemanMeeting.VoteResult CreateVoteResult(MeetingHud meeting, byte voteTarget)
	{
		var target = GameData.Instance.GetPlayerById(voteTarget);

		return new IOnemanMeeting.VoteResult(voteTarget, target);
	}

	public string GetTitle(byte _)
		=> "CEO会議";

	public VoteAreaState GetVoteAreaState(NetworkedPlayerInfo player)
		=> VoteAreaState.None;

	public bool CanChatPlayer(PlayerControl target)
		=> target != null && target.Data != null && !target.Data.IsDead && !target.Data.Disconnected;

	public bool IsDefaultForegroundForDead(MeetingHud _, byte caller)
		=> PlayerControl.LocalPlayer.PlayerId != caller;

	public bool IsValidShowChatPlayer(PlayerControl chatSourcePlayer)
		=> chatSourcePlayer != null && chatSourcePlayer.Data != null && !chatSourcePlayer.Data.IsDead && !chatSourcePlayer.Data.Disconnected;

	public bool TryGetGameEndReason(out RoleGameOverReason reason)
	{
		reason = RoleGameOverReason.UnKnown;
		return false;
	}

	public bool TryStartMeeting(byte target)
	{
		if (!ExtremeRoleManager.TryGetSafeCastedRole<CEO>(target, out var ceo))
		{
			return false;
		}
		var player = Helper.Player.GetPlayerControlById(target);
		if (player == null)
		{
			return false;
		}
		ceo.ExiledAction(player);
		return true;
	}
}
