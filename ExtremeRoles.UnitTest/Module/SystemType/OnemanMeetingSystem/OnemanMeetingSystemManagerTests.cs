using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.OnemanMeetingSystem;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class OnemanMeetingSystemManagerTests
{
	public OnemanMeetingSystemManagerTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void CreateOrGet_And_TryGetSystem_ReturnsInstance()
	{
		var manager = OnemanMeetingSystemManager.CreateOrGet();
		Assert.NotNull(manager);

		bool found = OnemanMeetingSystemManager.TryGetSystem(out var tryManager);
		Assert.True(found);
		Assert.Same(manager, tryManager);

		Assert.False(OnemanMeetingSystemManager.IsActive);
		Assert.False(OnemanMeetingSystemManager.TryGetActiveSystem(out _));
	}

	[Fact]
	public void MeetingQueue_And_TryStartMeeting()
	{
		var manager = new OnemanMeetingSystemManager();
		manager.AddQueue(1, OnemanMeetingSystemManager.Type.CEO);

		Assert.False(manager.TryStartMeeting());
	}

	[Fact]
	public void CanChatPlayer_And_IsValidShowChatPlayer_WhenMeetingNull()
	{
		var manager = new OnemanMeetingSystemManager();
		Assert.True(manager.CanChatPlayer(null!));
		Assert.True(manager.IsValidShowChatPlayer(null!));
		Assert.False(manager.IsDefaultForegroundForDead(null!));
		Assert.Equal(VoteAreaState.None, manager.GetVoteAreaState(null!));

		bool hasTitle = manager.TryGetMeetingTitle(out string title);
		Assert.False(hasTitle);
		Assert.Empty(title);

		bool hasReason = manager.TryGetGameEndReason(out var reason);
		Assert.False(hasReason);
		Assert.Equal(RoleGameOverReason.UnKnown, reason);
	}

	[Fact]
	public void UpdateSystem_SetTarget_WhenMeetingNull()
	{
		var manager = new OnemanMeetingSystemManager();
		var reader = new Mock<MessageReader>();
		reader.SetupSequence(r => r.ReadByte())
			.Returns((byte)OnemanMeetingSystemManager.Ops.SetTarget)
			.Returns((byte)5);

		manager.UpdateSystem(null!, reader.Object);
	}

	[Fact]
	public void Reset_ClearsOverride_OnMeetingEnd()
	{
		var manager = new OnemanMeetingSystemManager();
		manager.Reset(ResetTiming.MeetingEnd, null);
		Assert.False(manager.ActivateChatOverride);

		manager.Reset(ResetTiming.ExiledEnd, null);
	}
}
