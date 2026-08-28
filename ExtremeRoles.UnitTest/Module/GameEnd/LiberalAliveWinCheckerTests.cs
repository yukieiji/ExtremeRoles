using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

public sealed class LiberalAliveWinCheckerTests
{
    public LiberalAliveWinCheckerTests()
    {
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(target, value);
    }

    [Fact]
    public void TryCheckGameEnd_OnlyLiberalAlive_ReturnsTrue()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.TeamCrewmateAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);
        SetProperty(stats, nameof(PlayerStatistics.TeamLiberalAlive), 2);

        LiberalAliveWinChecker checker = new LiberalAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.LiberalRevolution, reason);
    }

    [Fact]
    public void TryCheckGameEnd_CrewmatesAlive_ReturnsFalse()
    {
        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.TeamCrewmateAlive), 1);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 2);
        SetProperty(stats, nameof(PlayerStatistics.TeamLiberalAlive), 1);

        LiberalAliveWinChecker checker = new LiberalAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
