using System;
using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class ExtremeGameEndCheckerTests
{
    public ExtremeGameEndCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
        SetupGameData();
    }

    private static void SetupGameData()
    {
        var mockData = new Mock<GameData>();
        var mockDataHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockDataHelper.Setup(h => h.Invoke()).Returns(mockData.Object);
        MockGameDataget_InstanceHelper.Instance = mockDataHelper.Object;

        var mockPlayers = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
        mockPlayers.SetupGet(p => p.Count).Returns(0);
        mockData.SetupGet(d => d.AllPlayers).Returns(mockPlayers.Object);
    }

    [Fact]
    public void Check_WhenCheckerReturnsGameEnd_CallsGameIsEndAndCleanUp()
    {
        var mockChecker = new Mock<IGameEndChecker>();
        GameOverReason expectedReason = GameOverReason.CrewmatesByTask;
        mockChecker.Setup(c => c.TryCheckGameEnd(out expectedReason)).Returns(true);
        mockChecker.Setup(c => c.CleanUp()).Verifiable();

        ExtremeGameEndChecker gameEndChecker = (ExtremeGameEndChecker)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ExtremeGameEndChecker));

        FieldInfo? checkersField = typeof(ExtremeGameEndChecker).GetField("checkers", BindingFlags.NonPublic | BindingFlags.Instance);
        checkersField?.SetValue(gameEndChecker, new IGameEndChecker[] { mockChecker.Object });

        FieldInfo? statsField = typeof(ExtremeGameEndChecker).GetField("statistics", BindingFlags.NonPublic | BindingFlags.Instance);
        statsField?.SetValue(gameEndChecker, new PlayerStatistics());

        gameEndChecker.Check();

        mockChecker.Verify(c => c.CleanUp(), Times.Once());
    }

    [Fact]
    public void Check_WhenCheckerReturnsFalse_DoesNotCallCleanUp()
    {
        var mockChecker = new Mock<IGameEndChecker>();
        GameOverReason dummyReason = GameOverReason.CrewmatesByTask;
        mockChecker.Setup(c => c.TryCheckGameEnd(out dummyReason)).Returns(false);

        ExtremeGameEndChecker gameEndChecker = (ExtremeGameEndChecker)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ExtremeGameEndChecker));

        FieldInfo? checkersField = typeof(ExtremeGameEndChecker).GetField("checkers", BindingFlags.NonPublic | BindingFlags.Instance);
        checkersField?.SetValue(gameEndChecker, new IGameEndChecker[] { mockChecker.Object });

        FieldInfo? statsField = typeof(ExtremeGameEndChecker).GetField("statistics", BindingFlags.NonPublic | BindingFlags.Instance);
        statsField?.SetValue(gameEndChecker, new PlayerStatistics());

        gameEndChecker.Check();

        mockChecker.Verify(c => c.CleanUp(), Times.Never());
    }
}
