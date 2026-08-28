using System;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

public sealed class NeutralAliveWinCheckerTests
{
    public NeutralAliveWinCheckerTests()
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

    private static void AddNeutralTeam(PlayerStatistics stats, NeutralSeparateTeam team, int controlId, int count)
    {
        FieldInfo? builderField = typeof(PlayerStatistics).GetField("builder", BindingFlags.NonPublic | BindingFlags.Instance);
        object? builder = builderField?.GetValue(stats);
        FieldInfo? containerField = builder?.GetType().GetField("neutralTeam", BindingFlags.NonPublic | BindingFlags.Instance);
        object? container = containerField?.GetValue(builder);
        MethodInfo? addMethod = container?.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < count; i++)
        {
            addMethod?.Invoke(container, new object[] { team, controlId });
        }
    }

    [Fact]
    public void TryCheckGameEnd_MultipleNeutralTeams_ReturnsFalse()
    {
        PlayerStatistics stats = new PlayerStatistics();
        AddNeutralTeam(stats, NeutralSeparateTeam.Jackal, 1, 1);
        AddNeutralTeam(stats, NeutralSeparateTeam.Lover, 2, 1);

        NeutralAliveWinChecker checker = new NeutralAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }

    [Fact]
    public void TryCheckGameEnd_AliceOutnumbersOthers_ReturnsTrueWithAliceReason()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        AddNeutralTeam(stats, NeutralSeparateTeam.Alice, 10, 1);

        NeutralAliveWinChecker checker = new NeutralAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.AliceKillAllOther, reason);
    }

    [Fact]
    public void TryCheckGameEnd_JackalOutnumbersOthers_NoImpostors_ReturnsTrueWithJackalReason()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        AddNeutralTeam(stats, NeutralSeparateTeam.Jackal, 11, 1);

        NeutralAliveWinChecker checker = new NeutralAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.JackalKillAllOther, reason);
    }
}
