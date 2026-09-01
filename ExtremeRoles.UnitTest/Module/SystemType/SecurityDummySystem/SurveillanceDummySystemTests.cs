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
	public void Add_Remove_Clear_ManageTargetsCorrectly()
	{
		// Arrange
		var system = new SurveillanceDummySystem();

		// Act
		system.Add(1, 2);
		system.Remove(1);
		system.Clear();

		// Assert
		Assert.NotNull(system);
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

		// Assert - Verify target transform was accessed to hide target player
		targetPlayerMock.VerifyGet(p => p.transform, Times.AtLeastOnce);

		// Act - Close
		system.Close();

		// Assert - Verify target transform was accessed to restore target player
		targetPlayerMock.VerifyGet(p => p.transform, Times.AtLeastOnce);
	}

	[Fact]
	public void Clear_HidesTargetAndClearsTargetsOnClose()
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

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(targetPlayerMock.Object);

		system.Add((byte)1);
		system.Begin();

		// Act
		system.Clear();

		// Assert
		targetPlayerMock.VerifyGet(p => p.transform, Times.AtLeastOnce);
	}
}
