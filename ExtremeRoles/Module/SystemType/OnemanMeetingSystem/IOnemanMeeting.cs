using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public interface IOnemanMeeting
{
	public readonly record struct VoteResult(byte VoteFor, NetworkedPlayerInfo? ExiledTarget);
	public readonly record struct ExiledInfo(bool IsShowPlayer, string Text);

	public byte VoteTarget { set; }

	public bool TryGetGameEndReason(out RoleGameOverReason reason);
	public bool TryStartMeeting(byte target);
	public string GetTitle(byte caller);

	public VoteResult CreateVoteResult(MeetingHud meeting, byte voteTarget);
	public ExiledInfo CreateExiledInfo();

	public bool IsValidShowChatPlayer(PlayerControl chatSourcePlayer);
	public bool IsDefaultForegroundForDead(MeetingHud hud, byte caller);
	public bool CanChatPlayer(PlayerControl target);
}
