using System.Collections.Generic;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class LiberalAliveWinCheckerTests
{
    public LiberalAliveWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void TryCheckGameEnd_OnlyLiberalAlive_ReturnsTrue()
    {
        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamCrewmateAlive).Returns(0);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(0);
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.TeamLiberalAlive).Returns(2);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        LiberalAliveWinChecker checker = new LiberalAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.LiberalRevolution, reason);
    }

    [Fact]
    public void TryCheckGameEnd_CrewmatesAlive_ReturnsFalse()
    {
        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamCrewmateAlive).Returns(1);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(0);
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.TeamLiberalAlive).Returns(1);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        LiberalAliveWinChecker checker = new LiberalAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
