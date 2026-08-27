using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class TaskEndCheckerTests
{
    public TaskEndCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void TryCheckGameEnd_CompletedTasksEqualTotalTasks_ReturnsTrue()
    {
        var mockData = new Mock<GameData>();
        mockData.SetupGet(d => d.TotalTasks).Returns(10);
        mockData.SetupGet(d => d.CompletedTasks).Returns(10);

        var mockHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockHelper.Setup(h => h.Invoke()).Returns(mockData.Object);
        MockGameDataget_InstanceHelper.Instance = mockHelper.Object;

        TaskEndChecker checker = new TaskEndChecker();

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.CrewmatesByTask, reason);
    }

    [Fact]
    public void TryCheckGameEnd_CompletedTasksLessThanTotalTasks_ReturnsFalse()
    {
        var mockData = new Mock<GameData>();
        mockData.SetupGet(d => d.TotalTasks).Returns(10);
        mockData.SetupGet(d => d.CompletedTasks).Returns(5);

        var mockHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockHelper.Setup(h => h.Invoke()).Returns(mockData.Object);
        MockGameDataget_InstanceHelper.Instance = mockHelper.Object;

        TaskEndChecker checker = new TaskEndChecker();

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
