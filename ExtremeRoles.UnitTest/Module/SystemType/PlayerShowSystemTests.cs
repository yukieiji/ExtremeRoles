using System;
using ExtremeRoles.Module.SystemType;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class PlayerShowSystemTests
{
	public PlayerShowSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Get_And_TryGet_ReturnsInstance()
	{
		var system = PlayerShowSystem.Get();
		Assert.NotNull(system);

		bool found = PlayerShowSystem.TryGet(out var trySys);
		Assert.True(found);
		Assert.Same(system, trySys);
	}

	[Fact]
	public void TryGetScale_WhenPlayerNotRegistered_ReturnsFalse()
	{
		bool found = PlayerShowSystem.TryGetScale(99, out float scale);
		Assert.False(found);
		Assert.Equal(float.MaxValue, scale);
	}

	[Fact]
	public void Hide_And_Show_TargetPlayer()
	{
		var system = PlayerShowSystem.Get();

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		Vector3 currentScale = new Vector3(1f, 1f, 1f);
		mockTransform.SetupProperty(t => t.localScale, currentScale);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.transform).Returns(mockTransform.Object);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		system.Hide(mockPlayer.Object);

		// Hiding already hidden player does nothing
		system.Hide(mockPlayer.Object);

		system.Show(mockPlayer.Object);
	}

	[Fact]
	public void Reset_And_UpdateSystem_DoNotThrow()
	{
		var system = new PlayerShowSystem();
		system.Reset(ResetTiming.MeetingStart, null);
		system.Reset(ResetTiming.OnPlayer, null);
		system.Reset(ResetTiming.MeetingEnd, null);
		system.UpdateSystem(null!, null!);
	}
}
