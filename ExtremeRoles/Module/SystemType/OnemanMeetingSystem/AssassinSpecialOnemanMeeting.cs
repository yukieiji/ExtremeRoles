
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination;

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public sealed class AssassinAssassinateTargetMeeting : IOnemanMeeting
{
	public byte VoteTarget
	{
		get => this.voteTarget;
		set
		{
			this.isSuccess = ExtremeRoleManager.TryGetRole(
				value, out var targetRole) && targetRole.Id is ExtremeRoleId.Marlin;
			this.voteTarget = value;
		}
	}

	private bool isSuccess = false;
	private byte voteTarget = byte.MaxValue;

	public IOnemanMeeting.ExiledInfo CreateExiledInfo()
	{
		var player = GameData.Instance.GetPlayerById(
			this.voteTarget);
		if (player == null)
		{
			return new IOnemanMeeting.ExiledInfo(true, "UNKNOWN TARGET PLAYER!!!");
		}

		string transKey = this.isSuccess ?
			"assassinateMarinSucsess" : "assassinateMarinFail";
		string printStr = $"{player.PlayerName}{Tr.GetString(transKey)}";
		return new IOnemanMeeting.ExiledInfo(false, printStr);
	}

	public IOnemanMeeting.VoteResult CreateVoteResult(MeetingHud meeting, byte voteTarget)
	{
		if (voteTarget == 254 || voteTarget == byte.MaxValue)
		{
			ExtremeRolesPlugin.Logger.LogWarning("Assassin Meeting Target is None!! start auto targeting");

			bool targetImposter;
			do
			{
				int randomPlayerIndex = UnityEngine.Random.RandomRange(
					0, meeting.playerStates.Length);
				voteTarget = meeting.playerStates[randomPlayerIndex].TargetPlayerId;

				targetImposter = ExtremeRoleManager.TryGetRole(
					voteTarget, out var sourcePlayerRole) && sourcePlayerRole.IsImpostor();

			}
			while (targetImposter);
		}
		return new IOnemanMeeting.VoteResult(voteTarget, null);
	}

	public string GetTitle(byte _)
		=> Tr.GetString("whoIsMarine");

	public bool CanChatPlayer(PlayerControl target)
		=> ExtremeRoleManager.TryGetRole(target.PlayerId, out var role) && role.IsImpostor();

	public bool IsValidShowChatPlayer(PlayerControl chatSourcePlayer)
	{
		var localPlayerRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (!ExtremeRoleManager.TryGetRole(chatSourcePlayer.PlayerId, out var sourcePlayerRole))
		{
			return false;
		}
		return !(localPlayerRole.IsImpostor() && sourcePlayerRole.IsImpostor());
	}

	public bool TryGetGameEndReason(out RoleGameOverReason reason)
	{
		reason = RoleGameOverReason.UnKnown;
		if (this.isSuccess)
		{
			reason = RoleGameOverReason.AssassinationMarin;
			return true;
		}
		return false;
	}

	public bool TryStartMeeting(byte target)
	{
		if (!ExtremeRoleManager.TryGetSafeCastedRole<Assassin>(target, out var assasin))
		{
			return false;
		}
		var player = Helper.Player.GetPlayerControlById(target);
		if (player == null)
		{
			return false;
		}
		assasin.ExiledAction(player);
		return true;
	}
}
