using System;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using Hazel;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection("UnityMock")]
public sealed class OnemanMeetingSystemManagerTests : IDisposable
{
	public OnemanMeetingSystemManagerTests()
	{
		resetState();
	}

	public void Dispose()
	{
		resetState();
	}

	private static void resetState()
	{
		PlayerCache.RemovePlayerControl(_ => true);
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	private static (Mock<PlayerControl> caller, Mock<PlayerControl> reporter) createCallerAndReporter()
	{
		var mockCaller = new Mock<PlayerControl>(IntPtr.Zero);
		mockCaller.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockReporter = new Mock<PlayerControl>(IntPtr.Zero);
		mockReporter.SetupGet(p => p.PlayerId).Returns((byte)2);

		return (mockCaller, mockReporter);
	}

	[Fact]
	public void CreateOrGet_ReturnsSystemInstance()
	{
		// Act
		var system = OnemanMeetingSystemManager.CreateOrGet();

		// Assert
		Assert.NotNull(system);
		Assert.False(OnemanMeetingSystemManager.IsActive);
		Assert.False(system.IsSkipButtonActive);
	}

	[Fact]
	public void TryGetSystem_WhenCreated_ReturnsTrueAndInstance()
	{
		// Arrange
		var created = OnemanMeetingSystemManager.CreateOrGet();

		// Act
		bool result = OnemanMeetingSystemManager.TryGetSystem(out var system);

		// Assert
		Assert.True(result);
		Assert.Same(created, system);
	}

	[Fact]
	public void TryGetActiveSystem_WhenNotStarted_ReturnsFalse()
	{
		// Arrange
		OnemanMeetingSystemManager.CreateOrGet();

		// Act
		bool result = OnemanMeetingSystemManager.TryGetActiveSystem(out var system);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void TryGetOnemanMeetingName_WhenInactive_ReturnsFalseAndEmpty()
	{
		// Arrange
		OnemanMeetingSystemManager.CreateOrGet();

		// Act
		bool result = OnemanMeetingSystemManager.TryGetOnemanMeetingName(out string name);

		// Assert
		Assert.False(result);
		Assert.Equal(string.Empty, name);
	}

	[Fact]
	public void Start_WithInvalidType_ThrowsArgumentException()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var (mockCaller, mockReporter) = createCallerAndReporter();

		// Act & Assert
		Assert.Throws<ArgumentException>(() =>
		{
			system.Start(mockCaller.Object, (OnemanMeetingSystemManager.Type)999, mockReporter.Object);
		});
	}

	[Fact]
	public void Start_WithCEOMeeting_SetsActiveAndProperties()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var (mockCaller, mockReporter) = createCallerAndReporter();

		// Act
		system.Start(mockCaller.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		// Assert
		Assert.True(OnemanMeetingSystemManager.IsActive);
		Assert.Equal(1, system.Caller);
		Assert.True(system.ActivateChatOverride);
		Assert.True(system.IsSkipButtonActive);
		Assert.True(system.IsActiveMeeting<CEOForceMeeting>());
		Assert.True(system.TryGetOnemanMeeting<CEOForceMeeting>(out var meeting));
		Assert.NotNull(meeting);
		Assert.True(OnemanMeetingSystemManager.TryGetOnemanMeetingName(out string name));
		Assert.Equal(nameof(CEOForceMeeting), name);
		mockReporter.Verify(r => r.ReportDeadBody(It.IsAny<NetworkedPlayerInfo>()), Times.Once);
	}

	[Fact]
	public void Start_WhenAlreadyStarted_DoesNotOverride()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var (mockCaller1, mockReporter) = createCallerAndReporter();
		var mockCaller2 = new Mock<PlayerControl>(IntPtr.Zero);
		mockCaller2.SetupGet(p => p.PlayerId).Returns((byte)3);

		system.Start(mockCaller1.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		// Act
		system.Start(mockCaller2.Object, OnemanMeetingSystemManager.Type.Assassin, mockReporter.Object);

		// Assert
		Assert.Equal(1, system.Caller);
		Assert.True(system.IsActiveMeeting<CEOForceMeeting>());
	}

	[Fact]
	public void Methods_WhenMeetingIsNull_ReturnDefaultValues()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act & Assert
		Assert.False(system.IsDefaultForegroundForDead(null!));
		Assert.Equal(VoteAreaState.None, system.GetVoteAreaState(null!));
		Assert.True(system.CanChatPlayer(mockPlayer.Object));
		Assert.True(system.IsValidShowChatPlayer(mockPlayer.Object));
		Assert.False(system.TryGetMeetingTitle(out string title));
		Assert.Equal(string.Empty, title);
		Assert.False(system.TryGetGameEndReason(out var reason));
		Assert.Equal(RoleGameOverReason.UnKnown, reason);
	}

