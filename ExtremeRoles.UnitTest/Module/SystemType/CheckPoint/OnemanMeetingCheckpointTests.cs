using System;

using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Performance;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.CheckPoint;

[Collection("UnityMock")]
public class OnemanMeetingCheckpointTests : IDisposable
{
	public OnemanMeetingCheckpointTests()
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
	}

	[Fact]
	public void Constructor_ReadsRolePlayerIdFromMessageReader()
	{
		// Arrange
		var mockPlayer = new Mock<PlayerControl>();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)3);
		PlayerCache.AddPlayerControl(mockPlayer.Object);

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadByte()).Returns((byte)3);

		// Act
		var checkpoint = new OnemanMeetingCheckpoint(mockReader.Object);

		// Assert
		Assert.NotNull(checkpoint);
		mockReader.Verify(r => r.ReadByte(), Times.Once);
	}

	[Fact]
	public void HandleChecked_WhenRolePlayerNotFound_ReturnsWithoutThrowing()
	{
		// Arrange
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadByte()).Returns((byte)99); // player 99 is not in PlayerCache

		var checkpoint = new OnemanMeetingCheckpoint(mockReader.Object);

		// Act
		checkpoint.HandleChecked();

		// Assert
		mockReader.Verify(r => r.ReadByte(), Times.Once);
	}
}
