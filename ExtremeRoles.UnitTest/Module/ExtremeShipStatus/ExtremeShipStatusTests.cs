using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.ExtremeShipStatusTests;

public sealed class ExtremeShipStatusTests : SerialTestBase, IClassFixture<GameOptionsManagerMock>
{
	private sealed class DummySingleRole : SingleRoleBase
	{
		public DummySingleRole(ExtremeRoleId roleId)
		{
			var core = new RoleCore(roleId, ExtremeRoleType.Crewmate, default, "Dummy");
			var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(this, core);
		}

		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
		protected override void RoleSpecificInit() { }

		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override UnityEngine.Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => default;
	}

	public ExtremeShipStatusTests(SerialFixture fixture, GameOptionsManagerMock gameOptionsManagerMock)
        : base(fixture, gameOptionsManagerMock, new LoggerMock(), new ShipStatusMock(), new AmongUsClientMock())
    {
		SetupAmongUsClientAndShipState();
	}

	private static void SetupAmongUsClientAndShipState()
	{
		MockSetupHelper.SetupMockExtremeRolePlugin();

		var mockClient = new Mock<AmongUsClient>();
		var mockHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
		mockHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
		MockAmongUsClientget_InstanceHelper.Instance = mockHelper.Object;

		var mockWriter = new Mock<Hazel.MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(
			It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<Hazel.SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockLocalPlayer = new Mock<PlayerControl>();
		mockLocalPlayer.SetupGet(p => p.NetId).Returns(1u);
		var mockPlayerHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		MockPlayerControlget_LocalPlayerHelper.Instance = mockPlayerHelper.Object;
		mockPlayerHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
	}

	[Fact]
	public void Initialize_ResetsAllState()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		status.SetWinControlId(42);
		status.SetDisableWinCheck(true);
		var player = new Mock<PlayerControl>().Object;
		status.AddWinner(player);

		// Act
		status.Initialize();

		// Assert
		Assert.Equal(int.MaxValue, status.WinGameControlId);
		Assert.False(status.IsDisableWinCheck);
		Assert.Empty(status.GetPlusWinner());
		Assert.Empty(status.DeadPlayerInfo);
		Assert.False(status.IsAssassinAssign);
	}

	[Fact]
	public void AddDeadInfo_AddsDeadInfoWhenNotExists()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		var deadPlayer = new Mock<PlayerControl>();
		deadPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var killer = new Mock<PlayerControl>();
		killer.SetupGet(p => p.PlayerId).Returns((byte)2);

		// Act
		status.AddDeadInfo(deadPlayer.Object, DeathReason.Exile, killer.Object);

