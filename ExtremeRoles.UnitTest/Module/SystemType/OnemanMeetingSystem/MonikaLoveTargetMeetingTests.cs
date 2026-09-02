using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.OnemanMeetingSystem;

[Collection("UnityMock")]
public class MonikaLoveTargetMeetingTests
{
	public MonikaLoveTargetMeetingTests()
	{
		PlayerCache.RemovePlayerControl(_ => true);
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Constructor_WhenMonikaSystemRegistered_Succeeds()
	{
		var monikaSystem = new MonikaTrashSystem(true);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.MonikaTrashSystem, monikaSystem);

		var meeting = new MonikaLoveTargetMeeting();
		Assert.NotNull(meeting);
		Assert.False(meeting.SkipButtonActive);
		Assert.Equal((byte)255, meeting.VoteTarget);

		bool hasReason = meeting.TryGetGameEndReason(out var reason);
		Assert.False(hasReason);

		Assert.Equal(VoteAreaState.None, meeting.GetVoteAreaState(null!));
		Assert.True(meeting.IsValidShowChatPlayer(null!));
		Assert.False(meeting.TryStartMeeting(1));
	}
}
