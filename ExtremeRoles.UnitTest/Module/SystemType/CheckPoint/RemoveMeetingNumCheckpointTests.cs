using System;

using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.CheckPoint;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class RemoveMeetingNumCheckpointTests : IDisposable
{
	public RemoveMeetingNumCheckpointTests()
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
	public void Constructor_ReadsTargetPlayerIdsFromMessageReader()
	{
		// Arrange
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(2);
		int callCount = 0;
		mockReader.Setup(r => r.ReadByte()).Returns(() =>
		{
			callCount++;
			return (byte)(callCount == 1 ? 5 : 10);
		});

		// Act
		var checkpoint = new RemoveMeetingNumCheckpoint(mockReader.Object);

		// Assert
		Assert.NotNull(checkpoint);
		mockReader.Verify(r => r.ReadPackedInt32(), Times.Once);
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
	}

	[Fact]
	public void HandleChecked_WhenLocalPlayerInTargetList_DecrementsRemainingEmergencies()
	{
		// Arrange
		var mockLocalPlayer = new Mock<PlayerControl>();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);
		mockLocalPlayer.SetupProperty(p => p.RemainingEmergencies, 3);

		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(1);
		mockReader.Setup(r => r.ReadByte()).Returns((byte)5);

		var checkpoint = new RemoveMeetingNumCheckpoint(mockReader.Object);

		// Act
		checkpoint.HandleChecked();

		// Assert
		Assert.Equal(2, mockLocalPlayer.Object.RemainingEmergencies);
		mockReader.Verify(r => r.ReadPackedInt32(), Times.Once);
		mockReader.Verify(r => r.ReadByte(), Times.Once);
	}

	[Fact]
	public void HandleChecked_WhenLocalPlayerNotInTargetList_DoesNotDecrementRemainingEmergencies()
	{
		// Arrange
		var mockLocalPlayer = new Mock<PlayerControl>();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockLocalPlayer.SetupProperty(p => p.RemainingEmergencies, 3);

		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(1);
		mockReader.Setup(r => r.ReadByte()).Returns((byte)5);

		var checkpoint = new RemoveMeetingNumCheckpoint(mockReader.Object);

		// Act
		checkpoint.HandleChecked();

		// Assert
		Assert.Equal(3, mockLocalPlayer.Object.RemainingEmergencies);
		mockReader.Verify(r => r.ReadPackedInt32(), Times.Once);
		mockReader.Verify(r => r.ReadByte(), Times.Once);
	}

	[Fact]
	public void HandleChecked_WhenLocalPlayerHasZeroEmergencies_DoesNotDecrement()
	{
		// Arrange
		var mockLocalPlayer = new Mock<PlayerControl>();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);
		mockLocalPlayer.SetupProperty(p => p.RemainingEmergencies, 0);

		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(false);

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.Setup(r => r.ReadPackedInt32()).Returns(1);
		mockReader.Setup(r => r.ReadByte()).Returns((byte)5);

		var checkpoint = new RemoveMeetingNumCheckpoint(mockReader.Object);

		// Act
		checkpoint.HandleChecked();

		// Assert
		Assert.Equal(0, mockLocalPlayer.Object.RemainingEmergencies);
		mockReader.Verify(r => r.ReadPackedInt32(), Times.Once);
		mockReader.Verify(r => r.ReadByte(), Times.Once);
	}
}
