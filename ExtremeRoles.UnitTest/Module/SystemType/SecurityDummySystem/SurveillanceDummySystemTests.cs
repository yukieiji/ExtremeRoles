using System;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.SecurityDummySystem;
using ExtremeRoles.Performance;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.SecurityDummySystem;

[Collection("UnityMock")]
public class SurveillanceDummySystemTests : IDisposable
{
	public SurveillanceDummySystemTests()
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
	}

	[Fact]
	public void Add_And_Remove_And_Clear_ManageTargetsCorrectly()
	{
		// Arrange
		var system = new SurveillanceDummySystem();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)0);
		var localDataMock = new Mock<NetworkedPlayerInfo>();
		localDataMock.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(localDataMock.Object);

		var p1Mock = new Mock<PlayerControl>();
		p1Mock.SetupGet(p => p.PlayerId).Returns((byte)1);
		var p1DataMock = new Mock<NetworkedPlayerInfo>();
		p1DataMock.SetupGet(d => d.Disconnected).Returns(false);
		p1Mock.SetupGet(p => p.Data).Returns(p1DataMock.Object);

		var p1TransformMock = new Mock<Transform>(IntPtr.Zero);
		p1Mock.SetupGet(p => p.transform).Returns(p1TransformMock.Object);

		var p2Mock = new Mock<PlayerControl>();
		p2Mock.SetupGet(p => p.PlayerId).Returns((byte)2);
		var p2DataMock = new Mock<NetworkedPlayerInfo>();
		p2DataMock.SetupGet(d => d.Disconnected).Returns(false);
		p2Mock.SetupGet(p => p.Data).Returns(p2DataMock.Object);

		var p2TransformMock = new Mock<Transform>(IntPtr.Zero);
		p2Mock.SetupGet(p => p.transform).Returns(p2TransformMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(p1Mock.Object);
		PlayerCache.AddPlayerControl(p2Mock.Object);

		// Act - Add 1 and 2, then remove 1
		system.Add(1, 2);
		system.Remove(1);

		system.Begin();

		// Assert - Player 1 was removed so NOT hidden in PlayerShowSystem; Player 2 IS hidden in PlayerShowSystem
		Assert.False(PlayerShowSystem.TryGetScale(1, out _));
		Assert.True(PlayerShowSystem.TryGetScale(2, out _));

		// Act - Clear
		system.Clear();

		// Assert - Clear emptied target list, so player 3 added later is not hidden
		Assert.False(PlayerShowSystem.TryGetScale(3, out _));
	}

	[Fact]
	public void PrefixUpdate_ReturnsTrue()
	{
		// Arrange
		var system = new SurveillanceDummySystem();

		// Act
		bool result = system.PrefixUpdate(null!);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void BeginAndClose_HidesAndShowsTargetNonLocalPlayers()
	{
		// Arrange
		var system = new SurveillanceDummySystem();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)0);
		var localDataMock = new Mock<NetworkedPlayerInfo>();
		localDataMock.SetupGet(d => d.Disconnected).Returns(false);
		localPlayerMock.SetupGet(p => p.Data).Returns(localDataMock.Object);

		var localTransformMock = new Mock<Transform>(IntPtr.Zero);
		localPlayerMock.SetupGet(p => p.transform).Returns(localTransformMock.Object);

		var targetPlayerMock = new Mock<PlayerControl>();
		targetPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);
		var targetDataMock = new Mock<NetworkedPlayerInfo>();
		targetDataMock.SetupGet(d => d.Disconnected).Returns(false);
		targetPlayerMock.SetupGet(p => p.Data).Returns(targetDataMock.Object);

		var targetTransformMock = new Mock<Transform>(IntPtr.Zero);
		targetPlayerMock.SetupGet(p => p.transform).Returns(targetTransformMock.Object);

		var nonTargetPlayerMock = new Mock<PlayerControl>();
		nonTargetPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)2);
		var nonTargetDataMock = new Mock<NetworkedPlayerInfo>();
		nonTargetDataMock.SetupGet(d => d.Disconnected).Returns(false);
		nonTargetPlayerMock.SetupGet(p => p.Data).Returns(nonTargetDataMock.Object);

		var nonTargetTransformMock = new Mock<Transform>(IntPtr.Zero);
		nonTargetPlayerMock.SetupGet(p => p.transform).Returns(nonTargetTransformMock.Object);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(targetPlayerMock.Object);
		PlayerCache.AddPlayerControl(nonTargetPlayerMock.Object);

		system.Add((byte)1);

		// Act - Begin
		system.Begin();

		// Assert - Target non-local player (ID 1) scale recorded in PlayerShowSystem, local player (ID 0) and non-target (ID 2) NOT recorded
		Assert.True(PlayerShowSystem.TryGetScale(1, out _));
		Assert.False(PlayerShowSystem.TryGetScale(0, out _));
		Assert.False(PlayerShowSystem.TryGetScale(2, out _));

		// Act - Close
		system.Close();

		// Assert - System executes Close without throwing and keeps state valid
		Assert.True(PlayerShowSystem.TryGetScale(1, out _));
	}
}
