using ExtremeRoles.Roles.Solo.Crewmate.Fencer;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Fencer;

public sealed class FencerStatusModelTests
{
    [Fact]
    public void Constructor_InitializesDefaultValues()
    {
        // Arrange
        float expectedMaxTime = 10.0f;

        // Act
        var status = new FencerStatusModel(expectedMaxTime);

        // Assert
        Assert.False(status.IsCounter);
        Assert.Equal(0.0f, status.Timer);
        Assert.Equal(expectedMaxTime, status.MaxTime);
        Assert.False(status.CanKill);
    }

    [Fact]
    public void IsCounter_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var status = new FencerStatusModel(5.0f);

        // Act
        status.IsCounter = true;
        var result = status.IsCounter;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Timer_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var status = new FencerStatusModel(5.0f);
        float expectedTimer = 3.5f;

        // Act
        status.Timer = expectedTimer;
        var result = status.Timer;

        // Assert
        Assert.Equal(expectedTimer, result);
    }

    [Fact]
    public void CanKill_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var status = new FencerStatusModel(5.0f);

        // Act
        status.CanKill = true;
        var result = status.CanKill;

        // Assert
        Assert.True(result);
    }
}
