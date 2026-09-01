using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.OnemanMeetingSystem;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CEOForceMeetingTests
{
	[Fact]
	public void CEOForceMeeting_Properties_And_BasicMethods()
	{
		var meeting = new CEOForceMeeting();
		Assert.True(meeting.SkipButtonActive);

		meeting.VoteTarget = 5;
		Assert.Equal((byte)5, meeting.VoteTarget);

		Assert.Equal(VoteAreaState.None, meeting.GetVoteAreaState(null!));

		bool canChat = meeting.CanChatPlayer(null!);
		Assert.False(canChat);

		bool defaultForeground = meeting.IsDefaultForegroundForDead(null!, 1);
		Assert.True(defaultForeground);

		bool validChat = meeting.IsValidShowChatPlayer(null!);
		Assert.False(validChat);

		bool hasReason = meeting.TryGetGameEndReason(out var reason);
		Assert.False(hasReason);
		Assert.Equal(RoleGameOverReason.UnKnown, reason);

		bool started = meeting.TryStartMeeting(1);
		Assert.False(started);
	}
}
