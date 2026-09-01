using System;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.CheckPoint;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GlobalCheckpointSystemTests : IDisposable
{
	public GlobalCheckpointSystemTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		PlayerCache.RemovePlayerControl(_ => true);
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
		mockIntroHelper.Setup(x => x.Invoke()).Returns((IntroCutscene)null!);
		MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;
	}

	[Fact]
	public void UpdateSystem_RoleAssignType_AddsPlayerToHandler()
	{
		// Arrange
		var system = new GlobalCheckpointSystem();
		var mockPlayer = new Mock<PlayerControl>();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadByte()).Returns((byte)GlobalCheckpointSystem.CheckpointType.RoleAssign);

		var mockPlayer2 = new Mock<PlayerControl>();
		mockPlayer2.SetupGet(p => p.PlayerId).Returns((byte)2);

		var mockReader2 = new Mock<MessageReader>(IntPtr.Zero);
		mockReader2.Setup(r => r.ReadByte()).Returns((byte)GlobalCheckpointSystem.CheckpointType.RoleAssign);

		// Act
		system.UpdateSystem(mockPlayer.Object, mockReader.Object);
		system.UpdateSystem(mockPlayer2.Object, mockReader2.Object);

		// Assert
		mockReader.Verify(r => r.ReadByte(), Times.Once);
		mockPlayer.VerifyGet(p => p.PlayerId, Times.AtLeastOnce);
		mockReader2.Verify(r => r.ReadByte(), Times.Once);
		mockPlayer2.VerifyGet(p => p.PlayerId, Times.AtLeastOnce);
	}

	[Fact]
	public void UpdateSystem_InvalidType_ThrowsArgumentException()
	{
		// Arrange
		var system = new GlobalCheckpointSystem();
		var mockPlayer = new Mock<PlayerControl>();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadByte()).Returns((byte)99);

		// Act
		Action action = () => system.UpdateSystem(mockPlayer.Object, mockReader.Object);

		// Assert
		var ex = Assert.Throws<ArgumentException>(action);
		Assert.Equal("InvalidType", ex.Message);
		mockReader.Verify(r => r.ReadByte(), Times.Once);
	}

	[Fact]
	public void UpdateSystem_WhenAllPlayersChecked_TriggersHandleCheckedAndRemovesCheckpoint()
	{
		// Arrange
		var system = new GlobalCheckpointSystem();
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);

		var mockData1 = new Mock<NetworkedPlayerInfo>();
		mockData1.SetupGet(d => d.Disconnected).Returns(false);
		var mockPlayer1 = new Mock<PlayerControl>();
		mockPlayer1.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockPlayer1.SetupGet(p => p.Data).Returns(mockData1.Object);

		var mockData2 = new Mock<NetworkedPlayerInfo>();
		mockData2.SetupGet(d => d.Disconnected).Returns(false);
		var mockPlayer2 = new Mock<PlayerControl>();
		mockPlayer2.SetupGet(p => p.PlayerId).Returns((byte)2);
		mockPlayer2.SetupGet(p => p.Data).Returns(mockData2.Object);

		PlayerCache.AddPlayerControl(mockPlayer1.Object);
		PlayerCache.AddPlayerControl(mockPlayer2.Object);

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpStart;

		var mockReader1 = new Mock<MessageReader>(IntPtr.Zero);
		mockReader1.Setup(r => r.ReadByte()).Returns((byte)GlobalCheckpointSystem.CheckpointType.RoleAssign);

		var mockReader2 = new Mock<MessageReader>(IntPtr.Zero);
		mockReader2.Setup(r => r.ReadByte()).Returns((byte)GlobalCheckpointSystem.CheckpointType.RoleAssign);

		// Act
		system.UpdateSystem(mockPlayer1.Object, mockReader1.Object);
		bool isReadyBeforeAllChecked = GameProgressSystem.Is(GameProgressSystem.Progress.RoleSetUpReady);

		system.UpdateSystem(mockPlayer2.Object, mockReader2.Object);
		bool isReadyAfterAllChecked = GameProgressSystem.Is(GameProgressSystem.Progress.RoleSetUpReady);

		// Assert
		Assert.False(isReadyBeforeAllChecked);
		Assert.True(isReadyAfterAllChecked);
		mockReader1.Verify(r => r.ReadByte(), Times.Once);
		mockReader2.Verify(r => r.ReadByte(), Times.Once);
	}

	[Fact]
	public void Reset_DoesNotThrow()
	{
		// Arrange
		var system = new GlobalCheckpointSystem();

		// Act
		system.Reset(ResetTiming.MeetingStart);

		// Assert
		Assert.NotNull(system);
	}
}
