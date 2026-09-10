using System;
using System.Reflection;
using ExtremeRoles.Module.CustomMonoBehaviour;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomMonoBehaviour;

public sealed class LazyOptionSyncerTests
{
	private static void SetTimer(LazyOptionSyncer syncer, float value)
	{
		var field = typeof(LazyOptionSyncer).GetField("timer", BindingFlags.NonPublic | BindingFlags.Instance);
		field?.SetValue(syncer, value);
	}

	private static float GetTimer(LazyOptionSyncer syncer)
	{
		var field = typeof(LazyOptionSyncer).GetField("timer", BindingFlags.NonPublic | BindingFlags.Instance);
		return (float)(field?.GetValue(syncer) ?? 0f);
	}

	private static void SetupHostGameManagerAndLogicOptions(
		out Mock<AmongUsClient> mockClient,
		out Mock<GameManager> mockGameManager,
		out Mock<LogicOptions> mockLogicOptions)
	{
		MockSetupHelper.SetupUnityCommonMocks();

		mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
		mockClient.SetupGet(c => c.AmHost).Returns(true);
		var mockClientHelper = new Mock<IMockAmongUsClientget_Instance>();
		mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		IMockAmongUsClientget_Instance.Instance = mockClientHelper.Object;

		mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameOptionsFactory = new Mock<GameOptionsFactory>(IntPtr.Zero);
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockLogicOptions.SetupGet(l => l.gameOptionsFactory).Returns(mockGameOptionsFactory.Object);
		mockLogicOptions.SetupGet(l => l.currentGameOptions).Returns(mockGameOptions.Object);

		mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);
		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;
	}

	[Fact]
	public void SyncOption_WhenTimerIsZero_CallsSyncOptionAndSetsTimer()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);

		// Act
		syncer.SyncOption();

		// Assert
		var timer = GetTimer(syncer);
		Assert.Equal(1.0f, timer);
		Assert.False(syncer.Wait);
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Once);
	}

	[Fact]
	public void SyncOption_WhenTimerGreaterThanZero_SetsWaitAndResetsTimer()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);
		SetTimer(syncer, 0.5f);

		// Act
		syncer.SyncOption();

		// Assert
		var timer = GetTimer(syncer);
		Assert.Equal(1.0f, timer);
		Assert.True(syncer.Wait);
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Never);
	}

	[Fact]
	public void Update_WhenWaitIsFalse_DoesNothing()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);
		SetTimer(syncer, 0.5f);

		// Act
		syncer.Update();

		// Assert
		var timer = GetTimer(syncer);
		Assert.Equal(0.5f, timer);
		Assert.False(syncer.Wait);
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Never);
	}

	[Fact]
	public void Update_WhenWaitIsTrueAndTimerGreaterThanZero_DecrementsTimer()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);
		SetTimer(syncer, 0.5f);
		syncer.SyncOption(); // Wait becomes true, timer = 1.0f

		// Act
		syncer.Update(); // Time.deltaTime mock returns 0.1f

		// Assert
		var timer = GetTimer(syncer);
		Assert.Equal(0.9f, timer, precision: 3);
		Assert.True(syncer.Wait);
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Never);
	}

	[Fact]
	public void Update_WhenWaitIsTrueAndTimerExpires_CallsSyncOptionAndResetsWait()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);
		SetTimer(syncer, 0.05f);
		syncer.SyncOption(); // Wait becomes true, timer = 1.0f
		SetTimer(syncer, 0.05f); // timer <= 0.1f (Time.deltaTime)

		// Act
		syncer.Update();

		// Assert
		Assert.False(syncer.Wait);
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Once);
	}

	[Fact]
	public void SyncOption_WhenNotHost_SkipsSetDirty()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		mockClient.SetupGet(c => c.AmHost).Returns(false);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);

		// Act
		syncer.SyncOption();

		// Assert
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Never);
	}

	[Fact]
	public void SyncOption_WhenGameManagerNull_SkipsSetDirty()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		IMockGameManagerget_Instance.Instance = new Mock<IMockGameManagerget_Instance>().Object;
		var syncer = new LazyOptionSyncer(IntPtr.Zero);

		// Act
		syncer.SyncOption();

		// Assert
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Never);
	}

	[Fact]
	public void SyncOption_WhenLogicOptionsNull_SkipsSetDirty()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns((LogicOptions)null!);
		var syncer = new LazyOptionSyncer(IntPtr.Zero);

		// Act
		syncer.SyncOption();

		// Assert
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Never);
	}

	[Fact]
	public void SyncOption_WhenLocalPlayerNotNull_CallsRpcSyncSettings()
	{
		// Arrange
		SetupHostGameManagerAndLogicOptions(out var mockClient, out var mockGameManager, out var mockLogicOptions);
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockPlayerHelper = new Mock<IMockPlayerControlget_LocalPlayer>();
		mockPlayerHelper.Setup(h => h.Invoke()).Returns(mockPlayer.Object);
		IMockPlayerControlget_LocalPlayer.Instance = mockPlayerHelper.Object;

		var syncer = new LazyOptionSyncer(IntPtr.Zero);

		// Act
		syncer.SyncOption();

		// Assert
		mockLogicOptions.Verify(l => l.SetDirty(), Times.Once);
		mockPlayer.Verify(p => p.RpcSyncSettings(It.IsAny<byte[]>()), Times.Once);
	}
}
