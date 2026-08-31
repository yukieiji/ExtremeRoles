using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class SpecialRoleWinCheckerTests
{
    public SpecialRoleWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        SetupAmongUsClientAndShipState();
    }

    private static void SetupAmongUsClientAndShipState()
    {
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), BindingFlags.Public | BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeShipStatus());
        }

        var mockClient = new Mock<AmongUsClient>();
        var mockHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
        mockHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
        MockAmongUsClientget_InstanceHelper.Instance = mockHelper.Object;

        var mockWriter = new Mock<Hazel.MessageWriter>(IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<Hazel.SendOption>(), It.IsAny<int>())).Returns(mockWriter.Object);

        var mockLocalPlayer = new Mock<PlayerControl>();
        mockLocalPlayer.SetupGet(p => p.NetId).Returns(1u);
        var mockPlayerHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        MockPlayerControlget_LocalPlayerHelper.Instance = mockPlayerHelper.Object;
        mockPlayerHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
    }

    [Fact]
    public void TryCheckGameEnd_NoSpecialWinRoles_ReturnsFalse()
    {
        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.SpecialWinCheckRoleAlive).Returns(new Dictionary<int, IWinChecker>());

        SpecialRoleWinChecker checker = new SpecialRoleWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }

    [Fact]
    public void TryCheckGameEnd_SpecialWinRoleWins_ReturnsTrue()
    {
        var mockWinChecker = new Mock<IWinChecker>();
        mockWinChecker.Setup(w => w.IsWin(It.IsAny<IPlayerStatistics>())).Returns(true);
        mockWinChecker.SetupGet(w => w.Reason).Returns(RoleGameOverReason.TaskMasterGoHome);

        var dict = new Dictionary<int, IWinChecker>
        {
            { 1, mockWinChecker.Object }
        };

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.SpecialWinCheckRoleAlive).Returns(dict);

        SpecialRoleWinChecker checker = new SpecialRoleWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.TaskMasterGoHome, reason);
    }
}
