using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.OnemanMeetingSystem;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class AssassinAssassinateTargetMeetingTests
{
	[Fact]
	public void BasicMethods_And_Properties()
	{
		var meeting = new AssassinAssassinateTargetMeeting();
		Assert.False(meeting.SkipButtonActive);

		meeting.VoteTarget = 2;
		Assert.Equal((byte)2, meeting.VoteTarget);

		bool hasReason = meeting.TryGetGameEndReason(out var reason);
		Assert.False(hasReason);
		Assert.Equal(RoleGameOverReason.UnKnown, reason);

		Assert.False(meeting.TryStartMeeting(1));
	}
}
