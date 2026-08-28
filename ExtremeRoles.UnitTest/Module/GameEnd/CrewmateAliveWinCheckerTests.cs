using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class CrewmateAliveWinCheckerTests
{
    public CrewmateAliveWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(target, value);
    }

    [Fact]
    public void TryCheckGameEnd_CrewmatesAliveAndNoThreats_ReturnsTrue()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.TeamCrewmateAlive), 2);
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);

        CrewmateAliveWinChecker checker = new CrewmateAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.CrewmatesByVote, reason);
    }

    [Fact]
    public void TryCheckGameEnd_OnlyNeutralsAliveNoCrewmatesNoThreats_ReturnsTrue()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.TeamCrewmateAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);
        SetProperty(stats, nameof(PlayerStatistics.TeamNeutralAlive), 2);

        CrewmateAliveWinChecker checker = new CrewmateAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.CrewmatesByVote, reason);
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorAlive_ReturnsFalse()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.TeamCrewmateAlive), 1);
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 1);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);

        CrewmateAliveWinChecker checker = new CrewmateAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
