using System;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.SecurityDummySystem;
using ExtremeRoles.Performance;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.SecurityDummySystem;

[Collection("UnityMock")]
public class SecurityDummySystemManagerTests : IDisposable
{
	public SecurityDummySystemManagerTests()
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

		var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}

	[Fact]
	public void Get_ReturnsInstanceFromExtremeSystemTypeManager()
	{
		// Act
		var system = SecurityDummySystemManager.Get();

		// Assert
		Assert.Same(system, SecurityDummySystemManager.Get());
	}

	[Fact]
	public void TryGet_WhenCreated_ReturnsTrueAndInstance()
	{
		// Arrange
		var created = SecurityDummySystemManager.Get();

		// Act
		bool result = SecurityDummySystemManager.TryGet(out var system);

		// Assert
		Assert.True(result);
		Assert.Same(created, system);
	}

	[Fact]
	public void Properties_DefaultValuesAndCanBeSet()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();

		// Act
		system.IsActive = true;
		system.Mode = SecurityDummySystemManager.DummyMode.No;

		// Assert
		Assert.True(system.IsActive);
		Assert.Equal(SecurityDummySystemManager.DummyMode.No, system.Mode);
	}

	[Fact]
	public void PostfixBegin_And_PostfixClose_DelegatesToMainSystem()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)0);
		var localDataMock = new Mock<NetworkedPlayerInfo>();
		localDataMock.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(localDataMock.Object);

		var targetPlayerMock = new Mock<PlayerControl>();
		targetPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);
		var targetDataMock = new Mock<NetworkedPlayerInfo>();
		targetDataMock.SetupGet(d => d.Disconnected).Returns(false);
		targetPlayerMock.SetupGet(p => p.Data).Returns(targetDataMock.Object);

		var targetTransformMock = new Mock<Transform>(IntPtr.Zero);
		targetPlayerMock.SetupGet(p => p.transform).Returns(targetTransformMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(targetPlayerMock.Object);

		system.Add(1);

		// Act - PostfixBegin
		system.PostfixBegin();

		// Assert - Player 1 scale recorded in PlayerShowSystem
		Assert.True(PlayerShowSystem.TryGetScale(1, out _));

		// Act - PostfixClose
		system.PostfixClose();

		// Assert
		Assert.True(PlayerShowSystem.TryGetScale(1, out _));
	}

	[Fact]
	public void Reset_OnPlayer_ClearsTargets_WhileMeetingStart_DoesNotClear()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)0);
		var localDataMock = new Mock<NetworkedPlayerInfo>();
		localDataMock.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(localDataMock.Object);

		var targetPlayerMock = new Mock<PlayerControl>();
		targetPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);
		var targetDataMock = new Mock<NetworkedPlayerInfo>();
		targetDataMock.SetupGet(d => d.Disconnected).Returns(false);
		targetPlayerMock.SetupGet(p => p.Data).Returns(targetDataMock.Object);

		var targetTransformMock = new Mock<Transform>(IntPtr.Zero);
		targetPlayerMock.SetupGet(p => p.transform).Returns(targetTransformMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(targetPlayerMock.Object);

		system.Add(1);

		// Act - MeetingStart Reset
		system.Reset(ResetTiming.MeetingStart);

		// Assert - System still has target 1
		system.PostfixBegin();
		Assert.True(PlayerShowSystem.TryGetScale(1, out _));

		// Act - OnPlayer Reset
		system.Reset(ResetTiming.OnPlayer);

		// Assert - Target 1 was cleared, so player 2 added later is not hidden and player 1 target list is clear
		Assert.False(PlayerShowSystem.TryGetScale(2, out _));
	}

	[Fact]
	public void UpdateSystem_OptionAdd_CallsAdd()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();
		var mockPlayer = new Mock<PlayerControl>();
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)0);
		var localDataMock = new Mock<NetworkedPlayerInfo>();
		localDataMock.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(localDataMock.Object);

		var targetPlayerMock = new Mock<PlayerControl>();
		targetPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)5);
		var targetDataMock = new Mock<NetworkedPlayerInfo>();
		targetDataMock.SetupGet(d => d.Disconnected).Returns(false);
		targetPlayerMock.SetupGet(p => p.Data).Returns(targetDataMock.Object);

		var targetTransformMock = new Mock<Transform>(IntPtr.Zero);
		targetPlayerMock.SetupGet(p => p.transform).Returns(targetTransformMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(targetPlayerMock.Object);

		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)SecurityDummySystemManager.Option.Add)
			.Returns((byte)5);

		// Act
		system.UpdateSystem(mockPlayer.Object, mockReader.Object);
		system.PostfixBegin();

		// Assert - Player 5 was added and gets processed on Begin
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
		Assert.True(PlayerShowSystem.TryGetScale(5, out _));
	}

	[Fact]
	public void UpdateSystem_OptionRemove_CallsRemove()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();
		var mockPlayer = new Mock<PlayerControl>();
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)0);
		var localDataMock = new Mock<NetworkedPlayerInfo>();
		localDataMock.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(localDataMock.Object);

		var targetPlayerMock = new Mock<PlayerControl>();
		targetPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)5);
		var targetDataMock = new Mock<NetworkedPlayerInfo>();
		targetDataMock.SetupGet(d => d.Disconnected).Returns(false);
		targetPlayerMock.SetupGet(p => p.Data).Returns(targetDataMock.Object);

		var targetTransformMock = new Mock<Transform>(IntPtr.Zero);
		targetPlayerMock.SetupGet(p => p.transform).Returns(targetTransformMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(targetPlayerMock.Object);

		system.Add(5);

		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)SecurityDummySystemManager.Option.Remove)
			.Returns((byte)5);

		// Act
		system.UpdateSystem(mockPlayer.Object, mockReader.Object);
		system.PostfixBegin();

		// Assert - Player 5 was removed and scale is NOT recorded in PlayerShowSystem
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
		Assert.False(PlayerShowSystem.TryGetScale(5, out _));
	}

	[Fact]
	public void UpdateSystem_InvalidOption_HandlesGracefully()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();
		var mockPlayer = new Mock<PlayerControl>();
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);

		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)99) // Invalid option
			.Returns((byte)5);

		// Act
		system.UpdateSystem(mockPlayer.Object, mockReader.Object);

		// Assert
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
	}
}