	[Fact]
	public void TryStartMeeting_WhenQueueIsEmpty_ReturnsFalse()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();

		// Act
		bool result = system.TryStartMeeting();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void AddQueue_And_TryStartMeeting_WhenStartFails_ReturnsFalse()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		system.AddQueue(5, OnemanMeetingSystemManager.Type.CEO);

		// Act - CEOForceMeeting.TryStartMeeting(5) will fail because player 5 is not set up
		bool result = system.TryStartMeeting();

		// Assert
		Assert.False(result);
		Assert.False(OnemanMeetingSystemManager.IsActive);
	}

	[Fact]
	public void Reset_MeetingEndTiming_DisablesActivateChatOverride()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var (mockCaller, mockReporter) = createCallerAndReporter();
		system.Start(mockCaller.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		// Act
		system.Reset(ResetTiming.MeetingEnd);

		// Assert
		Assert.False(system.ActivateChatOverride);
		// starting is true, so meeting shouldn't be cleared yet on MeetingEnd
		Assert.True(OnemanMeetingSystemManager.IsActive);
	}

	[Fact]
	public void GetVoteAreaState_WhenMeetingActive_ResetsStartingAndDelegatesToMeeting()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var (mockCaller, mockReporter) = createCallerAndReporter();
		system.Start(mockCaller.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		// Act - Calling GetVoteAreaState resets `starting` field to false
		var state = system.GetVoteAreaState(null!);

		// Assert
		Assert.Equal(VoteAreaState.None, state);

		// Now Reset with ExiledEnd timing should clear the meeting since starting is false
		system.Reset(ResetTiming.ExiledEnd);
		Assert.False(OnemanMeetingSystemManager.IsActive);
		Assert.Equal(byte.MaxValue, system.Caller);
	}

	[Fact]
	public void UpdateSystem_SetTarget_UpdatesVoteTargetOnActiveMeeting()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var (mockCaller, mockReporter) = createCallerAndReporter();
		system.Start(mockCaller.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		var mockWriter = new Mock<MessageReader>(IntPtr.Zero);
		// Ops.SetTarget = 0, Target = 3
		int readIndex = 0;
		byte[] bytes = new byte[] { (byte)OnemanMeetingSystemManager.Ops.SetTarget, 3 };
		mockWriter.Setup(r => r.ReadByte()).Returns(() => bytes[readIndex++]);

		// Act
		system.UpdateSystem(mockCaller.Object, mockWriter.Object);

		// Assert
		system.TryGetOnemanMeeting<CEOForceMeeting>(out var ceoMeeting);
		Assert.NotNull(ceoMeeting);
		Assert.Equal(3, ceoMeeting.VoteTarget);
	}

	[Fact]
	public void UpdateSystem_WithUnknownOps_DoesNotThrow()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();
		var (mockCaller, mockReporter) = createCallerAndReporter();
		system.Start(mockCaller.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		var mockWriter = new Mock<MessageReader>(IntPtr.Zero);
		mockWriter.Setup(r => r.ReadByte()).Returns((byte)255);

		// Act & Assert
		system.UpdateSystem(mockCaller.Object, mockWriter.Object);
	}

	[Fact]
	public void OverrideMeetingHudCheckForEndVoting_WhenMeetingIsNull_ReturnsEarly()
	{
		// Arrange
		var system = OnemanMeetingSystemManager.CreateOrGet();

		// Act & Assert (Should not throw NRE)
		system.OverrideMeetingHudCheckForEndVoting(null!);
	}
}
