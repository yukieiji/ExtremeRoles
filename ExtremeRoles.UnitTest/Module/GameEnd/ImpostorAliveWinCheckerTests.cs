using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

public sealed class ImpostorAliveWinCheckerTests
{
    public ImpostorAliveWinCheckerTests()
    {
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

        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 2);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 3);

        ImpostorAliveWinChecker checker = new ImpostorAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.ImpostorsByVote, reason);
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorsEqualOrOutnumberOthers_Kill_ReturnsTrueWithKillReason()
    {
        SetupLastDeathReasonMock(DeathReason.Kill);

        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 1);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);

        ImpostorAliveWinChecker checker = new ImpostorAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.ImpostorsByKill, reason);
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorsFewerThanOthers_ReturnsFalse()
    {
        SetupLastDeathReasonMock(DeathReason.Kill);

        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 1);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 3);

        ImpostorAliveWinChecker checker = new ImpostorAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(target, value);
    }
}