		// Assert
		Assert.True(status.DeadPlayerInfo.ContainsKey(1));
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Exiled, status.DeadPlayerInfo[1].Reason);
		Assert.Equal(killer.Object, status.DeadPlayerInfo[1].Killer);
	}

	[Fact]
	public void AddDeadInfo_MultiplePlayers_AddsAndRemovesCorrectly()
	{
		// Arrange
		var status = new ExtremeShipStatus();

		var p1 = new Mock<PlayerControl>();
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);
		var p2 = new Mock<PlayerControl>();
		p2.SetupGet(p => p.PlayerId).Returns((byte)2);
		var p3 = new Mock<PlayerControl>();
		p3.SetupGet(p => p.PlayerId).Returns((byte)3);

		var killer = new Mock<PlayerControl>();
		killer.SetupGet(p => p.PlayerId).Returns((byte)99);

		// Act - Add multiple players with different death reasons
		status.AddDeadInfo(p1.Object, DeathReason.Kill, killer.Object);
		status.AddDeadInfo(p2.Object, DeathReason.Exile, killer.Object);
		status.AddDeadInfo(p3.Object, DeathReason.Disconnect, killer.Object);

		// Assert - Check all dead infos are recorded independently
		Assert.Equal(3, status.DeadPlayerInfo.Count);
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Killed, status.DeadPlayerInfo[1].Reason);
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Exiled, status.DeadPlayerInfo[2].Reason);
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Disconnected, status.DeadPlayerInfo[3].Reason);
		Assert.Equal(killer.Object, status.DeadPlayerInfo[1].Killer);
		Assert.Equal(killer.Object, status.DeadPlayerInfo[2].Killer);
		Assert.Equal(killer.Object, status.DeadPlayerInfo[3].Killer);

		// Act - Remove one player
		status.RemoveDeadInfo(2);

		// Assert - Verify specified player is removed while others remain intact
		Assert.Equal(2, status.DeadPlayerInfo.Count);
		Assert.True(status.DeadPlayerInfo.ContainsKey(1));
		Assert.False(status.DeadPlayerInfo.ContainsKey(2));
		Assert.True(status.DeadPlayerInfo.ContainsKey(3));

		// Act - Remove remaining players
		status.RemoveDeadInfo(1);
		status.RemoveDeadInfo(3);

		// Assert - Dictionary is empty
		Assert.Empty(status.DeadPlayerInfo);
	}

	[Fact]
	public void AddDeadInfo_IgnoresWhenAlreadyExists()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		var deadPlayer = new Mock<PlayerControl>();
		deadPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		var killer = new Mock<PlayerControl>();
		killer.SetupGet(p => p.PlayerId).Returns((byte)2);

		status.AddDeadInfo(deadPlayer.Object, DeathReason.Exile, killer.Object);

		// Act - Try adding again with different reason
		status.AddDeadInfo(deadPlayer.Object, DeathReason.Kill, killer.Object);

		// Assert - Remains Exiled
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Exiled, status.DeadPlayerInfo[1].Reason);
	}

	[Theory]
	[InlineData(DeathReason.Exile, 2, ExtremeShipStatus.PlayerStatus.Exiled)]
	[InlineData(DeathReason.Disconnect, 2, ExtremeShipStatus.PlayerStatus.Disconnected)]
	[InlineData(DeathReason.Kill, 2, ExtremeShipStatus.PlayerStatus.Killed)]
	[InlineData(DeathReason.Kill, 1, ExtremeShipStatus.PlayerStatus.Suicide)]
	[InlineData((DeathReason)999, 2, ExtremeShipStatus.PlayerStatus.Dead)]
	public void AddDeadInfo_MapsDeathReasonsCorrectly(DeathReason deathReason, byte killerId, ExtremeShipStatus.PlayerStatus expectedStatus)
	{
		// Arrange
		var status = new ExtremeShipStatus();
		var deadPlayer = new Mock<PlayerControl>();
		deadPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var killer = new Mock<PlayerControl>();
		killer.SetupGet(p => p.PlayerId).Returns(killerId);

		// Act
		status.AddDeadInfo(deadPlayer.Object, deathReason, killer.Object);

		// Assert
		Assert.Equal(expectedStatus, status.DeadPlayerInfo[1].Reason);
	}

	[Fact]
	public void RemoveDeadInfo_RemovesDeadInfo()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		var deadPlayer = new Mock<PlayerControl>();
		deadPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		var killer = new Mock<PlayerControl>();
		killer.SetupGet(p => p.PlayerId).Returns((byte)2);

		status.AddDeadInfo(deadPlayer.Object, DeathReason.Kill, killer.Object);
		Assert.Single(status.DeadPlayerInfo);

		// Act
		status.RemoveDeadInfo(1);

		// Assert
		Assert.Empty(status.DeadPlayerInfo);
	}

	[Fact]
	public void ReplaceDeadReason_UpdatesReasonWhenPlayerExists()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		var deadPlayer = new Mock<PlayerControl>();
		deadPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		var killer = new Mock<PlayerControl>();
		killer.SetupGet(p => p.PlayerId).Returns((byte)2);

		status.AddDeadInfo(deadPlayer.Object, DeathReason.Kill, killer.Object);

		// Act
		status.ReplaceDeadReason(1, ExtremeShipStatus.PlayerStatus.Retaliate);

		// Assert
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Retaliate, status.DeadPlayerInfo[1].Reason);
		Assert.Equal(killer.Object, status.DeadPlayerInfo[1].Killer);
	}

	[Fact]
	public void ReplaceDeadReason_DoesNothingWhenPlayerDoesNotExist()
	{
		// Arrange
		var status = new ExtremeShipStatus();

		// Act
		status.ReplaceDeadReason(99, ExtremeShipStatus.PlayerStatus.Retaliate);

		// Assert
		Assert.False(status.DeadPlayerInfo.ContainsKey(99));
	}

	[Fact]
	public void RpcReplaceDeadReason_CallsRpcAndReplacesReason()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		var deadPlayer = new Mock<PlayerControl>();
		deadPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		var killer = new Mock<PlayerControl>();
		killer.SetupGet(p => p.PlayerId).Returns((byte)2);

		status.AddDeadInfo(deadPlayer.Object, DeathReason.Kill, killer.Object);

		// Act
		status.RpcReplaceDeadReason(1, ExtremeShipStatus.PlayerStatus.Martyrdom);

		// Assert
		Assert.Equal(ExtremeShipStatus.PlayerStatus.Martyrdom, status.DeadPlayerInfo[1].Reason);
	}

	[Fact]
	public void AddGlobalActionRole_Assassin_SetsIsAssignAssassinTrue()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		Assert.False(status.IsAssassinAssign);

		var dummyRole = new DummySingleRole(ExtremeRoleId.Assassin);

		// Act
		status.AddGlobalActionRole(dummyRole);

		// Assert
		Assert.True(status.IsAssassinAssign);
	}

	[Fact]
	public void AddGlobalActionRole_OtherRole_DoesNotSetIsAssignAssassinTrue()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		Assert.False(status.IsAssassinAssign);

		var dummyRole = new DummySingleRole(ExtremeRoleId.Sheriff);

		// Act
		status.AddGlobalActionRole(dummyRole);

		// Assert
		Assert.False(status.IsAssassinAssign);
	}

	[Fact]
	public void Version_AddAndTryGetPlayerVersion_WorksCorrectly()
	{
		// Arrange
		var status = new ExtremeShipStatus();
		int clientId = 10;
		var version = new Version(1, 2, 3, 4);

		// Act & Assert 1: Add by Version object
		status.AddPlayerVersion(clientId, version);
		bool success = status.TryGetPlayerVersion(clientId, out var resultVersion);
		Assert.True(success);
		Assert.Equal(version, resultVersion);

		// Act & Assert 2: Add by int parameters
		int clientId2 = 20;
		status.AddPlayerVersion(clientId2, 2, 3, 4, 5);
		bool success2 = status.TryGetPlayerVersion(clientId2, out var resultVersion2);
		Assert.True(success2);
		Assert.Equal(new Version(2, 3, 4, 5), resultVersion2);

		// Act & Assert 3: TryGet non-existent client
		bool success3 = status.TryGetPlayerVersion(999, out var resultVersion3);
		Assert.False(success3);
		Assert.Null(resultVersion3);
	}

	[Fact]
	public void Win_StateManagement_WorksCorrectly()
	{
		// Arrange
		var status = new ExtremeShipStatus();

		// SetGameOverReason & EndReason
		status.SetGameOverReason((GameOverReason)42);
		Assert.Equal((GameOverReason)42, status.EndReason);

		// SetWinControlId & WinGameControlId
		status.SetWinControlId(7);
		Assert.Equal(7, status.WinGameControlId);

		// SetDisableWinCheck & IsDisableWinCheck
		status.SetDisableWinCheck(true);
		Assert.True(status.IsDisableWinCheck);

		// AddWinner with PlayerControl
		var mockPlayer = new Mock<PlayerControl>();
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero).Object;
		mockPlayer.SetupGet(p => p.Data).Returns(mockData);

		status.AddWinner(mockPlayer.Object);
		Assert.Single(status.GetPlusWinner());
		Assert.Same(mockData, status.GetPlusWinner()[0]);

		// AddWinner with NetworkedPlayerInfo
		var mockData2 = new Mock<NetworkedPlayerInfo>(IntPtr.Zero).Object;
		status.AddWinner(mockData2);
		Assert.Equal(2, status.GetPlusWinner().Count);
		Assert.Same(mockData2, status.GetPlusWinner()[1]);

		// SetPlusWinner
		var newWinners = new List<NetworkedPlayerInfo> { mockData2 };
		status.SetPlusWinner(newWinners);
		Assert.Single(status.GetPlusWinner());
		Assert.Same(mockData2, status.GetPlusWinner()[0]);
	}
}