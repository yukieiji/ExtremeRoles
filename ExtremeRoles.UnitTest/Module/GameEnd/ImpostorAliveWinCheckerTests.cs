using System;
using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class ImpostorAliveWinCheckerTests
{
    public ImpostorAliveWinCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
        SetupGameData();
    }

    private static void SetupGameData()
    {
        var mockData = new Mock<GameData>();
        var mockHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockHelper.Setup(h => h.Invoke()).Returns(mockData.Object);
        MockGameDataget_InstanceHelper.Instance = mockHelper.Object;
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(target, value);
    }

    [Fact]
    public void TryCheckGameEnd_ImpostorsEqualOrOutnumberOthers_Exile_ReturnsTrueWithVoteReason()
    {
        var field = typeof(GameData).GetField("<LastDeathReason>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, DeathReason.Exile);

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
        var field = typeof(GameData).GetField("<LastDeathReason>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, DeathReason.Kill);

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
        var field = typeof(GameData).GetField("<LastDeathReason>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, DeathReason.Kill);

        PlayerStatistics stats = new PlayerStatistics();
        SetProperty(stats, nameof(PlayerStatistics.LiberalMilitantAlive), 0);
        SetProperty(stats, nameof(PlayerStatistics.TeamImpostorAlive), 1);
        SetProperty(stats, nameof(PlayerStatistics.TotalAlive), 3);

        ImpostorAliveWinChecker checker = new ImpostorAliveWinChecker(stats);

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
