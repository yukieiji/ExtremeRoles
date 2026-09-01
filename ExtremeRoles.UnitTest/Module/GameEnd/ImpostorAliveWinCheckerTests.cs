using System.Collections.Generic;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class ImpostorAliveWinCheckerTests
{
    public ImpostorAliveWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    private static void SetupLastDeathReasonMock(DeathReason deathReason)
    {
        var mockGetter = new Mock<MockGameDataget_LastDeathReasonHelper>();
        mockGetter.Setup(h => h.Invoke()).Returns(deathReason);
        MockGameDataget_LastDeathReasonHelper.Instance = mockGetter.Object;
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorsEqualOrOutnumberOthers_Exile_ReturnsTrueWithVoteReason()
    {
        SetupLastDeathReasonMock(DeathReason.Exile);

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(2);
        mockStats.SetupGet(s => s.TotalAlive).Returns(3);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        ImpostorAliveWinChecker checker = new ImpostorAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.ImpostorsByVote, reason);
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorsEqualOrOutnumberOthers_Kill_ReturnsTrueWithKillReason()
    {
        SetupLastDeathReasonMock(DeathReason.Kill);

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(1);
        mockStats.SetupGet(s => s.TotalAlive).Returns(2);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        ImpostorAliveWinChecker checker = new ImpostorAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.ImpostorsByKill, reason);
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorsFewerThanOthers_ReturnsFalse()
    {
        SetupLastDeathReasonMock(DeathReason.Kill);

        var mockStats = new Mock<IPlayerStatistics>();
        mockStats.SetupGet(s => s.LiberalMilitantAlive).Returns(0);
        mockStats.SetupGet(s => s.TeamImpostorAlive).Returns(1);
        mockStats.SetupGet(s => s.TotalAlive).Returns(3);
        mockStats.SetupGet(s => s.SeparatedNeutralAlive).Returns(new Dictionary<NeutralSeparateTeamContainer.NeutralTeam, int>());

        ImpostorAliveWinChecker checker = new ImpostorAliveWinChecker(mockStats.Object);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
