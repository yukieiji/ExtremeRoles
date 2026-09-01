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
public sealed class NeutralAliveWinCheckerTests
{
    public NeutralAliveWinCheckerTests()
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
    public void TryCheckGameEnd_MultipleNeutralTeams_ReturnsFalse()
    {
        var dict = new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>
        {
            { new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Jackal, 1), 1 },
            { new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Lover, 2), 1 }
        };

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(dict);
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);

        NeutralAliveWinChecker checker = new NeutralAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }

    [Fact]
    public void TryCheckGameEnd_AliceOutnumbersOthers_ReturnsTrueWithAliceReason()
    {
        var dict = new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>
        {
            { new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Alice, 10), 1 }
        };

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(dict);

        NeutralAliveWinChecker checker = new NeutralAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.AliceKillAllOther, reason);
    }

    [Fact]
    public void TryCheckGameEnd_JackalOutnumbersOthers_NoImpostors_ReturnsTrueWithJackalReason()
    {
        var dict = new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>
        {
            { new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Jackal, 11), 1 }
        };

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(0);
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(dict);

        NeutralAliveWinChecker checker = new NeutralAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.JackalKillAllOther, reason);
    }
}
