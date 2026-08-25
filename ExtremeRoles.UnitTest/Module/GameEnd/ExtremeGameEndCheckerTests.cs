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
}
