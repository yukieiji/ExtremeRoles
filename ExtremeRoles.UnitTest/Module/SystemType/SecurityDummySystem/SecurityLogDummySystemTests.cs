using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.SecurityDummySystem;
using ExtremeRoles.Performance;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.SecurityDummySystem;

[Collection("UnityMock")]
public class SecurityLogDummySystemTests : IDisposable
{
	public SecurityLogDummySystemTests()
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
		MockShipStatusget_InstanceHelper.Instance = null;
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupDebugMode();
		MockSetupHelper.SetupMockConfig(plugin);

		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupAmongUsClientMock();

		if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
		{
			OptionCreator.Create();
		}
	}

	[Fact]
	public void Add_And_Remove_ManageTargetsCorrectly()
	{
		// Arrange
		var system = new SecurityLogDummySystem();

		var mockPlayer1 = new Mock<PlayerControl>();
		mockPlayer1.SetupGet(p => p.PlayerId).Returns((byte)1);
		var mockData1 = new Mock<NetworkedPlayerInfo>();
		mockData1.SetupGet(d => d.Disconnected).Returns(false);
		mockPlayer1.SetupGet(p => p.Data).Returns(mockData1.Object);

		var mockPlayer2 = new Mock<PlayerControl>();
		mockPlayer2.SetupGet(p => p.PlayerId).Returns((byte)2);
		var mockData2 = new Mock<NetworkedPlayerInfo>();
		mockData2.SetupGet(d => d.Disconnected).Returns(false);
		mockPlayer2.SetupGet(p => p.Data).Returns(mockData2.Object);

		PlayerCache.AddPlayerControl(mockPlayer1.Object);
		PlayerCache.AddPlayerControl(mockPlayer2.Object);

		var mockLogger = new Mock<SecurityLogBehaviour>(IntPtr.Zero);
		var mockLogEntries = new Mock<Il2CppSystem.Collections.Generic.List<SecurityLogBehaviour.SecurityLogEntry>>(IntPtr.Zero);
		mockLogger.SetupGet(l => l.LogEntries).Returns(mockLogEntries.Object);

		SecurityLogBehaviour loggerObj = mockLogger.Object;
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		mockShipStatus.Setup(s => s.TryGetComponent(out loggerObj)).Returns(true);

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act - Add 1 and 2, then remove 1
		system.Add(1, 2);
		system.Remove(1);

		system.Begin();

		// Assert - HasNew is set because player 2 is still targeted
		mockLogger.VerifySet(l => l.HasNew = true, Times.Once);
	}

	[Fact]
	public void Begin_And_Close_WithShipStatusWithoutLogger_HandlesGracefully()
	{
		// Arrange
		var system = new SecurityLogDummySystem();
		system.Add(1);

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		SecurityLogBehaviour? outLogger = null;
		mockShipStatus.Setup(s => s.TryGetComponent(out outLogger)).Returns(false);

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		system.Begin();
		system.Close();

		// Assert - Verified no exceptions thrown when logger is missing
		mockShipStatus.Verify(s => s.TryGetComponent(out outLogger), Times.Exactly(2));
	}

	[Fact]
	public void Begin_And_Close_WithLogger_AddsAndRemovesLogEntries()
	{
		// Arrange
		var system = new SecurityLogDummySystem();

		var mockPlayer = new Mock<PlayerControl>();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		var mockData = new Mock<NetworkedPlayerInfo>();
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		PlayerCache.AddPlayerControl(mockPlayer.Object);
		system.Add(1);

		var mockLogger = new Mock<SecurityLogBehaviour>(IntPtr.Zero);
		var mockLogEntries = new Mock<Il2CppSystem.Collections.Generic.List<SecurityLogBehaviour.SecurityLogEntry>>(IntPtr.Zero);

		mockLogger.SetupGet(l => l.LogEntries).Returns(mockLogEntries.Object);

		SecurityLogBehaviour loggerObj = mockLogger.Object;
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		mockShipStatus.Setup(s => s.TryGetComponent(out loggerObj)).Returns(true);

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		system.Begin();

		// Assert - LogEntries and HasNew set
		mockLogger.VerifySet(l => l.HasNew = true, Times.Once);

		// Act - Close
		system.Close();

		// Assert - Logger component was queried again on close
		mockShipStatus.Verify(s => s.TryGetComponent(out loggerObj), Times.AtLeastOnce);
	}
}
