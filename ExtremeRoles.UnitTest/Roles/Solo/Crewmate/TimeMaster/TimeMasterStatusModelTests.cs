using ExtremeRoles.Roles.Solo.Crewmate.TimeMaster;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.TimeMaster;

public sealed class TimeMasterStatusModelTests
{
    [Fact]
    public void RewindSecond_Property_ReturnsValuePassedToConstructor()
    {
        // Arrange
        float expectedRewindSecond = 5.0f;
        var status = new TimeMasterStatusModel(expectedRewindSecond);

        // Act
        float result = status.RewindSecond;

        // Assert
        Assert.Equal(expectedRewindSecond, result);
    }

    [Fact]
    public void IsShieldOn_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var status = new TimeMasterStatusModel(5.0f);

        // Act
        status.IsShieldOn = true;
        bool result = status.IsShieldOn;

        // Assert
        Assert.True(result);
    }
}
