using System;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

public sealed class SpecialRoleWinCheckerTests
{
    public SpecialRoleWinCheckerTests()
    {
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

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(target, value);
    }

    [Fact]
    public void TryCheckGameEnd_NoSpecialWinRoles_ReturnsFalse()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SpecialRoleWinChecker checker = new SpecialRoleWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }

    [Fact]
    public void TryCheckGameEnd_SpecialWinRoleWins_ReturnsTrue()
    {
        PlayerStatistics stats = new PlayerStatistics();

        var mockWinChecker = new Mock<IWinChecker>();
        mockWinChecker.Setup(w => w.IsWin(It.IsAny<PlayerStatistics>())).Returns(true);
        mockWinChecker.SetupGet(w => w.Reason).Returns(RoleGameOverReason.TaskMasterGoHome);

        FieldInfo? field = typeof(PlayerStatistics).GetField("specialWinCheckRoleAlive", BindingFlags.NonPublic | BindingFlags.Instance);
        var dict = (System.Collections.Generic.Dictionary<int, IWinChecker>)field!.GetValue(stats)!;
        dict.Add(1, mockWinChecker.Object);

        SpecialRoleWinChecker checker = new SpecialRoleWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.TaskMasterGoHome, reason);
    }
}
