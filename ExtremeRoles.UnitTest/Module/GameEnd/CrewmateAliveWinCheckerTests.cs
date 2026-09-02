using System.Collections.Generic;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class CrewmateAliveWinCheckerTests
{
    public CrewmateAliveWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void TryCheckGameEnd_CrewmatesAliveAndNoThreats_ReturnsTrue()
    {
        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamCrewmateAlive).Returns(2);
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(0);
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        CrewmateAliveWinChecker checker = new CrewmateAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.CrewmatesByVote, reason);
    }

    [Fact]
    public void TryCheckGameEnd_OnlyNeutralsAliveNoCrewmatesNoThreats_ReturnsTrue()
    {
        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamCrewmateAlive).Returns(0);
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(0);
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.TeamNeutralAlive).Returns(2);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        CrewmateAliveWinChecker checker = new CrewmateAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.CrewmatesByVote, reason);
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorAlive_ReturnsFalse()
    {
        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.TeamCrewmateAlive).Returns(1);
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(1);
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        CrewmateAliveWinChecker checker = new CrewmateAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
