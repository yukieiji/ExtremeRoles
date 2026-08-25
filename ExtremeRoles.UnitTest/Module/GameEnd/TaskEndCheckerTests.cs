using System.Reflection;
using ExtremeRoles.Module.GameEnd;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class TaskEndCheckerTests
{
    public TaskEndCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    private static void SetField<T>(object target, string fieldName, T value)
    {
        FieldInfo? field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    [Fact]
    public void TryCheckGameEnd_CompletedTasksEqualTotalTasks_ReturnsTrue()
    {
        TaskEndChecker checker = new TaskEndChecker();
        FieldInfo? dataField = typeof(TaskEndChecker).GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);
        object? gameData = dataField?.GetValue(checker);

        if (gameData != null)
        {
            SetField(gameData, "TotalTasks", 10);
            SetField(gameData, "CompletedTasks", 10);
        }

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal(GameOverReason.CrewmatesByTask, reason);
    }

    [Fact]
    public void TryCheckGameEnd_CompletedTasksLessThanTotalTasks_ReturnsFalse()
    {
        TaskEndChecker checker = new TaskEndChecker();
        FieldInfo? dataField = typeof(TaskEndChecker).GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);
        object? gameData = dataField?.GetValue(checker);

        if (gameData != null)
        {
            SetField(gameData, "TotalTasks", 10);
            SetField(gameData, "CompletedTasks", 5);
        }

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
